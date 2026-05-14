using Microsoft.EntityFrameworkCore;
using social_media_console_app.Data;
using social_media_console_app.Models;
using social_media_console_app.Repositories.Base;

namespace social_media_console_app.Repositories;

public class MessageRepository : BaseEntityRepository<Message>
{
    public MessageRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
        
    }

    protected override IQueryable<Message> Query(bool track = true)
    {
        var query = _dbSet
            .Include(message => message.SenderUser)
            .Include(message => message.ReceiverUser);

        return track ? query : query.AsNoTracking();
    }

    public async Task<List<Message>> GetConversationAsync(
        int userA,
        int userB,
        int? pageNumber = null,
        int? pageSize = null )
    {
        return await GetWhereAsync(
            message => (message.SenderUserId == userA || message.SenderUserId == userB) &&
                       (message.ReceiverUserId == userA || message.ReceiverUserId == userB),
            pageNumber, pageSize);
    }

    public async Task<List<Message>> GetUnreadAsync(int senderId, int receiverId)
    {
        return await GetWhereAsync(message =>
            message.SenderUserId == senderId && message.ReceiverUserId == receiverId && !message.IsRead);
    }

    /// <summary>
    /// use this when the unread messages have already been loaded
    /// </summary>
    public async Task MarkAsReadAsync(List<Message> unreadMessages)
    {
        foreach (var message in unreadMessages)
        {
            message.IsRead = true;
        }

        await _dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// use this when the messages have not been loaded
    /// </summary>
    public async Task MarkConversationAsReadAsync(int senderId, int receiverId)
    {
        await _dbSet.Where(message =>
                message.SenderUserId   == senderId   &&
                message.ReceiverUserId == receiverId && 
                !message.IsRead)
            .ExecuteUpdateAsync(setter => setter.SetProperty(message => message.IsRead, true));
    }

    public async Task<bool> HasUnreadAsync(int userId)
    {
        return await ExistsAsync(message => message.ReceiverUserId == userId && !message.IsRead);
    }
}