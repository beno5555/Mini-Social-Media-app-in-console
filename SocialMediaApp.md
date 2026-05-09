# Social Media Console App

A C# console application simulating a basic social media platform. Built with EF Core (code-first), layered architecture, and manual dependency injection.

---

## Tech Stack

- C# Console Application (.NET)
- Entity Framework Core (SQL Server, code-first)
- `Microsoft.Extensions.DependencyInjection`

## NuGet Packages

```
Microsoft.EntityFrameworkCore
Microsoft.EntityFrameworkCore.SqlServer
Microsoft.EntityFrameworkCore.Tools
Microsoft.EntityFrameworkCore.Design
Microsoft.Extensions.DependencyInjection
```

---

## Features

- Register and login (with password hashing)
- Create, view, and delete posts (text only)
- Comment on posts
- Send, accept, and decline friend requests; view friends list
- Send and view messages between users
- Minimal role-based access: `Admin` and `User` roles, checked inline where needed

---

## Architecture

```
Entities / Models
DbContext + Configurations
Repositories
Services
Menus
Program.cs (DI wiring)
```

---

## Entities

All entities inherit from `BaseEntity`:

```csharp
public abstract class BaseEntity
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
```

| Entity | Properties |
|---|---|
| User | Username, Email, PasswordHash, Bio, Role (enum: User/Admin) |
| Post | UserId, Content |
| Comment | UserId, PostId, Content |
| Friendship | RequesterId, AddresseeId, Status (enum: Pending/Accepted/Declined) |
| Message | SenderId, ReceiverId, Content, SentAt, IsRead |

### Navigation Properties

- **User** — Posts, Comments, SentMessages, ReceivedMessages, SentFriendRequests, ReceivedFriendRequests
- **Post** — User, Comments
- **Comment** — User, Post
- **Friendship** — Requester (User), Addressee (User)
- **Message** — Sender (User), Receiver (User)

---

## Folder Structure

```
/Models
    BaseEntity.cs
    User.cs
    Post.cs
    Comment.cs
    Friendship.cs
    FriendshipStatus.cs
    Message.cs
    Role.cs
/Data
    AppDbContext.cs
    /Configurations
        UserConfiguration.cs
        PostConfiguration.cs
        CommentConfiguration.cs
        FriendshipConfiguration.cs
        MessageConfiguration.cs
/Repositories
    /Base
        IBaseRepository.cs
        BaseRepository.cs
    IUserRepository.cs
    IPostRepository.cs
    ICommentRepository.cs
    IFriendshipRepository.cs
    IMessageRepository.cs
    UserRepository.cs
    PostRepository.cs
    CommentRepository.cs
    FriendshipRepository.cs
    MessageRepository.cs
/Services
    AuthService.cs
    PostService.cs
    CommentService.cs
    FriendshipService.cs
    MessageService.cs
/Menus
    MainMenu.cs
    AuthMenu.cs
    PostMenu.cs
    FriendMenu.cs
    MessageMenu.cs
Program.cs
```

---

## DI Setup (Program.cs)

```csharp
var services = new ServiceCollection();

services.AddDbContext(options =>
    options.UseSqlServer(connectionString));

services.AddScoped();
services.AddScoped();
services.AddScoped();
services.AddScoped();
services.AddScoped();

services.AddScoped();
services.AddScoped();
services.AddScoped();
services.AddScoped();
services.AddScoped();

services.AddScoped();

var provider = services.BuildServiceProvider();

var mainMenu = provider.GetRequiredService();
mainMenu.Show();
```

---

## Key Design Decisions

- `BaseRepository<T> where T : BaseEntity` — generic repository, specific repositories extend it
- Each entity has its own `IEntityTypeConfiguration` class, applied in `AppDbContext.OnModelCreating`
- `Friendship` has two FKs to `User` — configured explicitly via Fluent API with `OnDelete(DeleteBehavior.Restrict)` to avoid multiple cascade paths
- `Message` has two FKs to `User` (Sender, Receiver) — same cascade restriction applies
- Messages are standalone (not attached to Friendship) — a conversation is derived via query
- `CreatedAt` defaults to `DateTime.UtcNow` in `BaseEntity`
- Role checks are inline (`if currentUser.Role != Role.Admin`), no permission infrastructure

---

## Implementation Steps

1. [x] Create project and install packages
2. [ ] Create entities (rewrite to inherit `BaseEntity`, add `Role` enum to `User`)
3. [ ] Create entity configurations (Fluent API)
4. [ ] Create `AppDbContext`
5. [ ] First migration and seed data
6. [ ] `IBaseRepository<T>` and `BaseRepository<T>`
7. [ ] Specific repositories
8. [ ] Services
9. [ ] Menus
10. [ ] Wire DI in `Program.cs`
```