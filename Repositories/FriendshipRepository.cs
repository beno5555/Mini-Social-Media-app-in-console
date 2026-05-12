using Microsoft.EntityFrameworkCore;
using social_media_console_app.Constants.Enums;
using social_media_console_app.Data;
using social_media_console_app.Models;
using social_media_console_app.Repositories.Base;

namespace social_media_console_app.Repositories;

public class FriendshipRepository : BaseRepository<Friendship>
{
    public FriendshipRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
        
    }

    protected override IQueryable<Friendship> Query(bool track = true)
    {
        var query = _dbSet
            .Include(friendShip => friendShip.RequesterUser)
            .Include(friendship => friendship.AddresseeUser);

        return track ? query : query.AsNoTracking();
    }

    /// <summary>
    /// gets the pending requests that were SENT to user with id userId
    /// </summary>
    public async Task<List<Friendship>> GetPendingRequestsAsync(int userId, int? pageNumber = null, int? pageSize = null)
    {
        return await GetAsync(userId, FriendshipStatus.Pending, pageNumber, pageSize);
    }

    /// <summary>
    /// gets the rejected requests that were SENT to user with id userId
    /// </summary>
    public async Task<List<Friendship>> GetRejectedRequestsAsync(int userId, int? pageNumber = null, int? pageSize = null)
    {
        return await GetAsync(userId, FriendshipStatus.Declined, pageNumber, pageSize);
    }
    
    /// <summary>
    /// fetches the friendships that userId sent with optional status filter
    /// </summary>
    public async Task<List<Friendship>> GetAsync(int userId, FriendshipStatus? status, int? pageNumber = null, int? pageSize = null)
    {
        return await GetWhereAsync(friendship =>
            friendship.AddresseeUserId == userId && (!status.HasValue || friendship.FriendshipStatus == status), // only checks for status if the parameter has value
            pageNumber, pageSize); 
    }

    /// <summary>
    /// fetches the friendships of userId which have been accepted (from either)
    /// </summary>
    public async Task<List<Friendship>> GetFriendsAsync(int userId, int? pageNumber = null, int? pageSize = null)
    {
        return await GetWhereAsync(friendship =>
            (friendship.RequesterUserId == userId || friendship.AddresseeUserId == userId) &&
            friendship.FriendshipStatus == FriendshipStatus.Accepted,
            pageNumber, pageSize);
    }
    
    /// <summary>
    /// fetches the accepted request of userA and userB. order does not matter
    /// </summary>
    public async Task<Friendship?> GetRelationshipAsync(int userIdA, int userIdB)
    {
        return await GetFirstAsync(friendship =>
            (friendship.RequesterUserId == userIdA || friendship.RequesterUserId == userIdB) &&
            (friendship.AddresseeUserId == userIdA || friendship.AddresseeUserId == userIdB));
    }

    public async Task<bool> ExistsAsync(int userIdA, int userIdB)
    {
        return await ExistsAsync(friendship => 
                (friendship.RequesterUserId == userIdA || friendship.RequesterUserId == userIdB) &&
                (friendship.AddresseeUserId == userIdA || friendship.AddresseeUserId == userIdB));
    }

    public async Task UpdateStatusAsync(Friendship friendship, FriendshipStatus status)
    {
        friendship.FriendshipStatus = status;
        await _dbContext.SaveChangesAsync();
    }
}