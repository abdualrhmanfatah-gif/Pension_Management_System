using System.Linq.Expressions;

namespace PensionMS.Core.Interfaces;

/// <summary>
/// مستودع عام للعمليات الأساسية على الكيانات
/// </summary>
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    Task ReloadAsync(T entity);
    Task<T?> GetByIdAsync(string id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
    Task<int> CountAsync();
    Task<int> CountAsync(Expression<Func<T, bool>> predicate);
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);
    Task AddAsync(T entity);
    Task AddRangeAsync(IEnumerable<T> entities);
    /// <summary>Marks a detached graph modified. Tracked query results need only property changes and SaveChanges.</summary>
    void Update(T entity);
    /// <summary>Hard DELETE; rejects ISoftDeletable. Services must set deletion fields for those entities.</summary>
    void Delete(T entity);
    /// <summary>Hard DELETE of all supplied rows; rejects the range if any row is ISoftDeletable.</summary>
    void DeleteRange(IEnumerable<T> entities);
    /// <summary>Tracked query; use AsNoTracking for read-only projections. Page only after a unique ordering.</summary>
    IQueryable<T> Query();

    /// <summary>
    /// Pagination مبني على cursor — أسرع من offset مع البيانات الكبيرة.
    /// cursor = null يعيد أول صفحة. HasMore = true يعني فيه بيانات بعد cursor.
    /// المفتاح يجب أن يكون عددًا صحيحًا فريدًا؛ الترتيب تنازلي والصفحة التالية تستخدم آخر مفتاح.
    /// </summary>
    Task<(IEnumerable<T> Items, bool HasMore)> GetCursorPagedAsync(
        Expression<Func<T, bool>>? predicate,
        int? cursor,
        int limit,
        Expression<Func<T, int>> orderByKeySelector);
}
