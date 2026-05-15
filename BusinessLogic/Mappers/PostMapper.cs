using social_media_console_app.BusinessLogic.Dtos.CommentDtos;
using social_media_console_app.BusinessLogic.Dtos.MessageDtos;
using social_media_console_app.BusinessLogic.Dtos.PostDtos;
using social_media_console_app.BusinessLogic.Mappers.Base;
using social_media_console_app.Models;

namespace social_media_console_app.BusinessLogic.Mappers;

public class PostMapper : IMapper<Post, CreatePostDto, DisplayPostDto>
{
    public Post ToEntity(CreatePostDto createPostDto)
    {
        return new Post
        {
            UserId = createPostDto.UserId,
            PostTitle = createPostDto.PostTitle,
            PostContent = createPostDto.PostContent,
        };
    }

    public DisplayPostDto ToDisplay(Post post)
    {
        return new DisplayPostDto(post.Id, post.User!.Username, post.PostTitle, post.PostContent, post.CreatedAt);
    }

    public DisplayPostDto ToDisplay(Post post, List<DisplayCommentDto> commentDtos)
    {
        return new DisplayPostDto(post.Id, post.User!.Username, post.PostTitle, post.PostContent, post.CreatedAt, commentDtos);
    }
}