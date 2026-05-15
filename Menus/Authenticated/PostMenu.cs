using social_media_console_app.BusinessLogic.Dtos;
using social_media_console_app.BusinessLogic.Services;
using social_media_console_app.Menus.Base;

namespace social_media_console_app.Menus.Authenticated;

public class PostMenu : BaseMenu
{
    private readonly PostService    _postService;
    private readonly CommentService _commentService;

    protected override string Title => "Posts";
    protected override List<string> MenuOptions { get; } =
    [
    ];
    
    private const int PageSize = 10;

    public PostMenu(
        SessionUser    sessionUser,
        PostService    postService,
        CommentService commentService) : base(sessionUser)
    {
        _postService = postService;
        _commentService = commentService;
    }


    
    protected override Task CompleteOperation(int choice)
    {
        return Task.CompletedTask;
    }

    protected override void OnBack()
    {
        
    }

}