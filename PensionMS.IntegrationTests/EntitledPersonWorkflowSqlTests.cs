using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.IdentityModel.Tokens;
using Moq;
using PensionMS.Core.Constants;
using PensionMS.Core.DTOs.Documents;
using PensionMS.Core.DTOs.EntitledPersons;
using PensionMS.Core.DTOs.SalaryRecipients;
using PensionMS.Core.Entities;
using PensionMS.Core.Interfaces;
using PensionMS.Core.Time;
using PensionMS.API.Controllers;
using PensionMS.API.Helpers;
using PensionMS.API.Middleware;
using PensionMS.API.Security;
using PensionMS.Infrastructure.Data;
using PensionMS.Infrastructure.Repositories;
using PensionMS.Infrastructure.Services.Documents;
using PensionMS.Infrastructure.Services.Payroll;
using PensionMS.Infrastructure.Services.Pension.Beneficiaries;
using PensionMS.Infrastructure.Services.Pension.Entitlements;
using PensionMS.Infrastructure.Services.Pension.RetirementFile;
using PensionMS.Infrastructure.Services.Security;
using PensionMS.Testing;

namespace PensionMS.IntegrationTests;

// Real SQL, unit of work, eligibility, compliance, document and review services.
// Only physical storage and external notification/audit sinks are replaced.
public class EntitledPersonWorkflowSqlTests(SqlTestDatabase database) : IClassFixture<SqlTestDatabase>
{
    [Fact]
    public async Task HttpPipeline_BindsMultipartProof_RejectsEditOnlyLifecycleAndForeignMissingDocumentReads()
    {
        var own = await SeedAsync();
        var foreign = await SeedAsync();
        int branchId, missingId;
        var codes = new[] { "EntitledPerson.Lifecycle", "EntitledPerson.Edit", "MissingDocument.View",
            "MissingDocument.Add", "MissingDocument.Edit", "MissingDocument.Delete" };
        await using (var db = database.OpenContext())
        {
            branchId = (await db.RetirementFiles.FindAsync(own.File))!.PensionBranchID;
            var lifecycle = new Role { RoleName = "lifecycle-" + own.Person };
            var editor = new Role { RoleName = "editor-" + own.Person };
            foreach (var code in codes)
            {
                var permission = await db.Permissions.SingleOrDefaultAsync(p => p.Code == code) ?? new Permission { Code = code };
                lifecycle.RolePermissions.Add(new RolePermission { Permission = permission });
                if (code == "EntitledPerson.Edit") editor.RolePermissions.Add(new RolePermission { Permission = permission });
            }
            db.UserRoleAssignments.AddRange(new UserRoleAssignment { UserId = own.Actor, Role = lifecycle, BranchId = branchId },
                new UserRoleAssignment { UserId = own.Reviewer, Role = editor, BranchId = branchId });
            var missing = new MissingDocument { FileID = foreign.File, EntitledID = foreign.Person,
                DocumentTypeID = DocumentTypeConstants.NationalId, Status = MissingDocumentStatusConstants.Pending };
            db.Add(missing);
            await db.SaveChangesAsync();
            missingId = missing.MissingDocumentID;
        }
        const string key = "PensionMS-entitled-http-test-only-key-123456789";
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Testing" });
        builder.Logging.ClearProviders();
        builder.WebHost.ConfigureKestrel(k => k.Listen(IPAddress.Loopback, 0));
        builder.Services.AddScoped(_ => database.OpenContext());
        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
        builder.Services.AddScoped<IAccessiblePensionRepository, AccessiblePensionRepository>();
        builder.Services.AddScoped<IScopedPermissionResolver, ScopedPermissionResolver>();
        builder.Services.AddScoped<ICurrentUserContext, CurrentUserContext>();
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddSingleton<IAuthorizationHandler, PermissionHandler>();
        builder.Services.AddSingleton(Mock.Of<IImmutableAuditService>());
        builder.Services.AddSingleton(Mock.Of<INotificationService>());
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<IEntitlementEngine, EntitlementEngine>();
        builder.Services.AddScoped<IMissingDocumentService, MissingDocumentService>();
        builder.Services.AddScoped<IFileReviewService, FileReviewService>();
        builder.Services.AddScoped<IDocumentService, DocumentService>();
        builder.Services.AddScoped<IEntitledPersonService, EntitledPersonService>();
        var storage = new Mock<IFileStorageService>();
        storage.Setup(s => s.SaveFileAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string[]?>(), It.IsAny<long?>())).ReturnsAsync("documents/http-proof-" + own.Person + ".pdf");
        builder.Services.AddSingleton(storage.Object);
        builder.Services.AddControllers(o => o.ModelBinderProviders.Insert(0, new RemoteFileModelBinderProvider()))
            .AddApplicationPart(typeof(EntitledPersonsController).Assembly);
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(o =>
        {
            o.TokenValidationParameters = new TokenValidationParameters { ValidateIssuer = true, ValidIssuer = "test",
                ValidateAudience = true, ValidAudience = "test", ValidateLifetime = true, ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)), ClockSkew = TimeSpan.Zero };
            o.Events = new JwtBearerEvents { OnTokenValidated = ActiveUserValidation.ValidateAsync };
        });
        builder.Services.AddAuthorization(o =>
        {
            foreach (var code in codes) o.AddPolicy(code, p => p.RequireAuthenticatedUser().AddRequirements(new PermissionRequirement(code)));
        });
        await using var app = builder.Build();
        app.UseMiddleware<ExceptionHandlingMiddleware>();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        await app.StartAsync();
        using var client = new HttpClient { BaseAddress = new Uri(app.Services.GetRequiredService<IServer>()
            .Features.Get<IServerAddressesFeature>()!.Addresses.Single()) };
        string Token(int userId) => new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken("test", "test",
            [new Claim(ClaimTypes.NameIdentifier, userId.ToString()), new Claim("BranchId", branchId.ToString()), new Claim("IsHQ", "False")],
            expires: DateTime.UtcNow.AddMinutes(10), signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)), SecurityAlgorithms.HmacSha256)));
        void Authenticate(int userId) => client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token(userId));
        Authenticate(own.Actor);
        foreach (var path in new[] { $"by-file/{foreign.File}", $"auto-detected/by-file/{foreign.File}", missingId.ToString() })
        {
            using var denied = await client.GetAsync("/api/missing-documents/" + path);
            Assert.Equal(HttpStatusCode.NotFound, denied.StatusCode);
            var body = await denied.Content.ReadAsStringAsync();
            Assert.DoesNotContain("No service", body);
        }
        using (var response = await client.PostAsJsonAsync("/api/missing-documents", new { fileID = foreign.File, documentTypeID = 1 }))
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        using (var response = await client.PostAsJsonAsync("/api/missing-documents", new { fileID = own.File, entitledID = foreign.Person, documentTypeID = 1 }))
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        using (var response = await client.PutAsync($"/api/missing-documents/{missingId}/fulfill", null))
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        using (var response = await client.DeleteAsync($"/api/missing-documents/{missingId}"))
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        using (var json = await client.PutAsJsonAsync($"/api/entitled-persons/{own.Person}/reactivate", new { reactivationCode = ReactivationCodes.Divorced }))
            Assert.True(json.StatusCode is HttpStatusCode.UnsupportedMediaType or HttpStatusCode.BadRequest,
                await json.Content.ReadAsStringAsync());
        using (var form = new MultipartFormDataContent())
        {
            form.Add(new StringContent(ReactivationCodes.Divorced), "reactivationCode");
            var proof = new ByteArrayContent("%PDF-1.7 test proof"u8.ToArray());
            proof.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
            form.Add(proof, "document", "proof.pdf");
            using var response = await client.PutAsync($"/api/entitled-persons/{own.Person}/reactivate", form);
            Assert.True(response.IsSuccessStatusCode, await response.Content.ReadAsStringAsync());
        }
        Authenticate(own.Reviewer);
        using (var form = new MultipartFormDataContent())
        {
            form.Add(new StringContent("false"), "isEligible");
            form.Add(new StringContent("تغيير محظور"), "fullName");
            using var response = await client.PutAsync($"/api/entitled-persons/{own.Person}", form);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
        using (var form = new MultipartFormDataContent())
        {
            form.Add(new StringContent(EntitledStopCodes.AdminDecision), "stopCode");
            using var response = await client.PutAsync($"/api/entitled-persons/{own.Person}/stop", form);
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
        await using (var db = database.OpenContext())
        {
            var person = (await db.EntitledPersons.FindAsync(own.Person))!;
            Assert.True(person.IsActive);
            Assert.Equal("اسم متكرر", person.FullName);
            Assert.Equal(SocialStatusConstants.Divorced, person.SocialStatusID);
            Assert.True(await db.Documents.AnyAsync(d => d.EntitledID == own.Person && d.DocumentTypeID == DocumentTypeConstants.DivorceCertificate));
        }
        await app.StopAsync();
    }

    [Fact]
    public async Task DivorceUpload_AndLaterReevaluation_KeepCorrectSocialStateAndReviewHold()
    {
        var seed = await SeedAsync();
        using (var services = new Services(database.OpenContext()))
        {
            var result = await services.People.ReactivateAsync(seed.Person, new ReactivateEntitledPersonDto
            { ReactivationCode = ReactivationCodes.Divorced, Document = new Proof() }, seed.Actor);
            Assert.True(result.IsSuccess, result.Message);
            Assert.True((await services.Engine.ReEvaluateEntitlementStatusAsync(seed.Person)).IsSuccess);
        }
        await using var db = database.OpenContext();
        var person = await db.EntitledPersons.FindAsync(seed.Person);
        Assert.True(person!.IsActive);
        Assert.Equal(SocialStatusConstants.Divorced, person.SocialStatusID);
        Assert.Null(person.StopCode);
        Assert.Equal(PendingReviewStatus.Approved, person.PendingReviewStatus);
        var file = await db.RetirementFiles.FindAsync(seed.File);
        Assert.Equal(FileReviewStatus.PendingReview, file!.ReviewStatus);
        Assert.Equal(FileStatusConstants.Pending, file.StatusID);
        Assert.True(await db.PensionSuspensions.AnyAsync(s => s.FileID == seed.File && s.Status == SuspensionStatusConstants.Active));
        Assert.Contains(await db.Documents.Where(d => d.EntitledID == seed.Person).ToListAsync(),
            d => d.DocumentTypeID == DocumentTypeConstants.DivorceCertificate);
    }

    [Fact]
    public async Task ConcurrentReactivation_AllowsOnlyOneTransitionAndOneProof()
    {
        var seed = await SeedAsync();
        async Task<bool> Reactivate()
        {
            using var services = new Services(database.OpenContext());
            return (await services.People.ReactivateAsync(seed.Person, new ReactivateEntitledPersonDto
            { ReactivationCode = ReactivationCodes.Divorced, Document = new Proof() }, seed.Actor)).IsSuccess;
        }
        var results = await Task.WhenAll(Reactivate(), Reactivate());
        Assert.Single(results.Where(success => success));
        await using var db = database.OpenContext();
        Assert.Equal(1, await db.Documents.CountAsync(d => d.EntitledID == seed.Person && d.DocumentTypeID == DocumentTypeConstants.DivorceCertificate));
        Assert.True((await db.EntitledPersons.FindAsync(seed.Person))!.IsActive);
    }

    [Fact]
    public async Task SecondProofFailure_RollsBackSocialStateDocumentsReviewAndCleansFirstPhysicalUpload()
    {
        var seed = await SeedAsync();
        using (var services = new Services(database.OpenContext()))
        {
            services.Storage.SetupSequence(s => s.SaveFileAsync(It.IsAny<Stream>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string[]?>(), It.IsAny<long?>()))
                .ReturnsAsync("documents/first-proof.pdf")
                .ThrowsAsync(new IOException("simulated storage failure"));
            var result = await services.People.ReactivateAsync(seed.Person, new ReactivateEntitledPersonDto
            { ReactivationCode = ReactivationCodes.HusbandDeceased, Document = new Proof(), CivilServiceProof = new Proof() }, seed.Actor);
            Assert.False(result.IsSuccess);
            services.Storage.Verify(s => s.DeleteFileAsync("documents/first-proof.pdf"), Times.Once);
            services.Storage.Verify(s => s.DeleteFileAsync(It.Is<string>(p => p.StartsWith("existing/"))), Times.Never);
        }
        await using var db = database.OpenContext();
        var person = await db.EntitledPersons.FindAsync(seed.Person);
        Assert.False(person!.IsActive);
        Assert.Equal(SocialStatusConstants.Married, person.SocialStatusID);
        Assert.Equal(EntitledStopCodes.DaughterMarried, person.StopCode);
        Assert.Equal(PendingReviewStatus.Approved, person.PendingReviewStatus);
        Assert.Equal(FileReviewStatus.Approved, (await db.RetirementFiles.FindAsync(seed.File))!.ReviewStatus);
        Assert.DoesNotContain(await db.Documents.Where(d => d.EntitledID == seed.Person).ToListAsync(),
            d => d.DocumentTypeID is DocumentTypeConstants.DeathCertificate or DocumentTypeConstants.CivilServiceProof);
    }

    [Fact]
    public async Task DeathCertificateFailure_DoesNotPersistDeathOrDeactivateRecipients()
    {
        var seed = await SeedAsync(active: true);
        using (var services = new Services(database.OpenContext()))
        {
            services.Storage.Setup(s => s.SaveFileAsync(It.IsAny<Stream>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string[]?>(), It.IsAny<long?>()))
                .ThrowsAsync(new IOException("simulated storage failure"));
            Assert.False((await services.People.RecordDeathAsync(seed.Person,
                new RecordDeathDto { DeathDate = YemenTime.Today, DeathCertificate = new Proof() }, seed.Actor)).IsSuccess);
        }
        await using var db = database.OpenContext();
        var person = await db.EntitledPersons.FindAsync(seed.Person);
        Assert.False(person!.IsDeceased);
        Assert.True(person.IsActive);
        Assert.True((await db.SalaryRecipients.SingleAsync(r => r.EntitledID == seed.Person)).IsActive);
    }

    [Theory]
    [InlineData("reject")]
    [InlineData("delete")]
    [InlineData("expire")]
    public async Task IdentityChange_ReevaluatesEligibilityAndCompliance_WithoutRemovingReviewHold(string change)
    {
        var seed = await SeedAsync(active: true);
        using (var services = new Services(database.OpenContext()))
        {
            if (change == "reject")
                Assert.True((await services.Documents.ReviewAsync(seed.Identity, new ReviewDocumentDto
                { PendingReviewStatus = (int)PendingReviewStatus.Rejected, RejectionReason = "هوية غير صالحة" }, seed.Reviewer)).IsSuccess);
            else if (change == "delete")
                Assert.True((await services.Documents.DeleteAsync(seed.Identity, seed.Actor)).IsSuccess);
            else
                Assert.True((await services.Documents.UpdateAsync(seed.Identity, new UpdateDocumentDto
                { DocumentTypeID = DocumentTypeConstants.NationalId, ExpiryDate = YemenTime.Today.AddDays(-1) }, seed.Actor)).IsSuccess);
        }
        await using (var db = database.OpenContext())
        {
            Assert.False((await db.EntitledPersons.FindAsync(seed.Person))!.IsActive);
            Assert.False((await db.SalaryRecipients.SingleAsync(r => r.EntitledID == seed.Person)).IsActive);
            var file = await db.RetirementFiles.FindAsync(seed.File);
            Assert.True(file!.MissingDocumentsCount > 0);
            Assert.Equal(FileReviewStatus.PendingReview, file.ReviewStatus);
        }
        using (var services = new Services(database.OpenContext()))
            Assert.True((await services.Documents.UploadAsync(new UploadDocumentDto
            { DocumentTypeID = DocumentTypeConstants.NationalId, EntitledPersonID = seed.Person, File = new Proof() }, seed.Actor)).IsSuccess);
        await using (var db = database.OpenContext())
        {
            Assert.True((await db.EntitledPersons.FindAsync(seed.Person))!.IsActive);
            Assert.Equal(PendingReviewStatus.Approved, (await db.EntitledPersons.FindAsync(seed.Person))!.PendingReviewStatus);
            Assert.Equal(FileReviewStatus.PendingReview, (await db.RetirementFiles.FindAsync(seed.File))!.ReviewStatus);
            // Reassessment must not silently resume an ended payment instruction.
            Assert.False((await db.SalaryRecipients.SingleAsync(r => r.EntitledID == seed.Person)).IsActive);
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task SalaryRecipient_AllowsValidIdentityReplacementForAnInvalidHistoricalIdentity(bool expired)
    {
        var seed = await SeedAsync(active: true, recipient: false);
        await using (var db = database.OpenContext())
        {
            var doc = (await db.Documents.FindAsync(seed.Identity))!;
            doc.Checksum = Convert.ToHexString(SHA256.HashData("%PDF-1.7 test proof"u8)).ToLowerInvariant();
            if (expired) doc.ExpiryDate = YemenTime.Today.AddDays(-1);
            else doc.PendingReviewStatus = PendingReviewStatus.Rejected;
            await db.SaveChangesAsync();
        }
        using (var services = new Services(database.OpenContext()))
        {
            var result = await services.Recipients.CreateAsync(new CreateSalaryRecipientDto
            { FileID = seed.File, EntitledID = seed.Person, StartDate = DateTime.UtcNow, SharePercent = 100, NationalIdImage = new Proof() }, seed.Actor);
            Assert.True(result.IsSuccess, result.Message);
            services.Storage.Verify(s => s.SaveFileAsync(It.IsAny<Stream>(), It.IsAny<string>(),
                "documents", It.IsAny<string[]?>(), It.IsAny<long?>()), Times.Once);
        }
        await using var verify = database.OpenContext();
        Assert.Equal(2, await verify.Documents.CountAsync(d => d.EntitledID == seed.Person && d.DocumentTypeID == DocumentTypeConstants.NationalId));
        Assert.True((await verify.SalaryRecipients.SingleAsync(r => r.EntitledID == seed.Person)).IsActive);
        Assert.Equal(FileReviewStatus.PendingReview, (await verify.RetirementFiles.FindAsync(seed.File))!.ReviewStatus);
    }

    [Fact]
    public async Task FailedEligibilityRefresh_RollsBackTheDocumentReviewAndFileHoldTogether()
    {
        var seed = await SeedAsync(active: true);
        var engine = new Mock<IEntitlementEngine>();
        engine.Setup(e => e.ReEvaluateEntitlementStatusAsync(seed.Person))
            .ReturnsAsync(PensionMS.Core.DTOs.Common.ApiResponse<bool>.Fail("تعذر تحديث الأهلية"));
        using (var services = new Services(database.OpenContext(), engine.Object))
            await Assert.ThrowsAsync<InvalidOperationException>(() => services.Documents.ReviewAsync(seed.Identity,
                new ReviewDocumentDto { PendingReviewStatus = (int)PendingReviewStatus.Rejected, RejectionReason = "غير صالحة" }, seed.Reviewer));
        await using var db = database.OpenContext();
        Assert.Equal(PendingReviewStatus.Approved, (await db.Documents.FindAsync(seed.Identity))!.PendingReviewStatus);
        Assert.True((await db.EntitledPersons.FindAsync(seed.Person))!.IsActive);
        Assert.Equal(FileReviewStatus.Approved, (await db.RetirementFiles.FindAsync(seed.File))!.ReviewStatus);
        Assert.False(await db.PensionSuspensions.AnyAsync(s => s.FileID == seed.File && s.Status == SuspensionStatusConstants.Active));
    }

    [Fact]
    public async Task ManualActivation_UsesPersonIds_WhenTwoPeopleHaveTheSameName()
    {
        var seed = await SeedAsync(active: true, recipient: false);
        int otherId;
        await using (var db = database.OpenContext())
        {
            var person = (await db.EntitledPersons.FindAsync(seed.Person))!;
            person.IsActive = false;
            person.StopCode = EntitledStopCodes.MissingData;
            var identity = (await db.Documents.FindAsync(seed.Identity))!;
            identity.IsDeleted = true;
            var other = new EntitledPerson { FileID = seed.File, FullName = person.FullName, NationalID = "other-" + seed.Person,
                BirthDate = YemenTime.Today.AddYears(-20), RelationshipID = 2, HealthStatusID = HealthStatusConstants.Healthy,
                SocialStatusID = SocialStatusConstants.Single, IsActive = false, StopCode = EntitledStopCodes.MissingData,
                HasDisabilityException = true };
            db.Add(other);
            await db.SaveChangesAsync();
            otherId = other.EntitledID;
        }
        using (var services = new Services(database.OpenContext()))
            Assert.True((await services.People.ManualActivateAsync(seed.Person,
                new ManualActivateEntitledPersonDto { CreatePledgesForMissingDocs = true }, seed.Actor)).IsSuccess);
        await using var verify = database.OpenContext();
        var rows = await verify.MissingDocuments.Where(m => m.FileID == seed.File).ToListAsync();
        Assert.Contains(rows, m => m.EntitledID == seed.Person && m.DocumentTypeID == DocumentTypeConstants.NationalId && m.PledgeID.HasValue);
        Assert.DoesNotContain(rows, m => m.EntitledID == seed.Person && m.DocumentTypeID == DocumentTypeConstants.MedicalReport);
        Assert.DoesNotContain(rows, m => m.EntitledID == otherId && m.PledgeID.HasValue);
    }

    [Fact]
    public async Task ManualFulfillment_RequiresValidProof_AndRefreshesThePersistedComplianceCount()
    {
        var seed = await SeedAsync(active: true, recipient: false);
        int missingId;
        await using (var db = database.OpenContext())
        {
            var missing = new MissingDocument { FileID = seed.File, EntitledID = seed.Person,
                DocumentTypeID = DocumentTypeConstants.NationalId, Status = MissingDocumentStatusConstants.Pending };
            db.Add(missing);
            (await db.Documents.FindAsync(seed.Identity))!.ExpiryDate = YemenTime.Today.AddDays(-1);
            await db.SaveChangesAsync();
            missingId = missing.MissingDocumentID;
        }
        using (var services = new Services(database.OpenContext()))
            Assert.False((await services.Missing.MarkFulfilledAsync(missingId, seed.Actor)).IsSuccess);
        await using (var db = database.OpenContext())
        {
            Assert.Equal(MissingDocumentStatusConstants.Pending, (await db.MissingDocuments.FindAsync(missingId))!.Status);
            (await db.Documents.FindAsync(seed.Identity))!.ExpiryDate = YemenTime.Today;
            (await db.RetirementFiles.FindAsync(seed.File))!.MissingDocumentsCount = 99;
            await db.SaveChangesAsync();
        }
        using (var services = new Services(database.OpenContext()))
            Assert.True((await services.Missing.MarkFulfilledAsync(missingId, seed.Actor)).IsSuccess);
        await using var verify = database.OpenContext();
        Assert.Equal(MissingDocumentStatusConstants.Fulfilled, (await verify.MissingDocuments.FindAsync(missingId))!.Status);
        Assert.Equal(0, (await verify.RetirementFiles.FindAsync(seed.File))!.MissingDocumentsCount);
    }

    private async Task<Seed> SeedAsync(bool active = false, bool recipient = true)
    {
        await using var db = database.OpenContext();
        var key = Guid.NewGuid().ToString("N")[..12];
        var actor = new User { Username = "actor-" + key, FullName = "منشئ", PasswordHash = "test" };
        var reviewer = new User { Username = "review-" + key, FullName = "مراجع", PasswordHash = "test" };
        var file = new RetirementFile { FileNumber = key, PensionBranch = new PensionFundBranch { BranchName = key },
            StatusID = FileStatusConstants.Active, ReviewStatus = FileReviewStatus.Approved,
            Retiree = new Retiree { FullName = "متقاعد", NationalID = key, IsAlive = false,
                HealthStatusID = HealthStatusConstants.Deceased,
                ApprovalItem = new MinistryApprovalItem { FullName = "متقاعد", Batch = new MinistryApprovalBatch { ImportedByUser = actor, Status = "Test" } } } };
        var person = new EntitledPerson { RetirementFile = file, FullName = "اسم متكرر", NationalID = "person-" + key,
            BirthDate = YemenTime.Today.AddYears(-30), RelationshipID = 3, HealthStatusID = HealthStatusConstants.Healthy,
            SocialStatusID = active ? SocialStatusConstants.Divorced : SocialStatusConstants.Married,
            IsActive = active, StopCode = active ? null : EntitledStopCodes.DaughterMarried,
            PendingReviewStatus = PendingReviewStatus.Approved };
        Document Doc(int type, DocumentOwnerType owner) => new() { RetirementFile = file, DocumentTypeID = type,
            OwnerType = owner, UploadedByUser = actor, FileName = key + ".pdf", FilePath = "existing/" + Guid.NewGuid() + ".pdf",
            PendingReviewStatus = PendingReviewStatus.Approved };
        var identity = Doc(DocumentTypeConstants.NationalId, DocumentOwnerType.EntitledPerson);
        identity.EntitledPerson = person;
        var death = Doc(DocumentTypeConstants.DeathCertificate, DocumentOwnerType.Retiree);
        death.Retiree = file.Retiree;
        db.AddRange(person, reviewer, identity, death, Doc(DocumentTypeConstants.FileOriginals, DocumentOwnerType.RetirementFile),
            Doc(DocumentTypeConstants.InheritanceCert, DocumentOwnerType.RetirementFile));
        if (active)
        {
            var divorce = Doc(DocumentTypeConstants.DivorceCertificate, DocumentOwnerType.EntitledPerson);
            divorce.EntitledPerson = person;
            db.Add(divorce);
            if (recipient) db.Add(new SalaryRecipient { RetirementFile = file, EntitledPerson = person,
                RecipientType = "مستحق", StartDate = DateTime.UtcNow, SharePercent = 100, IsActive = true, PendingReviewStatus = PendingReviewStatus.Approved });
        }
        await db.SaveChangesAsync();
        return new(file.FileID, person.EntitledID, identity.DocumentID, actor.UserID, reviewer.UserID);
    }

    private record Seed(int File, int Person, int Identity, int Actor, int Reviewer);

    private sealed class Services : IDisposable
    {
        private readonly UnitOfWork _unit;
        public Mock<IFileStorageService> Storage { get; } = new();
        public EntitlementEngine Engine { get; }
        public MissingDocumentService Missing { get; }
        public DocumentService Documents { get; }
        public EntitledPersonService People { get; }
        public SalaryRecipientService Recipients { get; }
        public Services(PensionDbContext db, IEntitlementEngine? documentEngine = null)
        {
            _unit = new(db);
            var access = TestPensionAccess.All(_unit);
            Engine = new(_unit);
            var missing = Missing = new MissingDocumentService(_unit, Engine, NullLogger<MissingDocumentService>.Instance, access);
            var review = new FileReviewService(_unit, access, missing);
            Storage.Setup(s => s.SaveFileAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string[]?>(), It.IsAny<long?>())).ReturnsAsync(() => "documents/" + Guid.NewGuid() + ".pdf");
            Storage.Setup(s => s.DeleteFileAsync(It.IsAny<string>())).ReturnsAsync(true);
            Storage.Setup(s => s.GetFileUrl(It.IsAny<string>())).Returns((string p) => "/uploads/" + p);
            Documents = new(_unit, Storage.Object, documentEngine ?? Engine, review, access, Mock.Of<IImmutableAuditService>(), NullLogger<DocumentService>.Instance, missing);
            People = new(_unit, Storage.Object, Documents, Engine, missing, review, NullLogger<EntitledPersonService>.Instance, access);
            Recipients = new(_unit, Storage.Object, Documents, Engine, review, Mock.Of<INotificationService>(), access);
        }
        public void Dispose() => _unit.Dispose();
    }

    private sealed class Proof : IRemoteFile
    {
        public string FileName => "proof.pdf";
        public string ContentType => "application/pdf";
        public long Length => 18;
        public Stream OpenReadStream() => new MemoryStream("%PDF-1.7 test proof"u8.ToArray());
    }
}
