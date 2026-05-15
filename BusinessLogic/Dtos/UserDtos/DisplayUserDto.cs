namespace social_media_console_app.BusinessLogic.Dtos.UserDtos;

public record DisplayUserDto(int Id, string Username, string? Bio, DateTime CreatedAt, DateTime DateOfBirth);