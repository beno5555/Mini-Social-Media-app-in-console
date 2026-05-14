using social_media_console_app.BusinessLogic.Dtos;
using social_media_console_app.BusinessLogic.Dtos.UserDtos;
using social_media_console_app.Models;

namespace social_media_console_app.BusinessLogic.Mappers;

public class UserMapper 
{
    public User ToEntity(RegisterDto registerDto, string passwordHash, string passwordSalt)
    {
        return new User
        {
            Username = registerDto.Username,
            Email = registerDto.Email,
            PasswordHash = passwordHash,
            PasswordSalt = passwordSalt,
            Bio = registerDto.Bio,
            DateOfBirth = registerDto.DateOfBirth
        };
    }

    public DisplayUserDto ToDisplay(User user)
    {
        return new DisplayUserDto(user.Username, user.Bio, user.CreatedAt, user.DateOfBirth);
    }

    public SessionUser ToSessionUser(User user)
    {
        return new SessionUser
        {
            UserId = user.Id,
            Username = user.Username,
        };
    }

    public Friendship ToFriendship(int requesterId, int addresseeId)
    {
        return new Friendship
        {
            RequesterUserId = requesterId,
            AddresseeUserId = addresseeId,
        };
    }
}