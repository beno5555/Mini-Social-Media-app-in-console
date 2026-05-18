using social_media_console_app.BusinessLogic.Dtos;
using social_media_console_app.BusinessLogic.Dtos.MessageDtos;
using social_media_console_app.BusinessLogic.Dtos.UserDtos;
using social_media_console_app.BusinessLogic.Services;
using social_media_console_app.Helpers;
using social_media_console_app.Helpers.Inputs;
using social_media_console_app.Menus.Base;

namespace social_media_console_app.Menus.Authenticated;

public class MessageMenu : BaseMenu
{
    private readonly MessageService    _messageService;

    protected override string Title => "Messages";
    protected override List<string> MenuOptions { get; } = [
        "See Conversations",
        "Start a new conversation"
    ];

    public MessageMenu(SessionUser sessionUser, MessageService messageService) : base (sessionUser)
    {
        _messageService = messageService;
    }
    
    protected override async Task CompleteOperation(int choice)
    {
        switch (choice)
        {
            case 1:
                await OpenExistingConversationsAsync();
                break;
            case 2:
                await StartNewConversationAsync();
                break; 
        }
    }
    
    #region Existing conversations section

    private async Task OpenExistingConversationsAsync()
    {
        async Task<List<DisplayUserDto>> FetchFriends(int pageNumber, int pageSize) =>
            await _messageService.GetConversationFriendsAsync(_sessionUser.UserId, pageNumber, pageSize);

        await BrowseAndSelectAsync(
            FetchFriends,
            Printer.PrintUserPreview,
            ProjectConstants.Constants.DefaultPageSize,
            OpenConversationAsync,
            "Open Conversation - Select a friend");
    }

    private async Task OpenConversationAsync(DisplayUserDto otherUser)
    {
        async Task<List<DisplayMessageDto>> FetchMessages(int pageNumber, int pageSize)
        {
            List<DisplayMessageDto> messages = [];
            var conversationResponse = await _messageService.GetConversationAsync(_sessionUser.UserId, otherUser.Id, pageNumber, pageSize);
            
            if (conversationResponse.Success && conversationResponse.Data is not null)
            {
                messages = conversationResponse.Data;
            }
            else
            {
                Console.WriteLine("Something went wrong. " + conversationResponse.Message);
            }

            return messages;
        }

        async Task OnWriteMessage()
        {
            await SendMessageAsync(otherUser);
        }

        await BrowseMessagesAsync(
            FetchMessages,
            OnWriteMessage,
            $"Conversation with '{otherUser.Username}'"
        );
    }
    
    #endregion
    
    #region Start a new conversation
    private async Task StartNewConversationAsync()
    {
        
        async Task<List<DisplayUserDto>> FetchFriends(int pageNumber, int pageSize) =>
            await _messageService.GetNonConversationFriendsAsync(_sessionUser.UserId, pageNumber, pageSize);

        await BrowseAndSelectAsync(
            FetchFriends,
            Printer.PrintUserPreview,
            ProjectConstants.Constants.DefaultPageSize,
            SendMessageAsync,
            "New Conversation - select a friend"
        );

    }
    
    #endregion

    #region Send a message
    private async Task SendMessageAsync(DisplayUserDto receiverUser)
    {
        
        var messageDto          = DtoPrompter.Message(_sessionUser.UserId, receiverUser.Id);
        var sendMessageResponse = await _messageService.SendMessageAsync(messageDto);

        if (sendMessageResponse.Success)
        {
            Console.WriteLine("Sent a message!");
        }
        else
        {
            Console.WriteLine("Something went wrong. " + sendMessageResponse.Message);
        }
    }
    #endregion
    
    #region Message pagination and actions

    private async Task<bool> PaginateMessagesAsync(
        Func<int, int, Task<List<DisplayMessageDto>>> fetchMessages,
        string sectionTitle
    )
    {
        int currentPage = 1;
        
        bool run          = true;
        bool writeMessage = false;

        var cache = new Dictionary<int, List<DisplayMessageDto>>();

        while (run)
        {
            if (!cache.TryGetValue(currentPage, out var messages))
            {
                messages = await fetchMessages(currentPage, ProjectConstants.Constants.DefaultConversationPageSize);
                cache[currentPage] = messages;
            }


            if (messages.Count > 0)
            {
                await OnEnter(sectionTitle);
                Console.WriteLine($"Page - {currentPage}");
                
                Printer.PrintChatBorder();
                Printer.PrintMessages(messages, _sessionUser.Username);
                Printer.PrintChatBorder();
                
                Console.WriteLine();
                
                bool hasNext     = currentPage    > 1;
                bool hasPrevious = messages.Count == ProjectConstants.Constants.DefaultConversationPageSize;
                
                ConversationInput input = Prompter.GetConversationInput(hasPrevious, hasNext);

                switch (input.Type)
                {
                    case ConversationInput.Kind.WriteMessage:
                        writeMessage = true;
                        run = false;
                        break;
                    case ConversationInput.Kind.Previous:
                        currentPage++;
                        break;
                    case ConversationInput.Kind.Next:
                        currentPage--;
                        break;
                    case ConversationInput.Kind.BackToMenu:
                        run = false;
                        break;
                }
            }
        }

        return writeMessage;
    }


    private async Task BrowseMessagesAsync(
        Func<int, int, Task<List<DisplayMessageDto>>> fetchMessages,
        Func<Task>                                    onWriteMessage,
        string                                        sectionTitle
        )
    {
        bool run = true;
        while (run)
        {
            bool writeMessage = await PaginateMessagesAsync(fetchMessages, sectionTitle);

            if (writeMessage)
            {
                await onWriteMessage();
            }
            else
            {
                run = false;
            }
        }
    }
    
    #endregion
}