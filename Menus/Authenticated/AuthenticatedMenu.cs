using social_media_console_app.BusinessLogic.Dtos;
using social_media_console_app.BusinessLogic.Services;
using social_media_console_app.Menus.Base;

namespace social_media_console_app.Menus.Authenticated;

public class AuthenticatedMenu : BaseMenu
{
    private readonly PostMenu    _postMenu;
    private readonly FriendMenu  _friendMenu;
    private readonly MessageMenu _messageMenu;

    // use this service to check if the user has any unread messages right after login
    private readonly MessageService _messageService;
    private readonly AccountService _accountService;

    protected override string Title     => $"Welcome, {_sessionUser.Username}!";
    protected override string BackLabel => "Log Out";

    protected override List<string> MenuOptions { get; } =
    [
        "Posts",
        "Friends",
        "Messages",
        "View your profile"
    ];


    public AuthenticatedMenu(
        SessionUser sessionUser,
        PostMenu postMenu,
        FriendMenu friendMenu,
        MessageMenu messageMenu,
        MessageService messageService,
        AccountService accountService) : base (sessionUser)
    {
        _postMenu = postMenu;
        _friendMenu = friendMenu;
        _messageMenu = messageMenu;

        // check for unread messages
        _messageService = messageService;
        _accountService = accountService;
        
        // menu bridging
        _friendMenu.OnViewUserPosts = userId => _postMenu.ViewUserPostsAsync(userId);
        _friendMenu.OnOpenConversation = otherUser => _messageMenu.OpenConversationAsync(otherUser);

        _postMenu.OnViewUserProfile = userId => _friendMenu.ViewProfileAsync(userId);
        
        _messageMenu.OnViewUserProfile = userId => _friendMenu.ViewProfileAsync(userId);
    }

    public override async Task<bool> Run()
    {
        while (true)
        {
            try
            {
                return await base.Run();
            }
            catch (Exception)
            {
                _currentMenuMessage = "Returned to menu.";
            }
        }
    }

    protected override async Task CompleteOperation(int choice)
    {
        switch (choice)
        {
            case 1:
                await _postMenu.Run();
                break;
            case 2:
                await _friendMenu.Run();
                break;
            case 3:
                await _messageMenu.Run();
                break;
            case 4:
                await ViewProfile();
                break;
            default:
                Console.WriteLine("Something went wrong");
                break;
        }
    }

    protected override async Task OnEnter(string? currentMenuMessage = null)
    {
        await base.OnEnter(currentMenuMessage);

        var hasUnread = await _messageService.HasUnreadAsync(_sessionUser.UserId);
        if (hasUnread)
        {
            Console.WriteLine("You have unread messages");
        }
    }

    protected override void OnBack()
    {
        LogOut();
    }

    private void LogOut()
    {
        _sessionUser.UserId = 0;
        _sessionUser.Username = string.Empty;
        Console.WriteLine("Logging out...");
        Thread.Sleep(ProjectConstants.Constants.MenuBackTrackDelayInMilliseconds);
    }

    private async Task ViewProfile()
    {
        var userResponse = await _accountService.GetByUsername(_sessionUser.Username);

        if (userResponse.Success)
        {
            await _friendMenu.ViewOwnProfileAsync(userResponse.Data!);
        }
        else
        {
            Console.WriteLine("Could not fetch profile");
        }
    }
}