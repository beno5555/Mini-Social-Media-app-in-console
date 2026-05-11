using Microsoft.EntityFrameworkCore;
using social_media_console_app.Data;
using social_media_console_app.Models;
using social_media_console_app.Repositories.Base;

namespace social_media_console_app.Repositories;

public class CommentRepository : BaseEntityRepository<Comment>
{
    public CommentRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
        
    }

    public async Task<List<Comment>> GetByUserIdAsync(int userId, int? pageNumber, int? pageSize)
    {
        return await GetWhereAsync(comment => comment.CommenterUserId == userId, pageNumber, pageSize);
    }

    public async Task<List<Comment>> GetByPostIdAsync(int postId, int? pageNumber, int? pageSize)
    {
        return await GetWhereAsync(comment => comment.PostId == postId, pageNumber, pageSize);
    }

    protected override IQueryable<Comment> Query(bool track = true)
    {
        var query = _dbSet
            .Include(comment => comment.CommenterUser)
            .Include(comment => comment.Post);
        
        return track ? query : query.AsNoTracking();
    }
}