using System.Runtime.CompilerServices;
using ProjectHelperLibrary.Utilities;
using social_media_console_app.BusinessLogic.Dtos;
using social_media_console_app.BusinessLogic.Dtos.UserDtos;
using social_media_console_app.BusinessLogic.Mappers;
using social_media_console_app.BusinessLogic.Services;
using social_media_console_app.Constants;
using social_media_console_app.Constants.Enums;
using social_media_console_app.Helpers;
using social_media_console_app.Menus.Base;

namespace social_media_console_app.Menus.Authenticated;

public class FriendMenu : BaseMenu
{
    private readonly FriendshipService _friendshipService;
    private readonly AccountService    _accountService;
    
    private readonly UserMapper  _userMapper;

    protected override string Title => "Friends";

    protected override List<string> MenuOptions { get; } =
    [
        "View Friends",
        "Pending Requests",
        "Sent Requests",
        "Find Users",
        "Remove Friend",
    ];
    
    public FriendMenu(
        FriendshipService friendshipService,
        AccountService    accountService,
        UserMapper        userMapper,
        SessionUser       sessionUser
    ) : base (sessionUser)
    {
        _friendshipService = friendshipService;
        _accountService = accountService;
        
        _userMapper = userMapper;
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
        await BrowseAndSelectAsync<DisplayUserDto>(async (pageNumber, pageSize) =>
            {
                return await _friendshipService.GetFriendsAsync(_sessionUser.UserId, pageNumber, pageSize);
            },
            Printer.PrintUser,
            Constraints.DefaultPageSize,
            ViewFriendProfileAsync);
    }

    
    private async Task ViewFriendProfileAsync(DisplayUserDto friend)
    {
        Printer.PrintUserDetails(friend);
        Console.WriteLine();
        // post viewing will we implemented later
        
        List<string> viewFriendOptions =
        [
            "Remove from friends"
        ];
        Printer.PrintLines(viewFriendOptions, "Back to friend list");
        int choice = Prompter.GetIntInput(string.Empty, 0, viewFriendOptions.Count);
        
        if (choice == 1)
        {
            await ConfirmDeleteFriendAsync(friend);
        }
    }
    
    #endregion

    #region View Pending Requests
    
    private async Task ViewPendingRequestsAsync()
    {
        await BrowseAndSelectAsync(async (pageNumbe, pageSize) =>
                await _friendshipService.GetPendingRequestsAsync(_sessionUser.UserId, pageNumbe, pageSize),
            Printer.PrintUser,
            Constraints.DefaultPageSize,
            RespondToRequestAsync);
    }

    private async Task RespondToRequestAsync(DisplayUserDto requesterUser)
    {
        Printer.PrintUserDetails(requesterUser);
        Console.WriteLine();
        
        List<string> respondToRequestOptions =
        [
            "Accept",
            "Decline"
        ];
        
        Printer.PrintLines(respondToRequestOptions, "Back to requests list");
        int choice = Prompter.GetIntInput(string.Empty, 0, respondToRequestOptions.Count);

        if (choice != 0)
        {
            FriendshipStatus status = choice == 1 ? FriendshipStatus.Accepted : FriendshipStatus.Declined;
            var responseResponse = await _friendshipService.RespondToRequestAsync(requesterUser.Id, _sessionUser.UserId, status);
            
            if (responseResponse.Success)
            {
                string action = status == FriendshipStatus.Accepted ? "Accepted" : "Declined";
                Console.WriteLine($"{action} a request from '{requesterUser.Username}'. You are now friends!");
            }
            else
            {
                Console.WriteLine("Response failed. " + responseResponse.Message);
            }
        }
    }
    
    #endregion

    #region View Sent Requests
    
    private async Task ViewSentRequestsAsync()
    {
        await BrowseAndSelectAsync(async (pageNumber, pageSize) =>
            await _friendshipService.GetSentRequestsAsync(_sessionUser.UserId, pageNumber, pageSize),
            Printer.PrintUser,
            Constraints.DefaultPageSize,
            CancelRequestAsync
        );
    }

    private async Task CancelRequestAsync(DisplayUserDto addresseeUser)
    {
        Printer.PrintUserDetails(addresseeUser);
        Console.WriteLine();
        
        List<string> sentRequestsOptions =
        [
            "Cancel request"
        ];
        
        Printer.PrintLines(sentRequestsOptions, "Back to requests list");
        int choice = Prompter.GetIntInput(string.Empty, 0, sentRequestsOptions.Count);

        if (choice == 1)
        {
            var cancelRequestResponse = await _friendshipService.RemoveRelationshipAsync(_sessionUser.UserId, addresseeUser.Id);
            if (cancelRequestResponse.Success)
            {
                Console.WriteLine("Request cancelled");
            }
            else
            {
                Console.WriteLine("Failed to cancel a request. " + cancelRequestResponse.Message);
            }
        }
    }
    
    #endregion

    #region Find Users
    private async Task FindUsersAsync()
    {
        string usernameQuery = Prompter.GetStringInput("Search username", 1, Constraints.UsernameMaxlength);

        await BrowseAndSelectAsync(
            async (pageNumber, pageSize) => await _accountService.SearchUsersAsync(usernameQuery, pageNumber, pageSize),
            Printer.PrintUser,
            Constraints.DefaultPageSize,
            ViewSearchedUserProfileAsync
        );

    }

    private async Task ViewSearchedUserProfileAsync(DisplayUserDto searchedUser)
    {
        Printer.PrintUserDetails(searchedUser);
        Console.WriteLine();
        
        List<string> viewSearchedUserOptions =
        [
            "Send a friend request"
        ];
        
        Printer.PrintLines(viewSearchedUserOptions, "Back to friend list");
        int choice = Prompter.GetIntInput(string.Empty, 0, viewSearchedUserOptions.Count);
        if (choice == 1)
        {
            var requestResponse = await _friendshipService.SendRequest(_sessionUser.UserId, searchedUser.Id);

            if (requestResponse.Success)
            {
                Console.WriteLine("Friend request sent!");
            }
            else
            {
                Console.WriteLine("Failed to send a friend request. " + requestResponse.Message);
            }
        }
        
        ConsoleUtilities.ResetMenu();
    }
    #endregion

    #region Remove a friend
    private async Task RemoveFriendAsync()
    {
        await BrowseAndSelectAsync(
            async (pageNumber, pageSize) => await _friendshipService.GetFriendsAsync(_sessionUser.UserId, pageNumber, pageSize),
            Printer.PrintUser,
            Constraints.DefaultPageSize,
            ConfirmDeleteFriendAsync
        );
    }

    private async Task ConfirmDeleteFriendAsync(DisplayUserDto friend)
    {
        bool wantsToDelete = Prompter.DoubleCheckIntent($"Are you sure you want to remove '{friend.Username}' from friends?");

        if (wantsToDelete)
        {
            var deleteResponse = await _friendshipService.RemoveRelationshipAsync(_sessionUser.UserId, friend.Id); if (deleteResponse.Success)
            {
                Console.WriteLine($"You and {friend.Username} are no longer friends :(");
            }
            else
            {
                Console.WriteLine("Could not delete user from friends list. " + deleteResponse.Message);
            }
        }
    }
    
    #endregion
    

}