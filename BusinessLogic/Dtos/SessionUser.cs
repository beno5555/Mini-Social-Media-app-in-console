using System.Security.Principal;

namespace social_media_console_app.BusinessLogic.Dtos;

public class SessionUser
{
    public int    UserId   { get; set; }
    public string Username { get; set; } = string.Empty;
}