using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using social_media_console_app.Data;
using social_media_console_app.Models;

namespace social_media_console_app.Repositories.Base;

public class BaseRepository<T> where T : class
{
    protected readonly ApplicationDbContext _dbContext;
    protected readonly DbSet<T>             _dbSet;

    public BaseRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
        _dbSet = _dbContext.Set<T>();
    }

    /// <summary>
    /// opt for querying in batches if the db set is too large
    /// </summary>
    public async Task<List<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public async Task<List<T>> GetWhereAsync(
        Expression<Func<T, bool>> predicate,
        int? pageNumber = null,
        int? pageSize = null,
        bool track = true)
    {
        var query = Query(track).Where(predicate);
        
        if (pageNumber.HasValue && pageSize.HasValue)
        {
            query = query
                .Skip((pageNumber.Value - 1) * pageSize.Value)
                .Take(pageSize.Value);
        }

        return await query.ToListAsync();
    }

    public async Task AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        await _dbContext.SaveChangesAsync();
    }

    public async Task UpdateAsync(T entity)
    {
        _dbSet.Update(entity);
        await _dbContext.SaveChangesAsync();
    }

    public async Task UpdateRangeAsync(ICollection<T> entities)
    {
        _dbSet.UpdateRange(entities);
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(T entity)
    {
        _dbSet.Remove(entity);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<T?> GetFirstAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.FirstOrDefaultAsync(predicate);
    }

    public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.AnyAsync(predicate);
    }

    protected virtual IQueryable<T> Query(bool track = true)
    {
        var query = _dbSet;
        return track ? query : query.AsNoTracking();
    }
}