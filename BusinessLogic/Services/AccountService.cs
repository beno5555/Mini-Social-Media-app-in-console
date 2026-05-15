using social_media_console_app.BusinessLogic.Dtos.PostDtos;
using social_media_console_app.BusinessLogic.Dtos.UserDtos;
using social_media_console_app.BusinessLogic.Mappers;
using social_media_console_app.BusinessLogic.Responses;
using social_media_console_app.Repositories;

namespace social_media_console_app.BusinessLogic.Services;

public class AccountService
{
    private readonly UserRepository _userRepository;
    private readonly UserMapper _userMapper;

    public AccountService(UserRepository userRepository, UserMapper userMapper)
    {
        _userRepository = userRepository;
        _userMapper = userMapper;
    }

    public async Task<Response> DeleteAccountAsync(int userId)
    {
        var response = new Response();

        var userToDelete = await _userRepository.GetByIdAsync(userId);

        if (userToDelete is not null)
        {
            await _userRepository.DeleteAsync(userToDelete);
        }

        return response;
    }

    public async Task<List<DisplayUserDto>> GetUsersAsync(int currentUserId, int? pageNumber, int? pageSize)
    {
        var users    = await _userRepository.GetUsersAsync(currentUserId, pageNumber, pageSize);
        var userDtos = users
            .Select(_userMapper.ToDisplay)
            .ToList();
        return userDtos;
    }
}