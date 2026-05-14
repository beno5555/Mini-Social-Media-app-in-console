using System.ComponentModel.DataAnnotations;

namespace social_media_console_app.Models;

public class User : BaseEntity
{
    public string   Username     { get; set; } = string.Empty;
    public string   Email        { get; set; } = string.Empty;
    public DateTime DateOfBirth  { get; set; }  
    public string   PasswordHash { get; set; } = string.Empty;
    public string   PasswordSalt { get; set; } = string.Empty;

    public string? Bio { get; set; }
    // role

    public ICollection<Post>       Posts                 { get; set; } = new List<Post>();
    public ICollection<Comment>    Comments              { get; set; } = new List<Comment>();
    public ICollection<Message>    SentMessages          { get; set; } = new List<Message>();
    public ICollection<Message>    ReceivedMessages      { get; set; } = new List<Message>();
    public ICollection<Friendship> SentFriendRequests     { get; set; } = new List<Friendship>();
    public ICollection<Friendship> ReceivedFriendRequests { get; set; } = new List<Friendship>();
    
}