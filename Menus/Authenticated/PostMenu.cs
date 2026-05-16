using social_media_console_app.BusinessLogic.Dtos;
using social_media_console_app.BusinessLogic.Dtos.CommentDtos;
using social_media_console_app.BusinessLogic.Dtos.PostDtos;
using social_media_console_app.BusinessLogic.Services;
using social_media_console_app.Constants;
using social_media_console_app.Helpers;
using social_media_console_app.Menus.Base;

namespace social_media_console_app.Menus.Authenticated;

public class PostMenu : BaseMenu
{
    private readonly PostService    _postService;
    private readonly CommentService _commentService;

    protected override string Title => "Posts";
    protected override List<string> MenuOptions { get; } =
    [
        "View Feed",
        "New Post",
        "My Posts"
    ];

    public PostMenu(
        SessionUser    sessionUser,
        PostService    postService,
        CommentService commentService) : base(sessionUser)
    {
        _postService = postService;
        _commentService = commentService;
    }
    
    protected override async Task CompleteOperation(int choice)
    {
        switch (choice)
        {
            case 1:
                await ViewFeedAsync();
                break;
            case 2:
                await CreatePostAsync();
                break;
            case 3:
                await ViewOwnPostsAsync();
                break;
            default:
                Console.WriteLine("Something went wrong.");
                break;
        }
    }
    
    #region User Feed flow

    private async Task ViewFeedAsync()
    {
        async Task<List<DisplayPostDto>> FetchPage(int pageNumber, int pageSize) =>
            await _postService.GetFeedAsync(_sessionUser.UserId, pageNumber, pageSize);

        await BrowseAndSelectAsync(
            FetchPage,
            Printer.PrintPost,
            Constraints.DefaultPageSize,
            ViewPostAsync,
            sectionTitle: "See what your friends have been up to!"
            );
    }

    private async Task ViewPostActionAsync(int choice, DisplayPostDto post)
    {
        switch (choice)
        {
            case 1:
                await ViewPostCommentsAsync(post);
                break;
            case 2:
                await WriteCommentAsync(post);
                break;
            case 3:
                await DeletePostAsync(post);
                break;
        }
    }

    private async Task ViewPostAsync(DisplayPostDto post)
    {
        Printer.PrintPostDetails(post);
        Console.WriteLine();

        List<string> viewPostOptions =
        [
            "View Comments",
            "Write a Comment"
        ];
        bool isOwner = post.AuthorUsername == _sessionUser.Username;
        if (isOwner)
        {
            viewPostOptions.Add("Delete post");
        }
        
        Printer.PrintLines(viewPostOptions, "Back to posts");
        int choice = Prompter.GetIntInput(string.Empty, 0, viewPostOptions.Count);

        await ViewPostActionAsync(choice, post);
    }

    private async Task ViewPostCommentsAsync(DisplayPostDto post)
    {
        async Task<List<DisplayCommentDto>> FetchPage(int pageNumber, int pageSize)
        {
            var response = await _commentService.GetByPostAsync(post.Id, pageNumber, pageSize);
            return response.Data ?? [];
        }

        await BrowseAndSelectAsync(
            FetchPage,
            Printer.PrintComment,
            Constraints.DefaultPageSize,
            sectionTitle: "Comments");
    }

    private async Task WriteCommentAsync(DisplayPostDto post)
    {
        var createCommentDto = DtoPrompter.NewComment(_sessionUser.UserId, post.Id);
        var response = await _commentService.AddCommentAsync(createCommentDto);
        if (response.Success)
        {
            Console.WriteLine("Comment uploaded");
        }
        else
        {
            Console.WriteLine("Failed to upload a comment. " + response.Message);
        }
        
    }

    private async Task DeletePostAsync(DisplayPostDto post)
    {
        await ConfirmAction($"Are you sure you want to delete post?", async () =>
        {
            var response = await _postService.DeletePostAsync(post.Id);
            
            if (response.Success)
            {
                Console.WriteLine("Post deleted");
            }
            else
            {
                Console.WriteLine("Could not delete post. " + response.Message);
            }
        });
    }
    
    #endregion
    
    private async Task CreatePostAsync()
    {
        var postDto = DtoPrompter.CreatePost(_sessionUser.UserId);
        var response = await _postService.UploadPost(postDto);

        if (response.Success)
        {
            Console.WriteLine("Post uploaded!");
        }
        else
        {
            Console.WriteLine("Upload failed. " + response.Message);
        }
    }

    private async Task ViewOwnPostsAsync()
    {
        async Task<List<DisplayPostDto>> FetchPage(int pageNumber, int pageSize)
        {
            var response = await _postService.GetByUserIdAsync(_sessionUser.UserId, pageNumber, pageSize);
            return response.Data ?? [];
        }

        await BrowseAndSelectAsync(
            FetchPage,
            Printer.PrintPost,
            Constraints.DefaultPageSize,
            ViewPostAsync);
    }

    /// <summary>
    /// might use this in friend menu if I figure out a way to wire the menus bidirectionally
    /// </summary>
    private async Task ViewUserPostsAsync(int userId)
    {
        var fetchInAdvance = await _postService.GetByUserIdAsync(userId, 1, Constraints.DefaultPageSize);
        if (fetchInAdvance.Success)
        {
            async Task<List<DisplayPostDto>> FetchPage(int pageNumber, int pageSize)
            {
                var response = await _postService.GetByUserIdAsync(userId, pageNumber, pageSize);
                return response.Data ?? [];
            }

            await BrowseAndSelectAsync(
                FetchPage,
                Printer.PrintPost,
                Constraints.DefaultPageSize,
                ViewPostAsync,
                $"Posts featured by '{_sessionUser.Username}'");
        }
        else
        {
            Console.WriteLine("Failed to fetch posts. " + fetchInAdvance.Message);
        }

    }
}