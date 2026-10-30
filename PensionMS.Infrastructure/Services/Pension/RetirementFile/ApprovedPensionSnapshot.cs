using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using PensionMS.Core.Entities;
using PensionMS.Core.Interfaces;
using PensionMS.Infrastructure.Data;

namespace PensionMS.Infrastructure.Services.Pension.RetirementFile;

/// <summary>نسخة بيانات الاستحقاق المعتمدة؛ لا تستخدم التعديلات المعلقة لحساب المعاش.</summary>
public sealed class ApprovedPensionSnapshot
{
    public int Version { get; set; } = 1;
    public DateTime? ApprovedAtUtc { get; set; }
    [JsonRequired] public Retiree Retiree { get; set; } = new();
    [JsonRequired] public List<FinancialEntitlement> Entitlements { get; set; } = [];
    [JsonRequired] public List<SalaryRecipient> Recipients { get; set; } = [];
    [JsonRequired] public List<PaymentBook> Books { get; set; } = [];
    [JsonRequired] public List<EntitledPerson> EntitledPersons { get; set; } = [];
    [JsonRequired] public List<GuardianEntitledPerson> GuardianLinks { get; set; } = [];

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        ReferenceHandler = ReferenceHandler.IgnoreCycles
    };

    public string Serialize() => JsonSerializer.Serialize(this, JsonOptions);

    public void MarkReviewed()
    {
        Retiree.PendingReviewStatus = PendingReviewStatus.Approved;
        foreach (var person in EntitledPersons) person.PendingReviewStatus = PendingReviewStatus.Approved;
        foreach (var book in Books) book.PendingReviewStatus = Core.Constants.PendingReviewStatusConstants.Approved;
        foreach (var recipient in Recipients)
        {
            recipient.PendingReviewStatus = PendingReviewStatus.Approved;
            if (recipient.Agent != null) recipient.Agent.PendingReviewStatus = PendingReviewStatus.Approved;
            if (recipient.Guardian != null) recipient.Guardian.PendingReviewStatus = PendingReviewStatus.Approved;
            if (recipient.EntitledPerson != null) recipient.EntitledPerson.PendingReviewStatus = PendingReviewStatus.Approved;
        }
    }

    public static ApprovedPensionSnapshot? Read(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        try
        {
            using var document = JsonDocument.Parse(json);
            return document.RootElement.ValueKind == JsonValueKind.Object &&
                document.RootElement.TryGetProperty(nameof(Version), out var version) &&
                version.ValueKind == JsonValueKind.Number && version.TryGetInt32(out var number) && number == 1
                ? JsonSerializer.Deserialize<ApprovedPensionSnapshot>(json, JsonOptions) : null;
        }
        catch (JsonException) { return null; }
    }

    public static Task<ApprovedPensionSnapshot> CaptureAsync(PensionDbContext db, int fileId, int retireeId,
        CancellationToken cancellationToken = default) => CaptureAsync(fileId, retireeId,
        db.Retirees, db.FinancialEntitlements, db.SalaryRecipients, db.PaymentBooks,
        db.EntitledPersons, db.GuardianEntitledPersons, cancellationToken);

    internal static ApprovedPensionSnapshot Capture(PensionDbContext db, int fileId, int retireeId)
    {
        var queries = Queries(fileId, retireeId, db.Retirees, db.FinancialEntitlements,
            db.SalaryRecipients, db.PaymentBooks, db.EntitledPersons, db.GuardianEntitledPersons);
        return new ApprovedPensionSnapshot
        {
            Retiree = queries.Retirees.First(), Entitlements = queries.Entitlements.ToList(),
            Recipients = queries.Recipients.ToList(), Books = queries.Books.ToList(),
            EntitledPersons = queries.EntitledPersons.ToList(), GuardianLinks = queries.Links.ToList()
        };
    }

    public static Task<ApprovedPensionSnapshot> CaptureAsync(IUnitOfWork uow, int fileId, int retireeId) =>
        CaptureAsync(fileId, retireeId, uow.Retirees.Query(), uow.FinancialEntitlements.Query(),
            uow.SalaryRecipients.Query(), uow.PaymentBooks.Query(), uow.EntitledPersons.Query(),
            uow.GuardianEntitledPersons.Query(), default);

    private static async Task<ApprovedPensionSnapshot> CaptureAsync(int fileId, int retireeId,
        IQueryable<Retiree> retirees, IQueryable<FinancialEntitlement> entitlements,
        IQueryable<SalaryRecipient> recipients, IQueryable<PaymentBook> books,
        IQueryable<EntitledPerson> entitledPersons, IQueryable<GuardianEntitledPerson> links,
        CancellationToken cancellationToken)
    {
        var queries = Queries(fileId, retireeId, retirees, entitlements, recipients, books, entitledPersons, links);
        return new ApprovedPensionSnapshot
        {
            Retiree = await queries.Retirees.FirstAsync(cancellationToken),
            Entitlements = await queries.Entitlements.ToListAsync(cancellationToken),
            Recipients = await queries.Recipients.ToListAsync(cancellationToken),
            Books = await queries.Books.ToListAsync(cancellationToken),
            EntitledPersons = await queries.EntitledPersons.ToListAsync(cancellationToken),
            GuardianLinks = await queries.Links.ToListAsync(cancellationToken)
        };
    }

    private sealed record SnapshotQueries(IQueryable<Retiree> Retirees, IQueryable<FinancialEntitlement> Entitlements,
        IQueryable<SalaryRecipient> Recipients, IQueryable<PaymentBook> Books,
        IQueryable<EntitledPerson> EntitledPersons, IQueryable<GuardianEntitledPerson> Links);

    private static SnapshotQueries Queries(int fileId, int retireeId, IQueryable<Retiree> retirees,
        IQueryable<FinancialEntitlement> entitlements, IQueryable<SalaryRecipient> recipients,
        IQueryable<PaymentBook> books, IQueryable<EntitledPerson> entitledPersons, IQueryable<GuardianEntitledPerson> links) =>
        new(retirees.AsNoTracking().Where(r => r.RetireeID == retireeId),
            entitlements.AsNoTracking().Where(e => e.RetireeID == retireeId && !e.IsDeleted),
            recipients.AsNoTracking().Where(r => r.FileID == fileId && !r.IsDeleted)
                .Include(r => r.Agent).ThenInclude(a => a!.EntitledPerson)
                .Include(r => r.Guardian).Include(r => r.EntitledPerson).Include(r => r.Retiree),
            books.AsNoTracking().Where(b => b.FileID == fileId && !b.IsDeleted),
            entitledPersons.AsNoTracking().Where(e => e.FileID == fileId && !e.IsDeleted),
            links.AsNoTracking().Where(g => g.EntitledPerson.FileID == fileId && !g.IsDeleted));
}
