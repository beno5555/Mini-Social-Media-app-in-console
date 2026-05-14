using social_media_console_app.BusinessLogic.Responses;
using social_media_console_app.Repositories;

namespace social_media_console_app.BusinessLogic.Services;

public class AccountSerice
{
    private readonly UserRepository _userRepository;

    public AccountSerice(UserRepository userRepository)
    {
        _userRepository = userRepository;
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
}