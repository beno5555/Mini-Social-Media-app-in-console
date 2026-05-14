using social_media_console_app.BusinessLogic.Dtos;
using social_media_console_app.BusinessLogic.Services;
using social_media_console_app.Models;

namespace social_media_console_app.Menus.Authenticated;

public class AuthenticatedMenu
{
    private readonly SessionUser _sessionUser;
    
    private readonly PostMenu    _postMenu;
    private readonly FriendMenu  _friendMenu;
    private readonly MessageMenu _messageMenu;

    // use this service to check if the user has any unread messages right after login
    private readonly MessageService _messageService;

    public AuthenticatedMenu(
        SessionUser sessionUser,
        PostMenu postMenu,
        FriendMenu friendMenu,
        MessageMenu messageMenu,
        MessageService messageService)
    {
        _sessionUser = sessionUser;
        
        _postMenu = postMenu;
        _friendMenu = friendMenu;
        _messageMenu = messageMenu;

        _messageService = messageService;
    }
    public async Task Run()
    {
        throw new NotImplementedException();
    }
}