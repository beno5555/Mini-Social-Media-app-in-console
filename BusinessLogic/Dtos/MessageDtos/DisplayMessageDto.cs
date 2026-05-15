namespace social_media_console_app.BusinessLogic.Dtos.MessageDtos;

public record DisplayMessageDto(int Id, string MessageContent, string SenderUsername, DateTime SentAt, bool IsRead);