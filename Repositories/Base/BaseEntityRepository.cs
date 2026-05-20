using social_media_console_app.Data;
using social_media_console_app.Models;

namespace social_media_console_app.Repositories.Base;

public class BaseEntityRepository<T> : BaseRepository<T> where T : BaseEntity
{
    public BaseEntityRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }
    
    public async Task<T?> GetByIdAsync(int id)
    {
        return await _dbSet.FindAsync(id);
    }
    
    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await GetByIdAsync(id);
        if (entity is not null)
        {
            await DeleteAsync(entity);
        }

        return entity is not null;
    }

    public async Task DeleteWithoutChangeTrackingAsync(int id)
    {
        await DeleteWhereAsync(entity => entity.Id == id);
    }
    
    public async Task<bool> ExistsByIdAsync(int id)
    {
        return await ExistsAsync(entity => entity.Id == id);
    }

    public void ClearTracker()
    {
        _dbContext.ChangeTracker.Clear();
    }

}
