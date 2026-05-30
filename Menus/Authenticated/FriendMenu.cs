using social_media_console_app.BusinessLogic.Dtos;
using social_media_console_app.BusinessLogic.Dtos.UserDtos;
using social_media_console_app.BusinessLogic.Services;
using social_media_console_app.Helpers;
using social_media_console_app.Helpers.Exceptions;
using social_media_console_app.Menus.Base;
using social_media_console_app.ProjectConstants;
using social_media_console_app.ProjectConstants.Enums;

namespace social_media_console_app.Menus.Authenticated;

public class FriendMenu : BaseMenu
{
    private readonly FriendshipService _friendshipService;
    private readonly AccountService    _accountService;

    public Func<int, Task>? OnViewUserPosts { get; set; }
    public Func<DisplayUserDto, Task>? OnOpenConversation { get; set; }
    

    protected override string Title => "Friends";

    protected override List<string> MenuOptions { get; } =
    [
        "View Friends",
        "Pending Requests",
        "Sent Requests",
        "Find Users",
        "Remove Friend",
    ];

    private Dictionary<string, Func<Task>> GetDefaultViewProfileOptions(DisplayUserDto user) => new()
    {
        {"View Posts", () => OnViewUserPosts!(user.Id) }
    };
    
    public FriendMenu(
        FriendshipService friendshipService,
        AccountService    accountService,
        SessionUser       sessionUser
    ) : base (sessionUser)
    {
        _friendshipService = friendshipService;
        _accountService = accountService;
        
    }

    protected override async Task CompleteOperation(int choice)
    {
        switch (choice)
        {
            case 1:
                await ViewFriendsAsync();
                break;
            case 2:
                await ViewPendingRequestsAsync();
                break;
            case 3:
                await ViewSentRequestsAsync();
                break;
            case 4:
                await FindUsersAsync();
                break;
            case 5:
                await RemoveFriendAsync();
                break;
            default:
                Printer.PrintError("Invalid input");
                break;
        }
    }
    
    #region View Friends

    private async Task ViewFriendsAsync()
    {
        async Task<List<DisplayUserDto>> FetchPage(int pageNumber, int pageSize) =>
            await _friendshipService.GetFriendsAsync(_sessionUser.UserId, pageNumber, pageSize);

        await BrowseAndSelectAsync(
            FetchPage,
            Printer.PrintUserPreview,
            Constants.DefaultPageSize,
            ViewUnknownUserProfileAsync,
            sectionTitle: "Friends",
            messageIfNoItemsFetched: ConsoleMessages.NoFriendsMessage);
    }
    
    #endregion
    
    #region View Pending Requests 
    
    private async Task ViewPendingRequestsAsync()
    {
        async Task<List<DisplayUserDto>> FetchPage(int pageNumber, int pageSize) =>
            await _friendshipService.GetPendingRequestUsersAsync(_sessionUser.UserId, pageNumber, pageSize);
            
        await BrowseAndSelectAsync(
            FetchPage,
            Printer.PrintUserPreview,
            Constants.DefaultPageSize,
            ViewUnknownUserProfileAsync,
            sectionTitle: "Pending Requests",
            messageIfNoItemsFetched: ConsoleMessages.NoPendingRequestsMessage);
    }

    private async Task RespondToRequestAsync(DisplayUserDto requesterUser, FriendshipStatus newStatus)
    {
        var responseResponse = await _friendshipService.RespondToRequestAsync(requesterUser.Id, _sessionUser.UserId, newStatus);
                
        if (responseResponse.Success)
        {
            string action = newStatus == FriendshipStatus.Accepted ? "Accepted" : "Declined";
            Printer.PrintSuccess($"{action} a request from '{requesterUser.Username}'.");
        }
        else
        {
            Printer.PrintError("Response failed. " + responseResponse.Message);
        }
    }
    
    #endregion
    
    #region View Sent Requests
    
    private async Task ViewSentRequestsAsync()
    {
        async Task<List<DisplayUserDto>> FetchPage(int pageNumber, int pageSize) =>
            await _friendshipService.GetSentRequestUsersAsync(_sessionUser.UserId, pageNumber, pageSize);
        
        await BrowseAndSelectAsync(
            FetchPage,
            Printer.PrintUserPreview,
            Constants.DefaultPageSize,
            ViewUnknownUserProfileAsync,
            sectionTitle: "Sent Requests",
            messageIfNoItemsFetched: ConsoleMessages.NoSentRequestsMessage
        );
    }

    private async Task ManageSentRequestAsync(int addresseeUserId)
    {
        var cancelRequestResponse = await _friendshipService.RemoveRelationshipAsync(_sessionUser.UserId, addresseeUserId);
        
        if (cancelRequestResponse.Success)
        {
            Printer.PrintSuccess("Request cancelled");
        }
        else
        {
            Printer.PrintError("Failed to cancel a request. " + cancelRequestResponse.Message);
        }
        
    }
    #endregion

    #region Find Users
    private async Task FindUsersAsync()
    {
        string usernameQuery = Prompter.GetStringInput("Search by username", 1, Constants.UsernameMaxlength);

        async Task<List<DisplayUserDto>> FetchPage(int pageNumber, int pageSize) =>
            await _accountService.SearchUsersAsync(usernameQuery, pageNumber, pageSize);

        await BrowseAndSelectAsync(
            FetchPage,
            Printer.PrintUserPreview,
            Constants.DefaultPageSize,
            ViewUnknownUserProfileAsync,
            ConsoleMessages.SearchResultsForUsernameMessage(usernameQuery),
            messageIfNoItemsFetched: ConsoleMessages.NoSearchResultsForUsernameMessage(usernameQuery)
        );

    }

    
    private async Task RemoveFriendAsync()
    {
        async Task<List<DisplayUserDto>> FetchPage(int pageNumber, int pageSize) =>
            await _friendshipService.GetFriendsAsync(_sessionUser.UserId, pageNumber, pageSize);
        
        await BrowseAndSelectAsync(
            FetchPage,
            Printer.PrintUserPreview,
            Constants.DefaultPageSize,
            RemoveFriendAsync,
            sectionTitle: ConsoleMessages.DeleteFriend,
            messageIfNoItemsFetched: ConsoleMessages.NoFriendsMessage
        );
    }

    private async Task RemoveFriendAsync(DisplayUserDto friend)
    {
        await ConfirmAction($"Are you sure you want to remove '{friend.Username}' from friends?", async () =>
        {
            var deleteResponse = await _friendshipService.RemoveRelationshipAsync(_sessionUser.UserId, friend.Id); if (deleteResponse.Success)
            {
                Printer.PrintSuccess($"You and {friend.Username} are no longer friends :(");
            }
            else
            {
                Printer.PrintError("Could not delete user from friends list. " + deleteResponse.Message);
            }
        });
    }
    
    #endregion
    
    #region User Profile
    
    /// <summary>
    /// method that takes the list of actions which the user can complete against the user whose profile is being viewed
    /// </summary>
    private async Task ViewUserProfileAsync(DisplayUserDto user, Dictionary<string, Func<Task>> options, string backLabel = "Back")
    {
        Printer.PrintUserDetails(user);
        Console.WriteLine();

        var labels = options.Keys.ToList();
        Printer.PrintLines(labels, backLabel);
        int choice = Prompter.GetIntInput(string.Empty, 0, labels.Count);

        if (choice != 0)
        {
            var operation = options.Values.ElementAt(choice - 1);
            await operation();
        }
    }

    /// <summary>
    /// adds specific operations in the profile menu based on the relationship the searched user has with the current user
    /// </summary>
    private async Task ConfigureUserProfileOptions(DisplayUserDto userProfile, Dictionary<string, Func<Task>> defaultOptions)
    {
        var relationship = await _friendshipService.GetRelationshipAsync(_sessionUser.UserId, userProfile.Id);
        
        if (_sessionUser.UserId == userProfile.Id)
        {
            defaultOptions.Add("Change/Add bio", () => UpdateBioAsync(userProfile.Id));
            defaultOptions.Add("Delete account", () => ConfirmDeleteAccountAsync(userProfile.Id));
        }
        else
        {
            if (relationship is null)
            {
                defaultOptions.Add("Send a friend request", () => SendRequestAsync(userProfile.Id));
            }
            else if (relationship.FriendshipStatus == FriendshipStatus.Pending)
            {
                if (relationship.RequesterUserId == _sessionUser.UserId)
                {   
                    defaultOptions.Add("Cancel a request", () => ManageSentRequestAsync(userProfile.Id));
                }
                else
                {
                    defaultOptions.Add("Accept",  () => RespondToRequestAsync(userProfile, FriendshipStatus.Accepted));
                    defaultOptions.Add("Decline", () => RespondToRequestAsync(userProfile, FriendshipStatus.Declined));
                }
            }
            else if (relationship.FriendshipStatus == FriendshipStatus.Accepted)
            {
                defaultOptions.Add("Remove from friends", () => RemoveFriendAsync(userProfile));
                defaultOptions.Add("Message", () => OnOpenConversation!(userProfile));
            }
            else
            {
                defaultOptions.Add("Your request has been rejected by this user. Resend a request", () => SendRequestAsync(userProfile.Id));
            }
        }
    }

    /// <summary>
    /// utilizes configure method to determine what operations current user can perform in the profile menu and passes those options alongside the user itself to a printer method.
    /// </summary>
    private async Task ViewUnknownUserProfileAsync(DisplayUserDto searchedUser)
    {
        var userProfileOptions = GetDefaultViewProfileOptions(searchedUser);
        await ConfigureUserProfileOptions(searchedUser, userProfileOptions);
        await ViewUserProfileAsync(searchedUser, userProfileOptions);
    }
    
    /// <summary>
    /// public view profile wrapper for other menus
    /// </summary>
    public async Task ViewProfileAsync(string username)
    {
        var response = await _accountService.GetByUsername(username);
                    
        if (response.Success && response.Data is not null)
        {
            await ViewUnknownUserProfileAsync(response.Data);
        }
        else
        {
            Printer.PrintError("User not found");
        }
    }

    #endregion
    
    private async Task SendRequestAsync(int addresseeId)
    {
        var requestResponse = await _friendshipService.SendRequest(_sessionUser.UserId, addresseeId);

        if (requestResponse.Success)
        {
            Printer.PrintSuccess("Friend request sent!");
        }
        else
        {
            Printer.PrintError("Failed to send a friend request. " + requestResponse.Message);
        }
    }

    private async Task ConfirmDeleteAccountAsync(int userToDeleteId)
    {
        await ConfirmAction("Are you sure you want to delete the account?", () => DeleteAccountAsync(userToDeleteId));
    }

    private async Task DeleteAccountAsync(int userToDeleteId)
    {
        if (userToDeleteId == _sessionUser.UserId)
        {
            var deleteResponse = await _accountService.DeleteAccountAsync(userToDeleteId);
            if (deleteResponse.Success)
            {
                Printer.PrintSuccess("Account deleted!");
                throw new AccountDeletedException();
            }
            else
            {
                Printer.PrintError("Could not delete an account. " +  deleteResponse.Message);
            }
        }
        else
        {
            Printer.PrintError("You can only delete your own account");
        }
    }

    private async Task UpdateBioAsync(int userId)
    {
        string bio      = Prompter.GetStringInput("New bio", 1, Constants.BioMaxLength);
        var    response = await _accountService.UpdateBioAsync(userId, bio);
        
        if (response.Success)
        {
            Printer.PrintSuccess("Bio Updated!");
        }
        else
        {
            Printer.PrintError("Could not update bio. " +   response.Message);
        }
    }
}