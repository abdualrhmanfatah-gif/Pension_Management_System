using PensionMS.Testing;
using System.Linq.Expressions;
using Moq;
using PensionMS.Core.Constants;
using PensionMS.Core.Entities;
using PensionMS.Core.Interfaces;
using PensionMS.Infrastructure.Services.Pension.RetirementFile;

namespace PensionMS.Tests.Services;

internal sealed class RetirementLifecycleFixture
{
    public Mock<IUnitOfWork> Unit { get; } = new();
    public RetirementFile File { get; } = new()
    {
        FileID = 1, RetireeID = 2, StatusID = FileStatusConstants.Pending,
        PreviousStatusID = FileStatusConstants.Active, ReviewStatus = FileReviewStatus.PendingReview,
        ReviewRequestedAt = DateTime.UtcNow.AddMinutes(-1),
        Retiree = new Retiree { RetireeID = 2, FullName = "متقاعد", IsAlive = true }
    };
    public List<FileReviewItem> Items { get; } = [];
    public List<PensionSuspension> Holds { get; } = [];
    public List<FinancialEntitlement> Entitlements { get; } = [];
    public List<PaymentBook> Books { get; } = [];
    public List<Core.DTOs.MissingDocuments.AutoDetectedMissingDocumentDto> RequiredDocuments { get; } = [];
    public List<Document> Documents { get; } = [];
    public List<MissingDocument> MissingDocuments { get; } = [];
    public List<DocumentPledge> Pledges { get; } = [];
    public List<DocumentExemption> Exemptions { get; } = [];
    public FileReviewService Service { get; }

    public RetirementLifecycleFixture()
    {
        var files = Repo(new List<RetirementFile> { File });
        files.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(File);
        Unit.Setup(u => u.RetirementFiles).Returns(files.Object);
        Set(u => u.Retirees, new List<Retiree> { File.Retiree });
        Set(u => u.FileReviewItems, Items);
        Set(u => u.PensionSuspensions, Holds);
        Set(u => u.FinancialEntitlements, Entitlements);
        Set(u => u.PaymentBooks, Books);
        Set(u => u.SalaryRecipients, new List<SalaryRecipient> { new() { FileID = 1, RecipientID = 3, RetireeID = 2, Retiree = File.Retiree, IsActive = true } });
        Set(u => u.EntitledPersons, []);
        Set(u => u.Agents, []);
        Set(u => u.Guardians, []);
        Set(u => u.GuardianEntitledPersons, []);
        Set(u => u.Documents, Documents);
        Set(u => u.DocumentTypes, []);
        Set(u => u.MissingDocuments, MissingDocuments);
        Set(u => u.DocumentPledges, Pledges);
        Set(u => u.DocumentExemptions, Exemptions);
        Set(u => u.StatusTransitionLogs, []);
        Set(u => u.AuditLogs, []);
        Set(u => u.SalaryPaymentItems, []);
        Entitlements.Add(new FinancialEntitlement { EntitlementID = 5, RetireeID = 2, Amount = 1000, EffectiveDate = DateTime.UtcNow.AddYears(-1) });
        Books.Add(new PaymentBook { PaymentBookID = 6, FileID = 1, Status = PaymentBookStatus.Active,
            IssueDate = Core.Time.YemenTime.Today.AddYears(-1), ExpiryDate = Core.Time.YemenTime.Today.AddYears(1) });
        Unit.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
        var scope = new Mock<ITestBranchScope>();
        scope.Setup(s => s.IsHQ()).Returns(true);
        var missing = new Mock<IMissingDocumentService>();
        missing.Setup(m => m.GetRequiredPairsAsync(1)).ReturnsAsync(RequiredDocuments);
        Service = new FileReviewService(Unit.Object, TestPensionAccess.From(Unit.Object, scope.Object), missing.Object);
    }

    public FileReviewItem AddItem(int status = FileReviewItemStatusConstants.Pending, bool historical = false)
    {
        var item = new FileReviewItem { ItemID = Items.Count + 1, RetirementFileID = 1, Status = status,
            CreatedAt = historical ? DateTime.UtcNow.AddDays(-1) : DateTime.UtcNow };
        Items.Add(item);
        return item;
    }

    private void Set<T>(Expression<Func<IUnitOfWork, IRepository<T>>> selector, List<T> rows) where T : class =>
        Unit.Setup(selector).Returns(Repo(rows).Object);

    private static Mock<IRepository<T>> Repo<T>(List<T> rows) where T : class
    {
        var repo = new Mock<IRepository<T>>();
        repo.Setup(r => r.Query()).Returns(() => new TestAsyncEnumerable<T>(rows));
        repo.Setup(r => r.AddAsync(It.IsAny<T>())).Callback<T>(rows.Add).Returns(Task.CompletedTask);
        return repo;
    }
}
