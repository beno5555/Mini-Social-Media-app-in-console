using social_media_console_app.BusinessLogic.Dtos.CommentDtos;

namespace social_media_console_app.BusinessLogic.Dtos.PostDtos;

public record DisplayPostDto(string AuthorUsername, string Title, string Content, DateTime UploadedAt, List<DisplayCommentDto>? Comments = null);