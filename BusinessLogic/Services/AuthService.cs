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
    
    public AuthService(UserRepository userRepository, UserMapper userMapper, PasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _userMapper = userMapper;
        _passwordHasher = passwordHasher;
    }

    public async Task<Response> RegisterAsync(RegisterDto registerDto)
    {
        var registerResponse = Response.Ok();

        var emailExists    = await _userRepository.ExistsByEmailAsync(registerDto.Email);
        var usernameExists = await _userRepository.ExistsByUsernameAsync(registerDto.Username);

        if (emailExists)
        {
            registerResponse = Response.Fail($"user with email {registerDto.Email} already exists");
        }
        else if (usernameExists)
        {
            registerResponse = Response.Fail($"User with username {registerDto.Username} already exists");
        }
        else
        {
            var (hash, salt) = _passwordHasher.HashPassword(registerDto.Password);
            User userToRegister = _userMapper.ToEntity(registerDto, hash, salt);
            
            await _userRepository.AddAsync(userToRegister);
        }

        return registerResponse;
    }

    public async Task<Response<SessionUser>> LoginAsync(LoginDto loginDto)
    {
        Response<SessionUser> loginResponse;

        string message     = "Invalid username or password";
        var    userToLogin = await _userRepository.GetByUniqueIdentifierAsync(loginDto.UniqueIdentifier);

        if (userToLogin is not null)
        {
            bool validPassword = _passwordHasher.VerifyPassword(loginDto.Password, userToLogin.PasswordHash, userToLogin.PasswordSalt);
            
            if (validPassword)
            {
                var sessionUser = _userMapper.ToSessionUser(userToLogin);
                loginResponse = Response<SessionUser>.Ok(sessionUser);
            }
            else
            {
                loginResponse = Response<SessionUser>.Fail(message);
            }
        }
        else
        {
            loginResponse = Response<SessionUser>.Fail(message);
        }

        return loginResponse;
    }
    
    
}