using social_media_console_app.BusinessLogic.Dtos.UserDtos;
using social_media_console_app.BusinessLogic.Mappers;
using social_media_console_app.BusinessLogic.Responses;
using social_media_console_app.Constants.Enums;
using social_media_console_app.Models;
using social_media_console_app.Repositories;

namespace social_media_console_app.BusinessLogic.Services;

public class FriendshipService
{
    private readonly FriendshipRepository _friendshipRepository;
    private readonly UserRepository       _userRepository;
    private readonly UserMapper           _userMapper;

    public FriendshipService(FriendshipRepository friendshipRepository, UserMapper userMapper, UserRepository userRepository)
    {
        _friendshipRepository = friendshipRepository;
        _userRepository = userRepository;
        _userMapper = userMapper;
    }

    public async Task<Response> SendRequest(int requesterId, int addresseeId)
    {
        var response = new Response();

        if (requesterId != addresseeId)
        {
            var addresseeExists = await _userRepository.ExistsByIdAsync(addresseeId);

            if (addresseeExists)
            {
                var relationship = await _friendshipRepository.GetRelationshipAsync(requesterId, addresseeId);

                if (relationship is null)
                {
                    var friendship = _userMapper.ToFriendship(requesterId, addresseeId);
                    await _friendshipRepository.AddAsync(friendship);
                }
                else
                {
                    response = await HandleExistingRelationship(relationship, requesterId);
                }
            }
            else
            {
                response.Fail("addressee not found");
            }
        }
        else
        {
            response.Fail("Friend request cannot be sent to oneself");
        }

        return response;
    }

    private async Task<Response> HandleExistingRelationship(Friendship relationship, int requesterId)
    {
        var response = new Response();
        
        if (relationship.FriendshipStatus == FriendshipStatus.Accepted)
        {
            response.Fail("You are already friends with this user");
        }
        else if (relationship.FriendshipStatus == FriendshipStatus.Pending)
        {
            response.Fail("A pending friend request already exists");
        }
        else if (relationship.FriendshipStatus == FriendshipStatus.Declined)
        {
            await _friendshipRepository.UpdateStatusAsync(relationship, FriendshipStatus.Pending);
        }

        return response;
    }

    public async Task<Response> RespondToRequestAsync(int requesterId, int addresseeId, FriendshipStatus status)
    {
        var response = new Response();

        if (ValidRequestResponse(status))
        {
            if (requesterId != addresseeId)
            {
                var friendship = await _friendshipRepository.GetRelationshipAsync(requesterId, addresseeId, true);

                if (friendship is not null && friendship.FriendshipStatus == FriendshipStatus.Pending)
                {
                    await _friendshipRepository.UpdateStatusAsync(friendship, status);
                }
                else
                {
                    response.Fail("No pending requests found");
                }
            }
            else
            {
                response.Fail("Cannot respond to a self-request");
            }
        }

        return response;
    }

    public async Task<Response> RemoveRelationshipAsync(int userId, int friendId)
    {
        var response = new Response();

        var friendship = await _friendshipRepository.GetRelationshipAsync(userId, friendId);

        if (friendship is not null)
        {
            await _friendshipRepository.DeleteAsync(friendship);
        }
        else
        {
            response.Fail("Friendship not found");
        }

        return response;
    }

    public async Task<List<DisplayUserDto>> GetFriendsAsync(int userId, int? pageNumber = null, int? pageSize = null)
    {
        return await FetchRelationshipsAsync(userId, _friendshipRepository.GetFriendshipsAsync, pageNumber, pageSize);
    }

    public async Task<List<DisplayUserDto>> GetPendingRequestsAsync(int userId, int? pageNumber,
        int?                                                                      pageSize)
    {
        return await FetchRelationshipsAsync(userId, _friendshipRepository.GetPendingRequestsAsync, pageNumber, pageSize);
    }
    
    public async Task<List<DisplayUserDto>> GetSentRequestsAsync(int userId, int? pageNumber, int? pageSize)
    {
        return await FetchRelationshipsAsync(userId, _friendshipRepository.GetSentRequestsAsync, pageNumber, pageSize);
    }

    private async Task<List<DisplayUserDto>> FetchRelationshipsAsync(int userId, Func<int, int?, int?, Task<List<Friendship>>> getAsync, int? pageNumber, int? pageSize)
    {
        var relationships = await getAsync(userId, pageNumber, pageSize);

        var friends = relationships
            .Select(relationship =>
                relationship.RequesterUserId == userId ? relationship.AddresseeUser : relationship.RequesterUser)
            .OfType<User>()
            .ToList();
    
        var userDtos =  friends.Select(_userMapper.ToDisplay).ToList();

        return userDtos;
    }

    public bool ValidRequestResponse(FriendshipStatus status) =>
        status is FriendshipStatus.Accepted or FriendshipStatus.Declined;
}