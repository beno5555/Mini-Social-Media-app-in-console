using social_media_console_app.BusinessLogic.Dtos;
using social_media_console_app.BusinessLogic.Dtos.CommentDtos;
using social_media_console_app.BusinessLogic.Dtos.PostDtos;
using social_media_console_app.BusinessLogic.Services;
using social_media_console_app.ProjectConstants;
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

    public Func<string, Task>? OnViewUserProfile { get; set; }

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
                Printer.PrintError("Something went wrong.");
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
            Printer.PrintPostPreview,
            Constants.DefaultPageSize,
            ViewPostAsync,
            sectionTitle: ConsoleMessages.FeedMessage,
            messageIfNoItemsFetched: ConsoleMessages.NoPostsInFeedMessage
            );
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
            Printer.PrintCommentPreview,
            Constants.DefaultPageSize,
            ViewCommentAsync,
            sectionTitle: "Comments",
            messageIfNoItemsFetched: ConsoleMessages.NoCommentsUnderPost(post.Title)
            );
    }

    private async Task ViewCommentAsync(DisplayCommentDto comment)
    {
        Printer.PrintCommentDetails(comment);
        Console.WriteLine();

        bool isOwner = comment.SenderUsername == _sessionUser.Username;

        Dictionary<string, Func<Task>> options = new()
        {
            { "View commenter profile", () => OnViewUserProfile!(comment.SenderUsername) }
        };
        if (isOwner)
        {
            options.Add("Delete comment", () => DeleteCommentAsync(comment.Id));
        }

        var labels = options.Keys.ToList();
        Printer.PrintLines(labels, "Back to comment section", true, false);
        int choice = Prompter.GetIntInput(string.Empty, 0, options.Count);

        if (choice != 0)
        {
            var operation = options.Values.ElementAt(choice - 1);
            await operation();
        }
    }

    private async Task DeleteCommentAsync(int commentId)
    {
        await ConfirmAction("Are you sure you want to delete a comment?", async () =>
        {
            var response = await _commentService.DeleteCommentAsync(commentId);
            if (response.Success)
            {
                Printer.PrintSuccess(   "Comment deleted");
            }
            else
            {
                Printer.PrintError("Could not delete comment. " + response.Message);
            }
        });
    }

    private async Task WriteCommentAsync(DisplayPostDto post)
    {
        var createCommentDto = DtoPrompter.Comment(_sessionUser.UserId, post.Id);
        var response = await _commentService.AddCommentAsync(createCommentDto);
        if (response.Success)
        {
            Printer.PrintSuccess("Comment uploaded");
        }
        else
        {
            Printer.PrintError("Failed to upload a comment. " + response.Message);
        }
        
    }

    private async Task DeletePostAsync(DisplayPostDto post)
    {
        await ConfirmAction($"Are you sure you want to delete post?", async () =>
        {
            var response = await _postService.DeletePostAsync(post.Id);
            
            if (response.Success)
            {
                Printer.PrintSuccess("Post deleted");
            }
            else
            {
                Printer.PrintError("Could not delete post. " + response.Message);
            }
        });
    }
    
    #endregion
    
    #region Create Post
    
    private async Task CreatePostAsync()
    {
        var postDto = DtoPrompter.Post(_sessionUser.UserId);
        var response = await _postService.UploadPost(postDto);

        if (response.Success)
        {
            Printer.PrintSuccess("Post uploaded!");
        }
        else
        {
            Printer.PrintError("Upload failed. " + response.Message);
        }
    }
    
    #endregion

    #region View Posts of a logged in user
    
    private async Task ViewOwnPostsAsync()
    {
        async Task<List<DisplayPostDto>> FetchPage(int pageNumber, int pageSize)
        {
            var response = await _postService.GetByUserIdAsync(_sessionUser.UserId, pageNumber, pageSize);
            return response.Data ?? [];
        }

        await BrowseAndSelectAsync(
            FetchPage,
            Printer.PrintPostPreview,
            Constants.DefaultPageSize,
            ViewPostAsync,
            sectionTitle: ConsoleMessages.OwnPostsMessage,
            messageIfNoItemsFetched: ConsoleMessages.NoOwnPosts);
    }
    
    #endregion

    #region View a post

    private async Task ViewPostAsync(DisplayPostDto post)
    {
        Printer.PrintPostDetails(post);
        Console.WriteLine();

        Dictionary<string, Func<Task>> viewPostOptions = new()
        {
            { "View Comments", () => ViewPostCommentsAsync(post) },
            { "Write Comment", () => WriteCommentAsync(post) },
            { "View Profile", () => OnViewUserProfile!(post.AuthorUsername) }
        };
        bool isOwner = post.AuthorUsername == _sessionUser.Username;
        if (isOwner)
        {
            viewPostOptions.Add("Delete post", () => DeletePostAsync(post));
        }

        var labels = viewPostOptions.Keys.ToList();
        Printer.PrintLines(labels, "Back to posts");
        int choice = Prompter.GetIntInput(string.Empty, 0, viewPostOptions.Count);

        if (choice != 0)
        {
            var operation = viewPostOptions.Values.ElementAt(choice - 1);
            await operation();
        }
    }

    #endregion
    
    
    /// <summary>
    /// might use this in friend menu if I figure out a way to wire the menus bidirectionally
    /// </summary>
    public async Task ViewUserPostsAsync(int userId)
    {
        var fetchInAdvance = await _postService.GetByUserIdAsync(userId, 1, Constants.DefaultPageSize);
        if (fetchInAdvance.Success)
        {
            async Task<List<DisplayPostDto>> FetchPage(int pageNumber, int pageSize)
            {
                var response = await _postService.GetByUserIdAsync(userId, pageNumber, pageSize);
                return response.Data ?? [];
            }

            await BrowseAndSelectAsync(
                FetchPage,
                Printer.PrintPostPreview,
                Constants.DefaultPageSize,
                ViewPostAsync
            );
        }
        else
        {
            Printer.PrintError("Failed to fetch posts. " + fetchInAdvance.Message);
        }
    }
}