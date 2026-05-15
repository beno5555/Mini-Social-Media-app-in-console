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

    protected override string Title     => $"Welcome, {_sessionUser.Username}!";
    protected override string BackLabel => "Log Out";

    protected override List<string> MenuOptions { get; } =
    [
        "Posts",
        "Friends",
        "Messages",
    ];


    public AuthenticatedMenu(
        SessionUser sessionUser,
        PostMenu postMenu,
        FriendMenu friendMenu,
        MessageMenu messageMenu,
        MessageService messageService) : base (sessionUser)
    {
        _postMenu = postMenu;
        _friendMenu = friendMenu;
        _messageMenu = messageMenu;

        _messageService = messageService;
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
            default:
                Console.WriteLine("Something went wrong");
                break;
        }
    }

    protected override async Task OnEnter()
    {
        Console.Write(Title);

        var hasUnread = await _messageService.HasUnreadAsync(_sessionUser.UserId);
        if (hasUnread)
        {
            Console.WriteLine(" You have unread messages");
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
        Thread.Sleep(1200);
    }
}