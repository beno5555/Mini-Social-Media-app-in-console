using social_media_console_app.BusinessLogic.Dtos;
using social_media_console_app.Constants.Enums;
using social_media_console_app.Menus.Authenticated;

namespace social_media_console_app.Menus;

public class MainMenu
{
    private readonly UnauthenticatedMenu _unauthenticatedMenu;
    private readonly AuthenticatedMenu   _authenticatedMenu;
    private readonly SessionUser         _sessionUser;

    public MainMenu(UnauthenticatedMenu unauthenticatedMenu, AuthenticatedMenu authenticatedMenu, SessionUser sessionUser)
    {
        _unauthenticatedMenu = unauthenticatedMenu;
        _authenticatedMenu = authenticatedMenu;
        _sessionUser = sessionUser;
    }

    public async Task Run()
    {
        bool run = true;
        
        while (run)
        {
            if (!_sessionUser.IsLoggedIn)
            {
                var exit = await _unauthenticatedMenu.Run();
                
                if (exit)
                {
                    run = false;
                }
            }
            else
            {
                await _authenticatedMenu.Run();
            }
        }
    }
}