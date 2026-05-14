using social_media_console_app.BusinessLogic.Dtos;
using social_media_console_app.BusinessLogic.Services;

namespace social_media_console_app.Menus.Authenticated;

public class PostMenu
{
    private readonly SessionUser _sessionUser;
    
    private readonly PostService    _postService;
    private readonly CommentService _commentService;

    private const int pageSize = 10;

    public PostMenu(
        SessionUser sessionUser,
        PostService postService,
        CommentService commentService)
    {
        _sessionUser = sessionUser;

        _postService = postService;
        _commentService = commentService;
    }

    public async Task Run()
    {
        
    }
}