namespace social_media_console_app.Menus;

public class MainMenu
{
    private readonly UnauthenticatedMenu _unauthenticatedMenu;
    private readonly AuthenticatedMenu _authenticatedMenu;

    public MainMenu(UnauthenticatedMenu unauthenticatedMenu, AuthenticatedMenu authenticatedMenu)
    {
        _unauthenticatedMenu = unauthenticatedMenu;
        _authenticatedMenu = authenticatedMenu;
    }

    public async Task Run()
    {
        
    }
}