using Microsoft.Extensions.DependencyInjection;
using social_media_console_app.BusinessLogic.Dtos;
using social_media_console_app.BusinessLogic.Mappers;
using social_media_console_app.BusinessLogic.Services;
using social_media_console_app.Data;
using social_media_console_app.Menus;
using social_media_console_app.Repositories;

var services = new ServiceCollection();

services.AddDbContext<ApplicationDbContext>();

services.AddScoped<CommentRepository>();
services.AddScoped<FriendshipRepository>();
services.AddScoped<MessageRepository>();
services.AddScoped<PostRepository>();
services.AddScoped<UserRepository>();

services.AddScoped<CommentMapper>();
services.AddScoped<MessageMapper>();
services.AddScoped<PostMapper>();
services.AddScoped<UserMapper>();

services.AddScoped<AccountService>();
services.AddScoped<CommentService>();
services.AddScoped<FriendshipService>();
services.AddScoped<MessageService>();
services.AddScoped<PostService>();
services.AddScoped<PasswordHasher>();
services.AddScoped<AuthService>();

services.AddScoped<SessionUser>();

services.AddScoped<AuthenticatedMenu>();
services.AddScoped<UnauthenticatedMenu>();
services.AddScoped<MainMenu>();

var serviceProvider = services.BuildServiceProvider();

var mainMenu = serviceProvider.GetRequiredService<MainMenu>();
await mainMenu.Run();