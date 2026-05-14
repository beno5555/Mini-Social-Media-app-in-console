using System.Security.Principal;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace social_media_console_app.Models;

public class Message : BaseEntity
{
    public string MessageContent { get; set; } = string.Empty;
    public bool   IsRead         { get; set; } = false;

    public int   SenderUserId { get; set; }
    public User? SenderUser   { get; set; }

    public int   ReceiverUserId { get; set; }
    public User? ReceiverUser   { get; set; } 
}