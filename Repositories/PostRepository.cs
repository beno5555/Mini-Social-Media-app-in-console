using Microsoft.EntityFrameworkCore;
using social_media_console_app.Data;
using social_media_console_app.Models;
using social_media_console_app.Repositories.Base;

namespace social_media_console_app.Repositories;

public class PostRepository : BaseEntityRepository<Post>
{
    public PostRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
        
    }

    public async Task<List<Post>> GetByUserIdAsync(int userId, int? pageNumber = null, int? pageSize = null)
    {
        return await GetWhereAsync(p => p.UserId == userId, pageNumber, pageSize);
    }

    /// <summary>
    /// fetches the latest posts of the users that are included in the integer list containing userIds.
    /// </summary>
    public async Task<List<Post>> GetFeedAsync(List<int> friendIds, int? pageNumber, int? pageSize)
    {
        return await GetWhereAsync(post => friendIds.Contains(post.UserId), pageNumber, pageSize,
            query => query.OrderByDescending(post => post.CreatedAt));
    }

    protected override IQueryable<Post> Query(bool track = true)
    {
        var query = _dbSet
            .Include(post => post.User);

        return track ? query : query.AsNoTracking();
    }
}