using Microsoft.EntityFrameworkCore;
using PensionMS.Core.Interfaces;
using PensionMS.Infrastructure.Data;
using System.Linq.Expressions;

namespace PensionMS.Infrastructure.Repositories;

/// <summary>
/// تنفيذ المستودع العام
/// </summary>
public class Repository<T> : IRepository<T> where T : class
{
    protected readonly PensionDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(PensionDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(int id)
        => await _dbSet.FindAsync(id);

    public Task ReloadAsync(T entity) => _context.Entry(entity).ReloadAsync();

    public async Task<T?> GetByIdAsync(string id)
        => await _dbSet.FindAsync(id);

    public async Task<IEnumerable<T>> GetAllAsync()
        => await _dbSet.ToListAsync();

    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        => await _dbSet.Where(predicate).ToListAsync();

    public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
        => await _dbSet.FirstOrDefaultAsync(predicate);

    public async Task<int> CountAsync()
        => await _dbSet.CountAsync();

    public async Task<int> CountAsync(Expression<Func<T, bool>> predicate)
        => await _dbSet.CountAsync(predicate);

    public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
        => await _dbSet.AnyAsync(predicate);

    public async Task AddAsync(T entity)
        => await _dbSet.AddAsync(entity);

    public async Task AddRangeAsync(IEnumerable<T> entities)
        => await _dbSet.AddRangeAsync(entities);

    public void Update(T entity)
        => _dbSet.Update(entity);

    public void Delete(T entity)
    {
        RejectSoftDeletable(entity);
        _dbSet.Remove(entity);
    }

    public void DeleteRange(IEnumerable<T> entities)
    {
        var items = entities.ToList();
        foreach (var entity in items) RejectSoftDeletable(entity);
        _dbSet.RemoveRange(items);
    }

    private static void RejectSoftDeletable(T entity)
    {
        if (entity is ISoftDeletable)
            throw new InvalidOperationException("الكيان يدعم الحذف المنطقي؛ يجب تحديث حقول الحذف بدلاً من حذفه فعلياً");
    }

    public IQueryable<T> Query()
        => _dbSet.AsQueryable();

    public async Task<(IEnumerable<T> Items, bool HasMore)> GetCursorPagedAsync(
        Expression<Func<T, bool>>? predicate,
        int? cursor,
        int limit,
        Expression<Func<T, int>> orderByKeySelector)
    {
        if (limit <= 0 || limit == int.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(limit), "حجم الصفحة غير صالح");
        var query = predicate != null ? _dbSet.Where(predicate) : _dbSet.AsQueryable();

        if (cursor.HasValue)
        {
            // Build: e => orderByKeySelector(e) < cursor
            var param = orderByKeySelector.Parameters[0];
            var body = Expression.LessThan(orderByKeySelector.Body, Expression.Constant(cursor.Value));
            var lambda = Expression.Lambda<Func<T, bool>>(body, param);
            query = query.Where(lambda);
        }

        var items = await query
            .OrderByDescending(orderByKeySelector)
            .Take(limit + 1)
            .ToListAsync();

        bool hasMore = items.Count > limit;
        return (hasMore ? items.Take(limit).ToList() : items, hasMore);
    }
}
