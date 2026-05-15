using social_media_console_app.BusinessLogic.Dtos;
using social_media_console_app.BusinessLogic.Mappers;
using social_media_console_app.BusinessLogic.Services;
using social_media_console_app.Menus.Base;

namespace social_media_console_app.Menus.Authenticated;

public class MessageMenu : BaseMenu
{
    private readonly MessageService _messageService;
    private readonly MessageMapper _messageMapper;
    
    protected override string       Title       => "Messages";
    protected override List<string> MenuOptions { get; } = [];

    public MessageMenu(SessionUser sessionUser, MessageService messageService, MessageMapper messageMapper) : base (sessionUser)
    {
        _messageService = messageService;
        _messageMapper = messageMapper;
    }
    
    protected override Task CompleteOperation(int choice)
    {
        return Task.CompletedTask;
    }
}