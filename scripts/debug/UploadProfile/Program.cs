using System.Data.Common;
using System.Diagnostics;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using PensionMS.API.Interceptors;
using PensionMS.Core.DTOs.Documents;
using PensionMS.Core.Entities;
using PensionMS.Core.Interfaces;
using PensionMS.Infrastructure.Data;
using PensionMS.Infrastructure.Repositories;
using PensionMS.Infrastructure.Services;

// Diagnostic harness: real services + SQL + audit interceptor, ALWAYS rollback.
// Never prints configuration, credentials, personal data, or SQL parameter values.
var root = Directory.GetCurrentDirectory();
var config = new ConfigurationBuilder().SetBasePath(Path.Combine(root, "PensionMS.API"))
    .AddJsonFile("appsettings.json").AddJsonFile("appsettings.Development.json", true)
    .AddUserSecrets("05973752-9457-4d0e-a307-0a6224474cd8", true)
    .AddEnvironmentVariables().Build();
var connection = config.GetConnectionString("DefaultConnection") ?? throw new Exception("Missing connection configuration");
var cs = new SqlConnectionStringBuilder(connection);
var server = cs.DataSource.Split('\\')[0].Split(',')[0];
if (!new[] { ".", "(local)", "(localdb)", "localhost", "127.0.0.1", Environment.MachineName }.Contains(server, StringComparer.OrdinalIgnoreCase))
    throw new Exception("This diagnostic only permits a local SQL server.");
var userId = int.Parse(Arg("--user", "1"));
var branchId = int.Parse(Arg("--branch", "1"));
var http = new HttpContextAccessor { HttpContext = new DefaultHttpContext() };
http.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[] {
    new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
    new Claim("BranchId", branchId.ToString()), new Claim("IsHQ", Arg("--hq", "false"))
}, "diagnostic"));
var metrics = new Measurements();
DbContextOptions<PensionDbContext> Options() => new DbContextOptionsBuilder<PensionDbContext>()
    .UseSqlServer(connection, sql => sql.UseCompatibilityLevel(120).CommandTimeout(30))
    .AddInterceptors(new ProfileAuditInterceptor(http, args.Contains("--without-review-audit"), metrics), new QueryTimings(metrics)).Options;

try
{
    await using var inspect = new PensionDbContext(Options());
    await inspect.Database.OpenConnectionAsync();
    if (args.Contains("--pending-migrations"))
    {
        var pending = (await inspect.Database.GetPendingMigrationsAsync()).ToArray();
        Console.WriteLine(JsonSerializer.Serialize(new { mode = "read-only migration check", pendingCount = pending.Length }));
        return;
    }
    // A modest read-only sample, including the largest document sets.
    if (args.Contains("--inspect"))
    {
        var files = await inspect.RetirementFiles.AsNoTracking().Where(f => !f.IsDeleted && f.PensionBranchID == branchId)
            .Select(f => new { f.FileID, f.StatusID, f.ReviewCompletedAt,
                Documents = inspect.Documents.Count(d => d.FileID == f.FileID && !d.IsDeleted),
                PendingReviewItems = inspect.FileReviewItems.Count(i => i.RetirementFileID == f.FileID && i.Status == PensionMS.Core.Constants.FileReviewItemStatusConstants.Pending) })
            .OrderByDescending(f => f.Documents).Take(6).ToListAsync();
        Console.WriteLine(JsonSerializer.Serialize(new { mode = "read-only", files }));
        return;
    }
    var fileId = int.Parse(Arg("--file", "0"));
    if (fileId <= 0) throw new Exception("An explicit --file is required.");
    var fileExists = await inspect.RetirementFiles.AnyAsync(f => f.FileID == fileId && !f.IsDeleted && f.PensionBranchID == branchId);
    if (!fileExists || !await inspect.Users.AnyAsync(u => u.UserID == userId && u.IsActive))
        throw new Exception("Invalid file scope or user.");
    if (args.Contains("--lock-probe"))
    {
        // Separate connections, real database locks, no writes or test records.
        var probeOptions = new DbContextOptionsBuilder<PensionDbContext>()
            .UseSqlServer(connection, sql => sql.UseCompatibilityLevel(120).CommandTimeout(5)).Options;
        await using var firstDb = new PensionDbContext(probeOptions);
        await using var secondDb = new PensionDbContext(probeOptions);
        await firstDb.Database.OpenConnectionAsync();
        await secondDb.Database.OpenConnectionAsync();
        await using var firstTx = await firstDb.Database.BeginTransactionAsync();
        await using var secondTx = await secondDb.Database.BeginTransactionAsync();
        var firstUnit = new UnitOfWork(firstDb);
        var secondUnit = new UnitOfWork(secondDb);
        if (!firstUnit.HasActiveTransaction || !secondUnit.HasActiveTransaction)
            throw new Exception("Caller-owned transactions were not detected.");
        await firstUnit.LockFileReviewAsync(fileId);
        var waiting = secondUnit.LockFileReviewAsync(fileId);
        var blocked = await Task.WhenAny(waiting, Task.Delay(200)) != waiting;
        await firstTx.RollbackAsync();
        var releasedAt = Stopwatch.StartNew();
        await waiting.WaitAsync(TimeSpan.FromSeconds(5));
        releasedAt.Stop();
        await secondTx.RollbackAsync();
        Console.WriteLine(JsonSerializer.Serialize(new { mode = "SQL lock probe; no writes",
            sameFileBlocked = blocked, resumedAfterRollback = true,
            releaseToAcquireMs = Math.Round(releasedAt.Elapsed.TotalMilliseconds, 2) }));
        if (!blocked) throw new Exception("Overlapping file reviews were not serialized.");
        return;
    }
    var types = await inspect.DocumentTypes.AsNoTracking().Where(d => d.IsActive &&
        (d.OwnerType == null || d.OwnerType == DocumentOwnerType.RetirementFile)).ToListAsync();
    var docType = types.FirstOrDefault(d => string.IsNullOrEmpty(d.AllowedExtensions) ||
        d.AllowedExtensions.Split(',').Any(e => e.Trim().Equals(".pdf", StringComparison.OrdinalIgnoreCase)))
        ?? throw new Exception("No suitable PDF document type.");
    var bytes = int.Parse(Arg("--bytes", "262144"));
    if (bytes > Math.Min(docType.MaxFileSize ?? 10, 10) * 1024 * 1024 || bytes < 1024)
        throw new Exception("Diagnostic fixture exceeds document policy.");
    var repeats = int.Parse(Arg("--repeat", "3"));
    if (repeats is < 1 or > 12) throw new Exception("Repeat limit is 1..12.");
    var tempRoot = Path.Combine(root, "scripts", "debug", "UploadProfile", "scratch", Guid.NewGuid().ToString("N"));
    Directory.CreateDirectory(tempRoot);
    var marker = "اختبار قياس مؤقت " + Guid.NewGuid().ToString("N");
    try
    {
        for (var iteration = 0; iteration < repeats; iteration++)
        {
            await using var db = new PensionDbContext(Options());
            await using var transaction = await db.Database.BeginTransactionAsync();
            // All DB services and the audit interceptor share THIS transaction/context.
            var unit = MeasuredProxy<IUnitOfWork>.Wrap(new UnitOfWork(db), "uow", metrics);
            var entitlement = new EntitlementEngine(unit);
            var missing = new MissingDocumentService(unit, entitlement, new DiagnosticLogger<MissingDocumentService>(metrics));
            var compliance = MeasuredProxy<IComplianceEngine>.Wrap(new ComplianceEngine(unit, missing), "compliance", metrics);
            var review = MeasuredProxy<IFileReviewService>.Wrap(new FileReviewService(unit), "review", metrics);
            var audit = MeasuredProxy<IImmutableAuditService>.Wrap(new ImmutableAuditService(db), "immutableAudit", metrics);
            var storageConfig = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?> {
                ["FileStorage:UploadsFolder"] = "uploads", ["FileStorage:BaseUrl"] = ""
            }).Build();
            var storage = MeasuredProxy<IFileStorageService>.Wrap(new LocalFileStorageService(new DiagnosticEnvironment(tempRoot), storageConfig), "storage", metrics);
            var service = new DocumentService(unit, storage, compliance, entitlement, review,
                new BranchScopeProvider(http), audit, new DiagnosticLogger<DocumentService>(metrics));
            metrics.Clear();
            var batchSize = Math.Max(1, int.Parse(Arg("--batch", "1")));
            if (batchSize > 12) throw new Exception("Batch limit is 1..12.");
            var deferredBatch = args.Contains("--deferred");
            var watch = Stopwatch.StartNew();
            bool success;
            var perDocMs = new List<double>();
            var uploadedIds = new List<int>();
            var successes = 0;
            var finalMs = 0.0;
            var refreshOk = false;
            try
            {
                for (var b = 0; b < batchSize; b++)
                {
                    var single = Stopwatch.StartNew();
                    var result = await service.UploadAsync(new UploadDocumentDto {
                        File = new PdfFixture(bytes), DocumentTypeID = docType.DocumentTypeID,
                        RetirementFileID = fileId, Notes = marker
                    }, userId, Guid.NewGuid().ToString("N"), deferredBatch);
                    single.Stop();
                    perDocMs.Add(Math.Round(single.Elapsed.TotalMilliseconds, 2));
                    if (result.IsSuccess)
                    {
                        successes++;
                        if (result.Data is not null) uploadedIds.Add(result.Data.DocumentID);
                    }
                    else Console.WriteLine(JsonSerializer.Serialize(new { validation = result.Message }));
                }
                if (deferredBatch && successes > 0)
                {
                    var final = Stopwatch.StartNew();
                    var refresh = await service.RefreshAfterUploadAsync(fileId, uploadedIds, userId);
                    final.Stop();
                    finalMs = Math.Round(final.Elapsed.TotalMilliseconds, 2);
                    refreshOk = refresh.IsSuccess;
                    if (!refreshOk) Console.WriteLine(JsonSerializer.Serialize(new { refreshFailed = refresh.Message }));
                }
                success = successes == batchSize && (!deferredBatch || refreshOk);
            }
            finally
            {
                watch.Stop();
                await transaction.RollbackAsync();
            }
            var summary = new
            {
                reviewMs = metrics.Stages.GetValueOrDefault("review.RequestReviewAsync", 0),
                reviewSaveMs = metrics.Stages.GetValueOrDefault("review.SaveChangesAsync", 0),
                complianceMs = metrics.Stages.GetValueOrDefault("compliance.RefreshComplianceCacheAsync", 0),
                storageMs = metrics.Stages.GetValueOrDefault("storage.SaveFileAsync", 0),
                docAuditMs = metrics.Stages.GetValueOrDefault("immutableAudit.LogAsync", 0),
                sqlCommands = metrics.Queries.Values.Sum(q => q.Count),
                sqlMs = Math.Round(metrics.Queries.Values.Sum(q => q.Ms), 2),
                itemsDeleted = metrics.Counters.GetValueOrDefault("review.itemsDeleted", 0),
                itemsInserted = metrics.Counters.GetValueOrDefault("review.itemsInserted", 0),
                auditRowsGenerated = metrics.Counters.GetValueOrDefault("review.auditRowsGenerated", 0),
                reviewSaveCalls = metrics.Counters.GetValueOrDefault("review.saveCalls", 0),
                auditRowsTotal = metrics.Counters.GetValueOrDefault("audit.rows.total", 0),
                complianceCalls = metrics.Counters.GetValueOrDefault("compliance.calls", 0),
                reviewCalls = metrics.Counters.GetValueOrDefault("review.calls", 0),
                batchSize,
                batchMode = deferredBatch ? "deferred+refresh" : "eager",
                successes,
                perDocMs,
                persistenceMs = Math.Round(perDocMs.Sum(), 2),
                finalMs,
                refreshOk,
            };
            Console.WriteLine(JsonSerializer.Serialize(new { iteration, fileId, bytes,
                mode = "real SQL and services; transaction rolled back; temporary storage; excludes HTTP",
                success, totalMs = Math.Round(watch.Elapsed.TotalMilliseconds, 2),
                variant = args.Contains("--without-review-audit") ? "review audit interceptor suppressed FOR DIAGNOSIS ONLY" : "baseline",
                summary, stages = metrics.Stages, queries = metrics.Queries, counters = metrics.Counters, errors = metrics.Errors }));
            if (iteration > 0 && double.TryParse(Arg("--budget-ms", ""), out var budget)
                && watch.Elapsed.TotalMilliseconds > budget)
                Environment.ExitCode = 2;
        }
    }
    finally
    {
        // Only delete the checked, unique scratch directory created above.
        var safeParent = Path.GetFullPath(Path.Combine(root, "scripts", "debug", "UploadProfile", "scratch")) + Path.DirectorySeparatorChar;
        if (!Path.GetFullPath(tempRoot).StartsWith(safeParent, StringComparison.OrdinalIgnoreCase))
            throw new Exception("Scratch cleanup boundary mismatch");
        Directory.Delete(tempRoot, true);
    }
    var retainedDocuments = await inspect.Documents.IgnoreQueryFilters().CountAsync(d => d.Notes == marker);
    Console.WriteLine(JsonSerializer.Serialize(new { cleanup = true, retainedDocuments, scratchExists = Directory.Exists(tempRoot) }));
    if (retainedDocuments != 0) throw new Exception("Rollback verification failed");
}
catch (Exception ex)
{
    var sql = ex as SqlException ?? ex.InnerException as SqlException;
    Console.WriteLine(JsonSerializer.Serialize(new { failed = ex.GetType().Name, sqlNumber = sql?.Number }));
    Environment.ExitCode = 1;
}

string Arg(string name, string fallback) { var i = Array.IndexOf(args, name); return i >= 0 && i + 1 < args.Length ? args[i + 1] : fallback; }

public sealed class Measurements
{
    public static readonly AsyncLocal<string?> Stage = new();
    public Dictionary<string, double> Stages { get; } = new();
    public Dictionary<string, QueryMeasurement> Queries { get; } = new();
    public List<string> Errors { get; } = new();
    public Dictionary<string, int> Counters { get; } = new();
    public void Clear() { Stages.Clear(); Queries.Clear(); Errors.Clear(); Counters.Clear(); }
    public void Count(string key, int count) => Counters[key] = Counters.GetValueOrDefault(key) + count;
    public void Record(string name, double ms) => Stages[name] = Math.Round(Stages.GetValueOrDefault(name) + ms, 2);
    public void Sql(DbCommand command, double ms)
    {
        var tables = Regex.Matches(command.CommandText, @"(?:FROM|JOIN|UPDATE|INTO|INSERT|MERGE)\s+\[([^\]]+)\]", RegexOptions.IgnoreCase)
            .Select(m => m.Groups[1].Value).Distinct();
        var key = (Stage.Value ?? "upload.other") + ":" + string.Join("+", tables);
        if (!Queries.TryGetValue(key, out var value)) Queries[key] = value = new();
        value.Count++; value.Ms = Math.Round(value.Ms + ms, 2);
    }
}
public sealed class QueryMeasurement { public int Count { get; set; } public double Ms { get; set; } }
public class MeasuredProxy<T> : DispatchProxy where T : class
{
    public T Target = null!;
    public string Name = "";
    public Measurements Metrics = null!;
    public static T Wrap(T target, string name, Measurements metrics)
    {
        var proxy = Create<T, MeasuredProxy<T>>();
        var p = (MeasuredProxy<T>)(object)proxy; p.Target = target; p.Name = name; p.Metrics = metrics;
        return proxy;
    }
    protected override object? Invoke(MethodInfo? method, object?[]? arguments)
    {
        if (Name == "uow" && method!.Name != "SaveChangesAsync") return method.Invoke(Target, arguments);
        if (Name != "uow") Metrics.Count(Name + ".calls", 1);
        var previous = Measurements.Stage.Value;
        var key = (Name == "uow" ? previous ?? "upload.other" : Name) + "." + method!.Name;
        Measurements.Stage.Value = Name == "uow" ? previous : Name;
        var sw = Stopwatch.StartNew();
        try
        {
            var result = method.Invoke(Target, arguments);
            if (result is Task task)
            {
                if (method.ReturnType.IsGenericType)
                    return GetType().GetMethod(nameof(TrackGeneric), BindingFlags.Instance | BindingFlags.Public)!
                        .MakeGenericMethod(method.ReturnType.GenericTypeArguments[0]).Invoke(this, new object[] { task, sw, key });
                return Track(task, sw, key);
            }
            Metrics.Record(key, sw.Elapsed.TotalMilliseconds); return result;
        }
        finally { Measurements.Stage.Value = previous; }
    }
    public async Task Track(Task task, Stopwatch watch, string key) { try { await task; } finally { Metrics.Record(key, watch.Elapsed.TotalMilliseconds); } }
    public async Task<TResult> TrackGeneric<TResult>(Task<TResult> task, Stopwatch watch, string key) { try { return await task; } finally { Metrics.Record(key, watch.Elapsed.TotalMilliseconds); } }
}
// One-variable probe: disable automatic audit generation ONLY during review.
// Never used in the application and never commits. Initial document audit stays enabled.
public sealed class ProfileAuditInterceptor(IHttpContextAccessor http, bool suppressReview, Measurements metrics) : AuditSaveChangesInterceptor(http)
{
    private bool Suppress => suppressReview && Measurements.Stage.Value == "review";
    public override InterceptionResult<int> SavingChanges(DbContextEventData e, InterceptionResult<int> r)
        => Suppress ? r : base.SavingChanges(e, r);
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData e, InterceptionResult<int> r, CancellationToken ct = default)
    {
        var isReview = Measurements.Stage.Value == "review" && e.Context is not null;
        var before = isReview && e.Context is { } pre
            ? pre.ChangeTracker.Entries<AuditLog>().Count(x => x.State == EntityState.Added)
            : 0;
        if (isReview && e.Context is { } db)
        {
            metrics.Count("review.saveCalls", 1);
            metrics.Count("review.itemsDeleted", db.ChangeTracker.Entries<FileReviewItem>().Count(x => x.State == EntityState.Deleted));
            metrics.Count("review.itemsInserted", db.ChangeTracker.Entries<FileReviewItem>().Count(x => x.State == EntityState.Added));
        }
        var result = Suppress ? r : await base.SavingChangesAsync(e, r, ct);
        // Rows generated by THIS save: counted after the base interceptor staged them.
        if (!Suppress && e.Context is { } staged)
            metrics.Count("audit.rows.total",
                staged.ChangeTracker.Entries<AuditLog>().Count(x => x.State == EntityState.Added));
        if (isReview && e.Context is { } post)
            metrics.Count("review.auditRowsGenerated",
                post.ChangeTracker.Entries<AuditLog>().Count(x => x.State == EntityState.Added) - before);
        return result;
    }
    public override int SavedChanges(SaveChangesCompletedEventData e, int r)
        => Suppress ? r : base.SavedChanges(e, r);
    public override ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData e, int r, CancellationToken ct = default)
        => Suppress ? ValueTask.FromResult(r) : base.SavedChangesAsync(e, r, ct);
}
public sealed class QueryTimings(Measurements metrics) : DbCommandInterceptor
{
    public override DbDataReader ReaderExecuted(DbCommand c, CommandExecutedEventData e, DbDataReader r) { metrics.Sql(c, e.Duration.TotalMilliseconds); return r; }
    public override ValueTask<DbDataReader> ReaderExecutedAsync(DbCommand c, CommandExecutedEventData e, DbDataReader r, CancellationToken ct = default) { metrics.Sql(c, e.Duration.TotalMilliseconds); return ValueTask.FromResult(r); }
    public override int NonQueryExecuted(DbCommand c, CommandExecutedEventData e, int r) { metrics.Sql(c, e.Duration.TotalMilliseconds); return r; }
    public override ValueTask<int> NonQueryExecutedAsync(DbCommand c, CommandExecutedEventData e, int r, CancellationToken ct = default) { metrics.Sql(c, e.Duration.TotalMilliseconds); return ValueTask.FromResult(r); }
    public override object? ScalarExecuted(DbCommand c, CommandExecutedEventData e, object? r) { metrics.Sql(c, e.Duration.TotalMilliseconds); return r; }
    public override ValueTask<object?> ScalarExecutedAsync(DbCommand c, CommandExecutedEventData e, object? r, CancellationToken ct = default) { metrics.Sql(c, e.Duration.TotalMilliseconds); return ValueTask.FromResult(r); }
    public override void CommandFailed(DbCommand c, CommandErrorEventData e) => metrics.Errors.Add("SQL:" + (e.Exception as SqlException)?.Number);
    public override Task CommandFailedAsync(DbCommand c, CommandErrorEventData e, CancellationToken ct = default) { CommandFailed(c,e); return Task.CompletedTask; }
}
public sealed class DiagnosticLogger<T>(Measurements metrics) : ILogger<T>
{
    public IDisposable? BeginScope<TState>(TState state) where TState:notnull => null;
    public bool IsEnabled(LogLevel level) => level >= LogLevel.Warning;
    public void Log<TState>(LogLevel level, EventId id, TState state, Exception? exception, Func<TState,Exception?,string> formatter)
    { if (level >= LogLevel.Warning) metrics.Errors.Add(typeof(T).Name + ":" + (exception?.GetType().Name ?? level.ToString())); }
}
public sealed class PdfFixture : IRemoteFile
{
    private readonly byte[] data;
    public PdfFixture(int bytes)
    {
        data = Enumerable.Repeat((byte)' ', bytes).ToArray();
        Encoding.ASCII.GetBytes("%PDF-1.4\n1 0 obj\n<< /Type /Catalog >>\nendobj\n%" + Guid.NewGuid()).CopyTo(data,0);
        Encoding.ASCII.GetBytes("\n%%EOF\n").CopyTo(data, bytes - 7);
    }
    public string FileName => "diagnostic-" + Guid.NewGuid().ToString("N") + ".pdf";
    public string ContentType => "application/pdf";
    public long Length => data.Length;
    public Stream OpenReadStream() => new MemoryStream(data, false);
}
public sealed class DiagnosticEnvironment(string root) : IWebHostEnvironment
{
    public string WebRootPath { get; set; } = root;
    public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
    public string ApplicationName { get; set; } = "UploadProfile";
    public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    public string ContentRootPath { get; set; } = root;
    public string EnvironmentName { get; set; } = "Development";
}
