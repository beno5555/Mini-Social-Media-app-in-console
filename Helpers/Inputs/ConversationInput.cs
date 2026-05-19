namespace social_media_console_app.Helpers.Inputs;

public readonly struct ConversationInput
{
    public enum Kind
    {
        WriteMessage = 1,
        Newer, 
        Older,
        ViewUserProfile,
        BackToMenu
    }

    public Kind Type { get; }

    private ConversationInput(Kind type)
    {
        Type = type;
    }
    
    public static ConversationInput WriteMessage() => new (Kind.WriteMessage);
    public static ConversationInput Newer()        => new (Kind.Newer);
    public static ConversationInput Older()        => new (Kind.Older);
    public static ConversationInput UserProfile()        => new (Kind.ViewUserProfile);
    public static ConversationInput BackToMenu() => new(Kind.BackToMenu);
}