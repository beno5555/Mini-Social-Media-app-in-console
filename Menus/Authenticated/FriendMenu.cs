using System.Reflection.Metadata;
using social_media_console_app.BusinessLogic.Dtos;
using social_media_console_app.BusinessLogic.Mappers;
using social_media_console_app.BusinessLogic.Services;
using social_media_console_app.Menus.Base;

namespace social_media_console_app.Menus.Authenticated;

public class FriendMenu : BaseMenu
{
    private readonly FriendshipService _friendshipService;
    private readonly AccountService    _accountService;
    
    private readonly UserMapper  _userMapper;

    protected override string Title => "Friends";

    protected override List<string> MenuOptions { get; } =
    [
    ];
    
    public FriendMenu(
        FriendshipService friendshipService,
        AccountService    accountService,
        UserMapper        userMapper,
        SessionUser       sessionUser
    ) : base (sessionUser)
    {
        _friendshipService = friendshipService;
        _accountService = accountService;
        
        _userMapper = userMapper;
    }


    protected override Task CompleteOperation(int choice)
    {
        throw new NotImplementedException();
    }

    protected override void OnBack()
    {
        throw new NotImplementedException();
    }

}