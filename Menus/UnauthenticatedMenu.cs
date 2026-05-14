using social_media_console_app.BusinessLogic.Dtos;
using social_media_console_app.BusinessLogic.Services;
using social_media_console_app.Constants.Enums;

namespace social_media_console_app.Menus;

public class UnauthenticatedMenu
{
    private readonly AuthService _authService;
    private readonly SessionUser _sessionUser;

    public UnauthenticatedMenu(AuthService authService, SessionUser sessionUser)
    {
        _authService = authService;
        _sessionUser = sessionUser;
    }
    public async Task<UnauthenticatedMenuChoice> Run()
    {
        throw new NotImplementedException();
    }
}