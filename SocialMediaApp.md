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

---

## Architecture

```
Entities / Models
DbContext + Configurations
Repositories (concrete classes, no interfaces)
Services
Menus
Program.cs (DI wiring)
```

---

## Entities

All entities except `Friendship` inherit from `BaseEntity`:

```csharp
public abstract class BaseEntity
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
```

| Entity     | Properties                                                                    |
|------------|-------------------------------------------------------------------------------|
| User       | Username, Email, PasswordHash, Bio                                            |
| Post       | UserId, Content                                                               |
| Comment    | UserId, PostId, Content                                                       |
| Friendship | RequesterId, AddresseeId, Status (enum: Pending/Accepted/Declined), CreatedAt |
| Message    | SenderId, ReceiverId, Content, IsRead                                         |

### Notes
- `Friendship` does not inherit `BaseEntity` — its PK is `(RequesterId, AddresseeId)`. `CreatedAt` is declared manually.
- `Message.SentAt` is removed — `CreatedAt` from `BaseEntity` serves as the sent timestamp.
- Enums (`FriendshipStatus`) are stored as strings in the database.

### Navigation Properties

- **User** — Posts, Comments, SentMessages, ReceivedMessages, SentFriendRequests, ReceivedFriendRequests
- **Post** — User, Comments
- **Comment** — User, Post
- **Friendship** — Requester (User), Addressee (User)
- **Message** — Sender (User), Receiver (User)

Navigation properties are defined in configurations for relationship mapping. They are not eagerly loaded by default unless explicitly included via `Query()` override or a dedicated repository method.

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
        BaseRepository.cs
        BaseEntityRepository.cs
    UserRepository.cs
    PostRepository.cs
    CommentRepository.cs
    FriendshipRepository.cs
    MessageRepository.cs
/BusinessLogic
    /Dtos
        /CommentDtos
            CreateCommentDto.cs
            DisplayCommentDto.cs
        /MessageDtos
            CreateMessageDto.cs
            DisplayMessageDto.cs
        /PostDtos
            CreatePostDto.cs
            DisplayPostDto.cs 
        /UserDtos
            DisplayUserDto.cs
            LoginDto.cs
            RegisterDto.cs
            SessionUser.cs
    /Mappers
        CommentMapper.cs
        MessageMapper.cs
        PostMapper.cs
        UserMapper.cs
    /Responses
        Response
        Response<T> : Response
    /Services
        AuthService.cs
        PostService.cs
        CommentService.cs - should we have a separate one or include comment functionality to PostService since Comments are coupled to posts?
        MessageService.cs
        FriendshipService.cs - possibly also implement conversation functionality that MessageSerivice should have?
/Menus - hesitant on this layout. have not decided yet. this is a placeholder
    MainMenu.cs
    AuthMenu.cs
    PostMenu.cs
    FriendMenu.cs
    MessageMenu.cs
Program.cs
```

---

## DbContext

Connection string is hardcoded in `OnConfiguring`. `ApplyConfigurationsFromAssembly` picks up all `IEntityTypeConfiguration` classes automatically.

---

## Repository Layer

### BaseRepository\<T\> where T : class

The root base class. Provides data access methods usable by all repositories regardless of PK shape.

- `Query()` — `protected virtual IQueryable<T>`; override in specific repositories to apply default `Include` chains
- `GetAllAsync()` — use only when the dataset is known to be small
- `GetWhereAsync(predicate, page?, pageSize?, orderBy? track?)` — optional pagination; `Skip/Take` translated to SQL; optional ordering; manual ef tracking toggle set to true
- `GetFirstAsync(predicate)` — returns `T?`
- `AddAsync(T entity)`
- `DeleteAsync(T entity)`

### BaseEntityRepository\<T\> where T : BaseEntity

Extends `BaseRepository<T>`. Adds methods that depend on a single integer PK.

- `GetByIdAsync(int id)` — uses `FindAsync`, checks change tracker before hitting DB
- `ExistsAsync(int id)` — translates to `SELECT 1 WHERE EXISTS`, does not load the entity
- `DeleteAsync(int id)` — fetches by id, delegates to `DeleteAsync(T entity)`

### Pagination

`GetWhereAsync` accepts optional `page` and `pageSize`. When both are provided, `Skip/Take` are applied and translated to SQL. When omitted, all matching rows are returned. Use pagination for large unbounded datasets (e.g. all posts). For user-scoped collections of known reasonable size (e.g. posts by a specific user), loading all and paging in memory is acceptable.

`Skip/Take` apply to the root entity only. Included collections cannot be paginated dynamically — use a fixed `Take` inside filtered include for preview scenarios, or query through the specific repository for full pagination.

### SaveChanges Strategy

`SaveChangesAsync` is called in the repository. For atomic multistep operations, wrap in a transaction via the shared scoped `AppDbContext` at the service layer. Within a transaction, `SaveChangesAsync` executes SQL immediately but does not commit until `CommitAsync`.

### User-facing Item Selection

Numbered lists are display-only. The selected number is used as a collection index once to get the `Id`. All subsequent operations use the `Id`.

---

## Specific Repositories

### UserRepository

Extends `BaseEntityRepository<User>`. No `Query()` override — User nav properties are never eagerly loaded by default due to their size.

Methods:
- `GetByUsernameAsync(string username)`
- `GetByEmailAsync(string email)`
- `GetWithPostsAsync(Expression<Func<User, bool>> predicate, int recentPostsCount = 10)` — loads user with N most recent posts via filtered include
- `GetWithPostsByUsernameAsync(string username, int recentPostsCount = 10)`

### PostRepository

Extends `BaseEntityRepository<Post>`. `Query()` includes `User` and `Comments`.

Methods:
- `GetByUserIdAsync(int userId, int? page, int? pageSize)`
- `GetFeedAsync(List<int> friendIds, int? page, int? pageSize)`

### CommentRepository

Extends `BaseEntityRepository<Comment>`. `Query()` includes `User` and `Post`.

Methods:
- `GetByPostIdAsync(int postId, int? page, int? pageSize)`
- `GetByUserIdAsync(int userId, int? page, int? pageSize)`

### FriendshipRepository

Extends `BaseRepository<Friendship>` (not `BaseEntityRepository` — composite PK). `Query()` includes `Requester` and `Addressee`.

Methods:
- `GetAsync(int userA, int userB)` — returns the relationship regardless of who is requester or addressee
- `GetAsync(int userId, FriendshipStatus? status)` — returns all friendships for a user, optionally filtered by status
- `GetPendingRequestsAsync(int userId)`
- `GetFriendsAsync(int userId)`
- `GetSentRequestsAsync(int userId)`
- `HasPendingRequestAsync(int userA, int userB)`

### MessageRepository

Extends `BaseEntityRepository<Message>`. `Query()` includes `Sender` and `Receiver`.

Methods:
- `GetConversationAsync(int userA, int userB, int? page, int? pageSize)`
- `GetUnreadAsync(int userId)`
- `MarkAsReadAsync(List<Message> messages)` — used when messages are already loaded; sets `IsRead = true` and calls `SaveChangesAsync` once
- `MarkConversationAsReadAsync(int senderId, int receiverId)` — used when marking as read without loading messages; uses `ExecuteUpdateAsync` for a single SQL `UPDATE` statement

---

## Result\<T\>

Services return `Result<T>` or `Result` instead of raw data or booleans. Repositories return raw data or `null` — they have no business context to determine whether a null result is an error.

```csharp
public class Response 
{
    public bool Success { get; }
    public string? Message { get; }
    
    public void Ok() => Success = true;
    public static Fail(string message) 
    {
        Success = false;
        Message = message;
    }
}

public class Result<T> : Result
{
    public T? Data { get; }
        
    public void Ok(T data) 
    {
        Success = true;
        Data = data;
    }
    public override void Fail(string message) 
    {
        Success = false;
        Message = message;
        Data = default;
    }
}

```

Menus only display — no business logic. Services interpret repository results, apply business rules, and return `Result` or `Result<T>`.

---

## DI Setup (Program.cs)

```csharp
var services = new ServiceCollection();

services.AddDbContext<AppDbContext>();

services.AddScoped<UserRepository>();
services.AddScoped<PostRepository>();
services.AddScoped<CommentRepository>();
services.AddScoped<FriendshipRepository>();
services.AddScoped<MessageRepository>();

services.AddScoped<AuthService>();
services.AddScoped<PostService>();
services.AddScoped<CommentService>();
services.AddScoped<FriendshipService>();
services.AddScoped<MessageService>();

services.AddScoped<MainMenu>();

var provider = services.BuildServiceProvider();

var mainMenu = provider.GetRequiredService<MainMenu>();
mainMenu.Show();
```

---

## Key Design Decisions

- No interfaces — concrete repository and service classes only
- Repository base split into `BaseRepository<T> where T : class` and `BaseEntityRepository<T> where T : BaseEntity` — allows composite PK entities like `Friendship` to share base methods without forcing a single integer PK
- `protected readonly AppDbContext _dbContext` on `BaseRepository` — _camelCase with `_` prefix for protected fields
- `Query()` virtual method on `BaseRepository` — specific repositories override to define default includes; all base methods use `Query()` so includes are applied consistently
- `FriendshipRepository` extends `BaseRepository<Friendship>` directly — composite PK prevents use of `BaseEntityRepository`
- Each entity has its own `IEntityTypeConfiguration` class, applied via `ApplyConfigurationsFromAssembly`
- `Friendship` uses composite PK `(RequesterId, AddresseeId)` — does not inherit `BaseEntity`
- `Friendship` and `Message` both have two FKs to `User` — configured with `OnDelete(DeleteBehavior.Restrict)` to avoid multiple cascade paths
- Messages are standalone (not attached to Friendship) — a conversation is derived via query
- `CreatedAt` defaults to `DateTime.UtcNow` in `BaseEntity`; declared manually on `Friendship`
- Enums stored as strings for DB readability - currently no separate model for `FriendshipStatus`
- `ExistsAsync` uses `AnyAsync` — translates to `SELECT 1 WHERE EXISTS`, more efficient than loading the entity
- `ExecuteUpdateAsync` used in `MarkConversationAsReadAsync` — single SQL `UPDATE` without loading entities; bypasses change tracker
- No early returns — a single `return` at the end of every method; branching is handled via `if/else`; `Result` is declared at the top of the method, mutated through the logic, and returned once at the end

---

## Implementation Steps

1. [x] Create project and install packages
2. [x] Create entities (inherit `BaseEntity`, `Role` enum on `User`)
3. [x] Create entity configurations (Fluent API)
4. [x] Create `AppDbContext`
5. [x] First migration and seed data
6. [x] `BaseRepository<T>` and `BaseEntityRepository<T>`
7. [x] Specific repositories
8. [x] Response\<T\>
9. [ ] Services - AuthService and PostService completed
10. [ ] Menus
11. [ ] Wire DI in `Program.cs`