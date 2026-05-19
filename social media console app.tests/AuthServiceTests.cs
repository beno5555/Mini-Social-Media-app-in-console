using social_media_console_app.BusinessLogic.Dtos.UserDtos;
using social_media_console_app.BusinessLogic.Mappers;
using social_media_console_app.BusinessLogic.Services;
using social_media_console_app.Data;
using social_media_console_app.Repositories;

namespace social_media_console_app.tests;

public class AuthServiceTests
{
    private AuthService CreateAuthService()
    {
        var context    = new ApplicationDbContext();
        var repo       = new UserRepository(context);
        var mapper     = new UserMapper();
        var hasher     = new PasswordHasher();
        return new AuthService(repo, mapper, hasher);
    }

    // ── Register ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task RegisterAsync_WithNewCredentials_Succeeds()
    {
        var service = CreateAuthService();
        var dto = new RegisterDto("testuser_new", "testuser_new@example.com", "password123", new DateTime(1995, 1, 1));

        var response = await service.RegisterAsync(dto);

        Assert.True(response.Success);
    }

    [Fact]
    public async Task RegisterAsync_WithExistingEmail_Fails()
    {
        var service = CreateAuthService();
        var dto = new RegisterDto("someotherusername", "someotherusername", "password123", new DateTime(1995, 1, 1));

        var response = await service.RegisterAsync(dto);

        Assert.False(response.Success);
        Assert.Equal("Email is already taken", response.Message);
    }

    [Fact]
    public async Task RegisterAsync_WithExistingUsername_Fails()
    {
        var service = CreateAuthService();
        var dto     = new RegisterDto("alice", "unique_email@example.com", "password123", new DateTime(1995, 1, 1));

        var response = await service.RegisterAsync(dto);

        Assert.False(response.Success);
        Assert.Equal("Username is already taken", response.Message);
    }

    // ── Login ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task LoginAsync_WithValidEmail_Succeeds()
    {
        var service = CreateAuthService();
        var dto     = new LoginDto("alice@example.com", "password123");

        var response = await service.LoginAsync(dto);

        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.Equal("alice", response.Data.Username);
    }

    [Fact]
    public async Task LoginAsync_WithValidUsername_Succeeds()
    {
        var service = CreateAuthService();
        var dto     = new LoginDto("alice", "password123");

        var response = await service.LoginAsync(dto);

        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.Equal("alice", response.Data.Username);
    }

    [Fact]
    public async Task LoginAsync_WithWrongPassword_Fails()
    {
        var service = CreateAuthService();
        var dto     = new LoginDto("alice", "wrongpassword");

        var response = await service.LoginAsync(dto);

        Assert.False(response.Success);
        Assert.Equal("Invalid username or password", response.Message);
    }

    [Fact]
    public async Task LoginAsync_WithNonExistentUser_Fails()
    {
        var service = CreateAuthService();
        var dto     = new LoginDto("nobody@example.com",  "password123");

        var response = await service.LoginAsync(dto);

        Assert.False(response.Success);
        Assert.Equal("Invalid username or password", response.Message);
    }
}