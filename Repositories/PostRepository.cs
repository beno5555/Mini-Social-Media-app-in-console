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

    protected override IQueryable<Post> Query(bool track = true)
    {
        var query = _dbSet
            .Include(post => post.User)
            .Include(post => post.Comments);

        return track ? query : query.AsNoTracking();
    }
}