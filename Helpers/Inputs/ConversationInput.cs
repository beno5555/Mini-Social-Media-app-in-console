namespace social_media_console_app.Helpers.Inputs;

public readonly struct ConversationInput
{
    public enum Kind
    {
        WriteMessage = 1,
        Next, 
        Previous,
        BackToMenu
    }

    public Kind Type { get; }

    private ConversationInput(Kind type)
    {
        Type = type;
    }
    
    public static ConversationInput WriteMessage()      => new (Kind.WriteMessage);
    public static ConversationInput Next()       => new (Kind.Next);
    public static ConversationInput Previous()   => new (Kind.Previous);
    public static ConversationInput BackToMenu() => new(Kind.BackToMenu);
}