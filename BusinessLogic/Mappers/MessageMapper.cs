using social_media_console_app.BusinessLogic.Dtos.MessageDtos;
using social_media_console_app.BusinessLogic.Mappers.Base;
using social_media_console_app.Models;

namespace social_media_console_app.BusinessLogic.Mappers;

public class MessageMapper : IMapper<Message, CreateMessageDto, DisplayMessageDto>
{
    public Message ToEntity(CreateMessageDto createMessageDto)
    {
        return new Message
        {
            ReceiverUserId = createMessageDto.ReceiverId,
            SenderUserId = createMessageDto.SenderId,
            MessageContent = createMessageDto.MessageContent
        };
    }

    public DisplayMessageDto ToDisplay(Message message)
    {
        return new DisplayMessageDto(
            message.Id,
            message.MessageContent,
            message.SenderUser!.Username,
            message.CreatedAt,
            message.IsRead);
    }
}