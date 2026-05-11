using social_media_console_app.BusinessLogic.Dtos.PostDtos;
using social_media_console_app.Models;

namespace social_media_console_app.BusinessLogic.Mappers;

public class PostMapper
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
        return null;
    }
}