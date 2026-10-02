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
    Task<IEnumerable<T>> GetPagedAsync(int page, int pageSize);
    Task<IEnumerable<T>> GetPagedAsync(Expression<Func<T, bool>> predicate, int page, int pageSize);
    Task<int> CountAsync();
    Task<int> CountAsync(Expression<Func<T, bool>> predicate);
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);
    Task AddAsync(T entity);
    Task AddRangeAsync(IEnumerable<T> entities);
    void Update(T entity);
    void Delete(T entity);
    void DeleteRange(IEnumerable<T> entities);
    IQueryable<T> Query();

    /// <summary>
    /// Pagination مبني على cursor — أسرع من offset مع البيانات الكبيرة.
    /// cursor = null يعيد أول صفحة. HasMore = true يعني فيه بيانات بعد cursor.
    /// </summary>
    Task<(IEnumerable<T> Items, bool HasMore)> GetCursorPagedAsync(
        Expression<Func<T, bool>>? predicate,
        int? cursor,
        int limit,
        Expression<Func<T, int>> orderByKeySelector);
}
