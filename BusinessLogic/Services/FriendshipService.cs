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
                    if (relationship.FriendshipStatus == FriendshipStatus.Accepted)
                    {
                        response.Fail("You are already friends with this user");
                    }
                    else if (relationship.FriendshipStatus == FriendshipStatus.Pending)
                    {
                        response.Fail("A pending friend request already exists");
                    }
                    else if (relationship.FriendshipStatus == FriendshipStatus.Declined &&
                             relationship.RequesterUserId == requesterId)
                    {
                        await _friendshipRepository.UpdateStatusAsync(relationship, FriendshipStatus.Pending);
                    }
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

    public async Task<Response> RespondToRequestAsync(int requesterId, int addresseeId, FriendshipStatus status)
    {
        var response = new Response();

        return response;
    }
}