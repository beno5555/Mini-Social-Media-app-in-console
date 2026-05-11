namespace social_media_console_app.BusinessLogic.Dtos.MessageDtos;

public record CreateMessageDto(int SenderId, int ReceiverId, string MessageContent);