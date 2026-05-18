namespace social_media_console_app.Helpers.Inputs;

public readonly struct PaginatedInput
{
    // nested enum
    public enum Kind
    {
        Item = 1, 
        Next,
        Previous,
        BackToMenu
    }

    public Kind Type  { get; init; }
    public int  Index { get; init; }

    private PaginatedInput(Kind type, int index = 0)
    {
        Type = type;
        Index = index;
    }

    public static PaginatedInput Item(int index) => new(Kind.Item, index);
    public static PaginatedInput Next()          => new(Kind.Next);
    public static PaginatedInput Previous()      => new(Kind.Previous);
    public static PaginatedInput BackToMenu()    => new(Kind.BackToMenu);
}