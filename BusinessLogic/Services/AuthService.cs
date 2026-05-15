using social_media_console_app.BusinessLogic.Dtos;
using social_media_console_app.BusinessLogic.Dtos.UserDtos;
using social_media_console_app.BusinessLogic.Responses;
using social_media_console_app.BusinessLogic.Mappers;
using social_media_console_app.Models;
using social_media_console_app.Repositories;

namespace social_media_console_app.BusinessLogic.Services;

public class AuthService
{
    private readonly UserRepository _userRepository;
    private readonly UserMapper     _userMapper;
    private readonly PasswordHasher _passwordHasher;

    private const string LoginErrorMessage = "Invalid username or password";
    
    public AuthService(UserRepository userRepository, UserMapper userMapper, PasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _userMapper = userMapper;
        _passwordHasher = passwordHasher;
    }

    public async Task<Response> RegisterAsync(RegisterDto registerDto)
    {
        Response registerResponse = new Response();
        var      emailExists      = await _userRepository.ExistsByEmailAsync(registerDto.Email);

        if (!emailExists)
        {
            var usernameExists = await _userRepository.ExistsByUsernameAsync(registerDto.Username);
            
            if (!usernameExists)
            {
                var (hash, salt) = _passwordHasher.HashPassword(registerDto.Password);
                User userToRegister = _userMapper.ToEntity(registerDto, hash, salt);
            
                await _userRepository.AddAsync(userToRegister);
            }
            else
            {
                registerResponse.Fail("Username is already taken");
            }
        }
        else 
        {
            registerResponse.Fail("Email is already taken");
        }

        return registerResponse;
    }

    public async Task<Response<SessionUser>> LoginAsync(LoginDto loginDto)
    {
        var loginResponse = new Response<SessionUser>();
        var userToLogin = await _userRepository.GetByUniqueIdentifierAsync(loginDto.UniqueIdentifier);
        
        if (userToLogin is not null)
        {
            bool validPassword = _passwordHasher.VerifyPassword(loginDto.Password, userToLogin.PasswordHash, userToLogin.PasswordSalt);
            
            if (validPassword)
            {
                var sessionUser = _userMapper.ToSessionUser(userToLogin);
                loginResponse.Ok(sessionUser);
            }
            else
            {
                loginResponse.Fail(LoginErrorMessage);
            }
        }
        else
        {
            loginResponse.Fail(LoginErrorMessage);
        }

        return loginResponse;
    }
    
}