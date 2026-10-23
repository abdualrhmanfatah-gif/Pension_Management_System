using Microsoft.EntityFrameworkCore;
using Moq;
using PensionMS.Core.Constants;
using PensionMS.Core.DTOs.MissingDocuments;
using PensionMS.Core.Entities;
using PensionMS.Core.Interfaces;
using PensionMS.Core.Time;
using PensionMS.Core.Rules.Documents;
using PensionMS.Infrastructure.Data;
using PensionMS.Infrastructure.Services.Pension.RetirementFile;
using PensionMS.Infrastructure.Services.Payroll;
using PensionMS.Testing;

namespace PensionMS.Tests.Services;

public class RetirementFileEligibilityConsistencyTests
{
    [Theory]
    [InlineData("valid", true)]
    [InlineData("expires-today", true)]
    [InlineData("expired", false)]
    [InlineData("rejected", false)]
    [InlineData("deleted", false)]
    [InlineData("wrong-owner", false)]
    [InlineData("wrong-file", false)]
    public async Task ComplianceAndApproval_UseTheSameDocumentValidityAndOwner(string scenario, bool allowed)
    {
        var f = new RetirementLifecycleFixture();
        f.AddItem();
        f.RequiredDocuments.Add(new() { DocumentTypeID = 1, AgentID = 8 });
        f.Documents.Add(new Document
        {
            FileID = scenario == "wrong-file" ? 2 : 1,
            DocumentTypeID = 1, AgentID = scenario == "wrong-owner" ? 9 : 8,
            OwnerType = DocumentOwnerType.Agent,
            IsDeleted = scenario == "deleted",
            PendingReviewStatus = scenario == "rejected" ? PendingReviewStatus.Rejected : PendingReviewStatus.None,
            ExpiryDate = YemenTime.Today.AddDays(scenario == "expired" ? -1 : scenario == "expires-today" ? 0 : 1)
        });
        var compliance = RetirementFileDocumentRules.Summarize(f.RequiredDocuments, f.MissingDocuments,
            f.Documents.Where(d => d.FileID == 1), YemenTime.Today);
        Assert.Equal(allowed ? 0 : 1, compliance.MissingCount);
        Assert.Equal(allowed, (await f.Service.ApproveReviewAsync(1, 7)).IsSuccess);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Activation_RequiresTheExactRetireeOwner(bool matching)
    {
        var f = new RetirementLifecycleFixture();
        var item = f.AddItem();
        f.RequiredDocuments.Add(new() { DocumentTypeID = 1, RetireeID = 2 });
        f.Documents.Add(new Document { FileID = 1, DocumentTypeID = 1, RetireeID = matching ? 2 : 3 });
        Assert.Equal(matching, (await f.Service.ApproveItemAsync(item.ItemID, 7)).IsSuccess);
    }

    [Theory]
    [InlineData(DocumentTypeConstants.PledgeDocument, true)]
    [InlineData(DocumentTypeConstants.PledgeDocument, false)]
    [InlineData(DocumentTypeConstants.ExemptionDirective, true)]
    [InlineData(DocumentTypeConstants.ExemptionDirective, false)]
    public async Task Activation_RequiresTheExactDirectiveOwner(int type, bool matching)
    {
        var f = new RetirementLifecycleFixture();
        f.AddItem();
        var pledge = type == DocumentTypeConstants.PledgeDocument;
        f.RequiredDocuments.Add(new() { DocumentTypeID = type, PledgeID = pledge ? 10 : null,
            ExemptionID = pledge ? null : 11 });
        f.Documents.Add(new Document { FileID = 1, DocumentTypeID = type,
            PledgeID = pledge ? matching ? 10 : 20 : null,
            ExemptionID = pledge ? null : matching ? 11 : 21 });
        Assert.Equal(matching, (await f.Service.ApproveReviewAsync(1, 7)).IsSuccess);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Coverage_CannotReplaceItsOwnRejectedDirective(bool exemption)
    {
        var f = new RetirementLifecycleFixture();
        f.AddItem();
        var type = exemption ? DocumentTypeConstants.ExemptionDirective : DocumentTypeConstants.PledgeDocument;
        f.RequiredDocuments.Add(new() { DocumentTypeID = type, PledgeID = exemption ? null : 10,
            ExemptionID = exemption ? 11 : null });
        f.MissingDocuments.Add(new MissingDocument { FileID = 1, DocumentTypeID = type,
            PledgeID = exemption ? null : 10, ExemptionID = exemption ? 11 : null,
            Status = exemption ? MissingDocumentStatusConstants.Exempted : MissingDocumentStatusConstants.Pending });
        f.Pledges.Add(new DocumentPledge { FileID = 1, PledgeID = 10, Deadline = DateTime.UtcNow.AddDays(1) });
        f.Exemptions.Add(new DocumentExemption { FileID = 1, ExemptionID = 11 });
        f.Documents.Add(new Document { FileID = 1, DocumentTypeID = type,
            PledgeID = exemption ? null : 10, ExemptionID = exemption ? 11 : null,
            PendingReviewStatus = PendingReviewStatus.Rejected });
        Assert.False((await f.Service.ApproveReviewAsync(1, 7)).IsSuccess);
    }

    [Theory]
    [InlineData(false, false, true)]
    [InlineData(false, true, false)]
    [InlineData(true, false, true)]
    [InlineData(true, true, false)]
    public async Task Coverage_IsLimitedToTheMissingDocumentsOwner(bool exemption, bool wrongOwner, bool allowed)
    {
        var f = new RetirementLifecycleFixture();
        f.AddItem();
        f.RequiredDocuments.Add(new() { DocumentTypeID = 1, AgentID = 8 });
        f.MissingDocuments.Add(new MissingDocument
        {
            FileID = 1, DocumentTypeID = 1, AgentID = wrongOwner ? 9 : 8,
            Status = exemption ? MissingDocumentStatusConstants.Exempted : MissingDocumentStatusConstants.Pledged,
            PledgeID = exemption ? null : 10, ExemptionID = exemption ? 11 : null
        });
        f.Pledges.Add(new DocumentPledge { FileID = 1, PledgeID = 10, Deadline = DateTime.UtcNow.AddDays(1) });
        f.Exemptions.Add(new DocumentExemption { FileID = 1, ExemptionID = 11 });
        Assert.Equal(allowed, (await f.Service.ApproveReviewAsync(1, 7)).IsSuccess);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Activation_RejectsExpiredPledgeOrDeletedExemption(bool exemption)
    {
        var f = new RetirementLifecycleFixture();
        f.AddItem();
        f.RequiredDocuments.Add(new() { DocumentTypeID = 1, AgentID = 8 });
        f.MissingDocuments.Add(new MissingDocument { FileID = 1, DocumentTypeID = 1, AgentID = 8,
            Status = exemption ? MissingDocumentStatusConstants.Exempted : MissingDocumentStatusConstants.Pledged,
            PledgeID = exemption ? null : 10, ExemptionID = exemption ? 11 : null });
        f.Pledges.Add(new DocumentPledge { FileID = 1, PledgeID = 10, Deadline = DateTime.UtcNow.AddDays(-1) });
        f.Exemptions.Add(new DocumentExemption { FileID = 1, ExemptionID = 11, IsDeleted = true });
        Assert.False((await f.Service.ApproveReviewAsync(1, 7)).IsSuccess);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task EndDateToday_AllowsActivationButPreventsEnding(bool perItem)
    {
        var f = new RetirementLifecycleFixture();
        var item = f.AddItem();
        f.Entitlements[0].EndDate = YemenTime.TodayDateTime;
        var approved = perItem ? await f.Service.ApproveItemAsync(item.ItemID, 7) : await f.Service.ApproveReviewAsync(1, 7);
        Assert.True(approved.IsSuccess, approved.Message);
        Assert.Equal(FileStatusConstants.Active, f.File.StatusID);
        var status = new FileStatusService(f.Unit.Object, Mock.Of<IImmutableAuditService>(),
            Mock.Of<INotificationService>(), f.Service, TestPensionAccess.All(f.Unit.Object));
        Assert.False((await status.ChangeStatusAsync(1, FileStatusConstants.EntitlementEnded, "سبب", 7)).IsSuccess);
        f.Entitlements[0].EndDate = YemenTime.TodayDateTime.AddDays(-1);
        Assert.True((await status.ChangeStatusAsync(1, FileStatusConstants.EntitlementEnded, "سبب", 7)).IsSuccess);
        Assert.Equal(FileStatusConstants.EntitlementEnded, f.File.StatusID);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task EndDateYesterday_ApprovalDeterminesEndedStatus(bool perItem)
    {
        var f = new RetirementLifecycleFixture();
        var item = f.AddItem();
        f.Entitlements[0].EndDate = YemenTime.TodayDateTime.AddDays(-1);
        var approved = perItem ? await f.Service.ApproveItemAsync(item.ItemID, 7) : await f.Service.ApproveReviewAsync(1, 7);
        Assert.True(approved.IsSuccess, approved.Message);
        Assert.Equal(FileStatusConstants.EntitlementEnded, f.File.StatusID);
    }

    [Fact]
    public async Task FutureEntitlement_PreventsActivationAndEnding()
    {
        var f = new RetirementLifecycleFixture();
        f.AddItem();
        f.Entitlements[0].EffectiveDate = YemenTime.TodayDateTime.AddDays(1);
        Assert.False((await f.Service.ApproveReviewAsync(1, 7)).IsSuccess);
        f.File.StatusID = FileStatusConstants.Active;
        f.File.ReviewStatus = FileReviewStatus.Approved;
        var status = new FileStatusService(f.Unit.Object, Mock.Of<IImmutableAuditService>(),
            Mock.Of<INotificationService>(), f.Service, TestPensionAccess.All(f.Unit.Object));
        Assert.False((await status.ChangeStatusAsync(1, FileStatusConstants.EntitlementEnded, "سبب", 7)).IsSuccess);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Payroll_IncludesTheWholeEndDate_WithLiveOrApprovedData(bool snapshot)
    {
        var f = new RetirementLifecycleFixture();
        f.File.StatusID = FileStatusConstants.Active;
        f.File.ReviewStatus = FileReviewStatus.Approved;
        f.Entitlements[0].EndDate = YemenTime.TodayDateTime;
        if (snapshot)
            f.File.LastReviewedFileState = (await ApprovedPensionSnapshot.CaptureAsync(f.Unit.Object, 1, 2)).Serialize();
        var generated = await new PayrollGenerationService(f.Unit.Object).GenerateForBatchAsync(
            new SalaryPaymentBatch { BatchID = 1, BatchMonth = YemenTime.TodayDateTime }, DateTime.UtcNow, 7);
        Assert.Equal(1000m, Assert.Single(generated.GeneratedItems).Amount);
    }

    [Fact]
    public void CalendarDates_IgnoreKindAndIncludeBothBoundaryDays()
    {
        var day = new DateOnly(2026, 10, 22);
        foreach (var kind in new[] { DateTimeKind.Unspecified, DateTimeKind.Utc })
        {
            var entitlement = new FinancialEntitlement
            {
                EffectiveDate = DateTime.SpecifyKind(day.ToDateTime(TimeOnly.MaxValue), kind),
                EndDate = DateTime.SpecifyKind(day.ToDateTime(TimeOnly.MinValue), kind)
            };
            Assert.True(PensionEntitlementRules.EffectiveOn(day).Compile()(entitlement));
            Assert.True(PensionEntitlementRules.NotEndedOn(day).Compile()(entitlement));
            Assert.False(PensionEntitlementRules.AllEnded([entitlement], day));
            Assert.True(PensionEntitlementRules.AllEnded([entitlement], day.AddDays(1)));
            Assert.False(PensionEntitlementRules.EffectiveOn(day.AddDays(-1)).Compile()(entitlement));
        }
        Assert.False(PensionEntitlementRules.AllEnded([], day));
        Assert.False(PensionEntitlementRules.AllEnded([new FinancialEntitlement { IsDeleted = true,
            EndDate = day.ToDateTime(TimeOnly.MinValue).AddDays(-1) }], day));
        Assert.False(PensionEntitlementRules.EffectiveOn(day).Compile()(new FinancialEntitlement
            { IsDeleted = true, EffectiveDate = day.ToDateTime(TimeOnly.MinValue) }));
        var monthStart = new DateOnly(day.Year, day.Month, 1);
        var monthEnd = monthStart.AddMonths(1).AddDays(-1);
        var midMonth = new FinancialEntitlement { EffectiveDate = monthStart.AddDays(14).ToDateTime(TimeOnly.MinValue) };
        Assert.False(PensionEntitlementRules.EffectiveOn(monthStart).Compile()(midMonth));
        Assert.True(PensionEntitlementRules.EffectiveDuring(monthStart, monthEnd).Compile()(midMonth));
    }

    [Fact]
    public void EntitlementPredicates_TranslateToSqlWithoutConnecting()
    {
        using var db = new PensionDbContext(new DbContextOptionsBuilder<PensionDbContext>()
            .UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=PensionMS_ModelOnly;Integrated Security=True").Options);
        var day = new DateOnly(2026, 10, 22);
        var effectiveSql = db.FinancialEntitlements.Where(PensionEntitlementRules.EffectiveOn(day)).ToQueryString();
        var notEndedSql = db.FinancialEntitlements.Where(PensionEntitlementRules.NotEndedOn(day)).ToQueryString();
        Assert.Contains("CONVERT(date,", effectiveSql);
        Assert.Contains("EffectiveDate", effectiveSql);
        Assert.Contains("EndDate", effectiveSql);
        Assert.Contains("CONVERT(date,", notEndedSql);
        Assert.Contains("EndDate", notEndedSql);
    }
}
