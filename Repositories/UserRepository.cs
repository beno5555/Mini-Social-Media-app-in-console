using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using social_media_console_app.Data;
using social_media_console_app.Models;
using social_media_console_app.Repositories.Base;

namespace social_media_console_app.Repositories;

public class UserRepository : BaseEntityRepository<User>
{
    public UserRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
        
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await GetFirstAsync(user => user.Username == username);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await GetFirstAsync(user => user.Email == email);
    }

    public async Task<User?> GetByUniqueIdentifierAsync(string uniqueIdentifier)
    {
        return await GetFirstAsync(user => user.Email    == uniqueIdentifier ||
                                           user.Username == uniqueIdentifier);
    }

    public async Task<bool> ExistsByUsernameAsync(string username)
    {
        return await ExistsAsync(user => user.Username == username);
    }

    public async Task<bool> ExistsByEmailAsync(string email)
    {
        return await ExistsAsync(user => user.Email == email);
    }
    
    public async Task<User?> GetWithPostsByUsernameAsync(string username, int recentPostsCount = 10)
    {
        return await GetWithPostsAsync(user => user.Username == username, recentPostsCount);
    }

    public async Task<User?> GetWithPostsByEmailAsync(string email, int recentPostsCount = 10)
    {
        return await GetWithPostsAsync(user => user.Email == email, recentPostsCount);
    }

    public async Task<User?> GetWithPostsAsync(Expression<Func<User, bool>> predicate, int recentPostsCount = 10)
    {
        return await _dbContext.Users
            .Include(user => user.Posts
                .OrderByDescending(post => post.CreatedAt)
                .Take(recentPostsCount))
            .FirstOrDefaultAsync(predicate);
    }

    public async Task<List<User>> GetUsersAsync(int excludedUserId, int? pageNumber, int? pageSize)
    {
        return await GetWhereAsync(user => user.Id != excludedUserId, pageNumber, pageSize);
    }

    public async Task<List<User>> SearchByUsernameAsync(string usernameInput, int? pageNumber, int? pageSize)
    {
        return await GetWhereAsync(user => user.Username.Contains(usernameInput), pageNumber, pageSize);
    }


    /// <summary>
    /// messages and friends aren't loaded from users.
    /// 
    /// </summary>
    /// <returns></returns>
    protected override IQueryable<User> Query(bool track = true)
    {
        var query = _dbSet
            .Include(user => user.Posts)
                .ThenInclude(post => post.Comments)
            .Include(user => user.Comments);

        return track ? query : query.AsNoTracking();
    }
}