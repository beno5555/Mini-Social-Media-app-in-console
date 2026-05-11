namespace social_media_console_app.BusinessLogic.Dtos.PostDtos;

public record CreatePostDto(int UserId, string PostTitle, string PostContent);