namespace social_media_console_app.Constants;

public static class Constraints
{
    public const int EmailMinLength    = 10;
    public const int EmailMaxLength    = 100;
    public const int UsernameMinLength = EmailMinLength;
    public const int UsernameMaxlength = EmailMaxLength;
    
    public const int PasswordMinLength     = 6;
    public const int PasswordMaxLength     = 100;
    public const int PasswordHashMaxLength = 44;
    public const int PasswordSaltMaxLength = 44;
    
    public const int BioMaxLength             = 300;
    public const int PostTitleMaxLength       = 100;
    public const int PostContentMaxLength     = 3000;
    public const int PostContentPreviewLength = 50;
    public const int CommentMaxLength         = 500;
    public const int MessageMaxLength         = 1000;
    
    public const int MinAge = 13;
    public const int MaxAge = 130;

    public const int DefaultPageSize      = 3;
    public const int ConversationPageSize = 20;
    
    public const string EmailRegexPattern    = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
    public const string UsernameRegexPattern = "^(?=.*[a-zA-Z])[a-zA-Z0-9._-]+$";

    public const int MenuBackTrackDelayInMilliseconds = 300;
}