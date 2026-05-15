using social_media_console_app.BusinessLogic.Dtos.CommentDtos;
using social_media_console_app.BusinessLogic.Mappers.Base;
using social_media_console_app.Models;

namespace social_media_console_app.BusinessLogic.Mappers;

public class CommentMapper : IMapper<Comment, CreateCommentDto, DisplayCommentDto>
{
    public Comment ToEntity(CreateCommentDto createCommentDto)
    {
        return new Comment
        {
            CommentContent = createCommentDto.CommentContent,
            CommenterUserId = createCommentDto.CommenterUserId,
            PostId = createCommentDto.PostId,
        };
    }

    public DisplayCommentDto ToDisplay(Comment comment)
    {
        return new DisplayCommentDto(comment.Id, comment.CommenterUser!.Username, comment.CommentContent, comment.CreatedAt);
    }
}