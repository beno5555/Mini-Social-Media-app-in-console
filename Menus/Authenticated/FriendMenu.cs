using social_media_console_app.BusinessLogic.Dtos;
using social_media_console_app.BusinessLogic.Dtos.UserDtos;
using social_media_console_app.BusinessLogic.Services;
using social_media_console_app.Helpers;
using social_media_console_app.Helpers.Exceptions;
using social_media_console_app.Menus.Base;
using social_media_console_app.Models;
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
                Console.WriteLine("Invalid input");
                break;
        }
    }
    
    #region View Friends

    private async Task ViewFriendsAsync()
    {
        async Task<List<DisplayUserDto>> FetchPage(int pageNumber, int pageSize) =>
            await _friendshipService.GetFriendsAsync(_sessionUser.UserId, pageNumber, pageSize);

        // add a back label parameter to an existing method.
        async Task FriendProfile(DisplayUserDto friend) =>
            await ViewFriendProfileAsync(friend, "Back to friends list");
        
        await BrowseAndSelectAsync(
            FetchPage,
            Printer.PrintUserPreview,
            Constants.DefaultPageSize,
            FriendProfile,
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
            ViewReceivedProfileAsync,
            sectionTitle: "Pending Requests",
            messageIfNoItemsFetched: ConsoleMessages.NoPendingRequestsMessage);
    }

    private async Task RespondToRequestAsync(DisplayUserDto requesterUser, FriendshipStatus newStatus)
    {
        var responseResponse = await _friendshipService.RespondToRequestAsync(requesterUser.Id, _sessionUser.UserId, newStatus);
                
        if (responseResponse.Success)
        {
            string action = newStatus == FriendshipStatus.Accepted ? "Accepted" : "Declined";
            Console.WriteLine($"{action} a request from '{requesterUser.Username}'.");
        }
        else
        {
            Console.WriteLine("Response failed. " + responseResponse.Message);
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
            ViewSentRequestProfileAsync,
            sectionTitle: "Sent Requests",
            messageIfNoItemsFetched: ConsoleMessages.NoSentRequestsMessage
        );
    }

    private async Task ManageSentRequestAsync(int addresseeUserId)
    {
        var cancelRequestResponse = await _friendshipService.RemoveRelationshipAsync(_sessionUser.UserId, addresseeUserId);
        
        if (cancelRequestResponse.Success)
        {
            Console.WriteLine("Request cancelled");
        }
        else
        {
            Console.WriteLine("Failed to cancel a request. " + cancelRequestResponse.Message);
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
                Console.WriteLine($"You and {friend.Username} are no longer friends :(");
            }
            else
            {
                Console.WriteLine("Could not delete user from friends list. " + deleteResponse.Message);
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

    private async Task ViewFriendProfileAsync(DisplayUserDto friend, string backLabel = "Back")
    {
        var viewFriendProfileOptions = GetDefaultViewProfileOptions(friend);
        viewFriendProfileOptions.Add("Remove from friends", () => RemoveFriendAsync(friend));
        await ViewUserProfileAsync(friend, viewFriendProfileOptions, backLabel);
    }
    
    public async Task ViewOwnProfileAsync(DisplayUserDto myself)
    {
        var viewOwnProfileOptions = GetDefaultViewProfileOptions(myself);
        viewOwnProfileOptions.Add("Change/Add bio", () => UpdateBioAsync(myself.Id));
        viewOwnProfileOptions.Add("Delete account", () => ConfirmDeleteAccountAsync(myself.Id));
        await ViewUserProfileAsync(myself, viewOwnProfileOptions);
    }

    private async Task ViewNonFriendProfileAsync(DisplayUserDto nonFriend)
    {
        var viewNonFriendProfileOptions = GetDefaultViewProfileOptions(nonFriend);
        viewNonFriendProfileOptions.Add("Send a friend request", () => SendRequestAsync(nonFriend.Id));
        await ViewUserProfileAsync(nonFriend, viewNonFriendProfileOptions);
    }

    private async Task ViewPendingFriendAsync(DisplayUserDto pendingFriend, Friendship relationship)
    {
        if (relationship.RequesterUserId == _sessionUser.UserId)
        {   
            await ViewSentRequestProfileAsync(pendingFriend);
        }
        else
        {
            await ViewReceivedProfileAsync(pendingFriend);
        }
    }

    private async Task ViewSentRequestProfileAsync(DisplayUserDto sentRequestUser)
    {
        var viewSentRequestUserProfileOptions = GetDefaultViewProfileOptions(sentRequestUser);
        viewSentRequestUserProfileOptions.Add("Cancel a request", () => ManageSentRequestAsync(sentRequestUser.Id));
        await ViewUserProfileAsync(sentRequestUser, viewSentRequestUserProfileOptions);
    }

    private async Task ViewReceivedProfileAsync(DisplayUserDto receivedRequestUser)
    {
        var viewReceivedProfileOptions = GetDefaultViewProfileOptions(receivedRequestUser);
        
        viewReceivedProfileOptions.Add("Accept",  () => RespondToRequestAsync(receivedRequestUser, FriendshipStatus.Accepted));
        viewReceivedProfileOptions.Add("Decline", () => RespondToRequestAsync(receivedRequestUser, FriendshipStatus.Declined));
        
        await ViewUserProfileAsync(receivedRequestUser, viewReceivedProfileOptions);
    }

    private async Task ViewRejecterProfileAsync(DisplayUserDto rejecterUser)
    {
        var viewRejecterProfileOptions = GetDefaultViewProfileOptions(rejecterUser);
        viewRejecterProfileOptions.Add("Your request has been rejected by this user. Resend a request", () => SendRequestAsync(rejecterUser.Id));
        await ViewUserProfileAsync(rejecterUser, viewRejecterProfileOptions);
    }

    private async Task ViewUnknownUserProfileAsync(DisplayUserDto searchedUser)
    {
        var relationship = await _friendshipService.GetRelationshipAsync(_sessionUser.UserId, searchedUser.Id);

        if (_sessionUser.UserId == searchedUser.Id)
        {
            await ViewOwnProfileAsync(searchedUser);
        }
        else
        {
            if (relationship is null)
            {
                await ViewNonFriendProfileAsync(searchedUser);
            }
            else if (relationship.FriendshipStatus == FriendshipStatus.Pending)
            {
                await ViewPendingFriendAsync(searchedUser, relationship);
            }
            else if (relationship.FriendshipStatus == FriendshipStatus.Accepted)
            {
                await ViewFriendProfileAsync(searchedUser);
            }
            else
            {
                await ViewRejecterProfileAsync(searchedUser);
            }
        }
    }
    
    /// <summary>
    /// public wrapper for other menus
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
            Console.WriteLine("User not found");
        }
    }

    #endregion
    
    
    private async Task SendRequestAsync(int addresseeId)
    {
        var requestResponse = await _friendshipService.SendRequest(_sessionUser.UserId, addresseeId);

        if (requestResponse.Success)
        {
            Console.WriteLine("Friend request sent!");
        }
        else
        {
            Console.WriteLine("Failed to send a friend request. " + requestResponse.Message);
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
                Console.WriteLine("Account deleted!");
                throw new AccountDeletedException();
            }
            else
            {
                Console.WriteLine("Could not delete an account. " +  deleteResponse.Message);
            }
        }
        else
        {
            Console.WriteLine("You can only delete your own account");
        }
    }

    private async Task UpdateBioAsync(int userId)
    {
        string bio      = Prompter.GetStringInput("New bio", 1, Constants.BioMaxLength);
        var    response = await _accountService.UpdateBioAsync(userId, bio);
        
        if (response.Success)
        {
            Console.WriteLine("Bio Updated!");
        }
        else
        {
            Console.WriteLine("Could not update bio. " +   response.Message);
        }
    }
}