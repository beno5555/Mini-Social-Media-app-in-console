using social_media_console_app.BusinessLogic.Dtos.MessageDtos;
using social_media_console_app.BusinessLogic.Mappers;
using social_media_console_app.BusinessLogic.Responses;
using social_media_console_app.Constants.Enums;
using social_media_console_app.Repositories;

namespace social_media_console_app.BusinessLogic.Services;

public class MessageService
{
    private readonly MessageRepository    _messageRepository;
    private readonly UserRepository       _userRepository;
    private readonly FriendshipRepository _friendshipRepository;
    private readonly MessageMapper        _messageMapper;

    public MessageService(
        MessageRepository messageRepository,
        UserRepository userRepository,
        FriendshipRepository friendshipRepository,
        MessageMapper messageMapper)
    {
        _messageRepository = messageRepository;
        _userRepository = userRepository;
        _friendshipRepository = friendshipRepository;
        _messageMapper = messageMapper;
    }

    /// <summary>
    /// Assumes that senderId is valid since the method should only be called when a logged-in user tries to send a message
    /// </summary>
    /// <param name="createMessageDto"></param>
    /// <returns></returns>
    public async Task<Response> SendMessageAsync(CreateMessageDto createMessageDto)
    {
        var response = new Response();
        
        var friendshipCheck = await ValidFriendship(createMessageDto.SenderId, createMessageDto.ReceiverId);
        
        if (friendshipCheck.Success)
        {
            var messageToAdd = _messageMapper.ToEntity(createMessageDto);
            await _messageRepository.AddAsync(messageToAdd);
        }
        else
        {
            response = friendshipCheck;
        }

        return response;
    }


    public async Task<Response<List<DisplayMessageDto>>> GetConversationAsync(
        int currentUserId,     
        int  responderUserId,
        int? pageNumber = null,
        int? pageSize = null)
    {
        var response = new Response<List<DisplayMessageDto>>();

        var friendshipCheck = await ValidFriendship(currentUserId, responderUserId);

        if (friendshipCheck.Success)
        {
            var messages = await _messageRepository.GetConversationAsync(currentUserId, responderUserId, pageNumber, pageSize);
            var unreadMessages = messages.Where(message => !message.IsRead && message.ReceiverUserId == currentUserId)
                .ToList();
            
            await _messageRepository.MarkAsReadAsync(unreadMessages);
            
            var messageDtos = messages.Select(_messageMapper.ToDisplay).ToList();
            response.Ok(messageDtos);
        }
        else
        {
            response.Fail(friendshipCheck.Message);
        }

        return response;
    }

    public async Task<bool> HasUnreadAsync(int userId)
    {
        return await _messageRepository.HasUnreadAsync(userId);
    }
    
    /// <summary>
    /// Checks if the ids match, receiverId is valid and 2 users are friends
    /// </summary>
    private async Task<Response> ValidFriendship(int senderId, int receiverId)
    {
        var response = new Response();

        if (receiverId != senderId)
        {
            var receiverExists = await _userRepository.ExistsByIdAsync(receiverId);

            if (receiverExists)
            {
                var areFriends = await _friendshipRepository.ExistsAsync(senderId, receiverId, FriendshipStatus.Accepted);

                if (!areFriends)
                {
                    response.Fail("You can only send messages to your friends");
                }
            }
            else
            {
                response.Fail("Receiver user not found");
            }
        }
        else
        {
            response.Fail("Cannot send a message to oneself");
        }

        return response;
    }
}