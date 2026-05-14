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

| Entity     | Properties                                                                                     |
|------------|------------------------------------------------------------------------------------------------|
| User       | Username, Email, PasswordHash, PasswordSalt, Bio, DateOfBirth                                 |
| Post       | UserId, Content                                                                                |
| Comment    | UserId, PostId, Content                                                                        |
| Friendship | RequesterId, AddresseeId, Status (enum: Pending/Accepted/Declined), CreatedAt                 |
| Message    | SenderId, ReceiverId, Content, IsRead                                                          |

### Notes
- `Friendship` does not inherit `BaseEntity` — its PK is `(RequesterId, AddresseeId)`. `CreatedAt` is declared manually.
- `Message.SentAt` is removed — `CreatedAt` from `BaseEntity` serves as the sent timestamp.
- Enums (`FriendshipStatus`) are stored as strings in the database.
- `User.DateOfBirth` is public. Validated at the database level via a check constraint and at the menu level. Constraint: age must be between 13 and 100.

```csharp
builder.ToTable("Users", t => t.HasCheckConstraint("CK_User_DateOfBirth",
    "DATEDIFF(year, DateOfBirth, GETUTCDATE()) BETWEEN 13 AND 100"));
```

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
        Response.cs
        Response<T>.cs
    /Services
        AuthService.cs
        PostService.cs
        CommentService.cs
        MessageService.cs
        FriendshipService.cs
/Menus — structure not finalised yet
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
- `GetWhereAsync(predicate, page?, pageSize?, orderBy?, track?)` — optional pagination; `Skip/Take` translated to SQL; optional ordering; EF tracking toggle defaults to true
- `GetFirstAsync(predicate)` — returns `T?`
- `AddAsync(T entity)`
- `DeleteAsync(T entity)`

### BaseEntityRepository\<T\> where T : BaseEntity

Extends `BaseRepository<T>`. Adds methods that depend on a single integer PK.

- `GetByIdAsync(int id)` — uses `FindAsync`, checks change tracker before hitting DB
- `ExistsAsync(int id)` — translates to `SELECT 1 WHERE EXISTS`, does not load the entity
- `DeleteAsync(int id)` — fetches by id, delegates to `DeleteAsync(T entity)`

### Pagination

`GetWhereAsync` accepts optional `page` and `pageSize`. When both are provided, `Skip/Take` are applied and translated to SQL. When omitted, all matching rows are returned. Use pagination for large unbounded datasets (e.g. all posts). For user-scoped collections of known reasonable size (e.g. posts by a specific user), loading all is acceptable.

`Skip/Take` apply to the root entity only. Included collections cannot be paginated dynamically — use a fixed `Take` inside filtered include for preview scenarios, or query through the specific repository for full pagination.

### SaveChanges Strategy

`SaveChangesAsync` is called in the repository. For atomic multistep operations, wrap in a transaction via the shared scoped `AppDbContext` at the service layer. Within a transaction, `SaveChangesAsync` executes SQL immediately but does not commit until `CommitAsync`.

### User-facing Item Selection

Numbered lists are display-only. The selected number is used as a collection index once to get the `Id`. All subsequent operations use the `Id`. Internal IDs are never displayed to the user.

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
- `GetRelationshipAsync(int userA, int userB)` — returns the relationship regardless of who is requester or addressee
- `GetAsync(int userId, FriendshipStatus? status)` — returns all friendships for a user, optionally filtered by status
- `GetPendingRequestsAsync(int userId)`
- `GetFriendsAsync(int userId)`
- `GetSentRequestsAsync(int userId)`
- `ExistsAsync(int userA, int userB, FriendshipStatus? status)` — checks existence optionally filtered by status; operator precedence handled by grouping the status condition: `(!status.HasValue || friendship.FriendshipStatus == status)`
- `UpdateStatusAsync(Friendship friendship, FriendshipStatus status)` — sets status and calls `SaveChangesAsync`; takes the already-fetched entity to avoid a redundant DB hit

### MessageRepository

Extends `BaseEntityRepository<Message>`. `Query()` includes `Sender` and `Receiver`.

Methods:
- `GetConversationAsync(int userA, int userB, int? page, int? pageSize)`
- `HasUnreadAsync(int userId)` — returns `bool`; translates to a single `AnyAsync` call; used for login notification
- `MarkAsReadAsync(List<Message> messages)` — used when messages are already loaded; sets `IsRead = true` and calls `SaveChangesAsync` once
- `MarkConversationAsReadAsync(int senderId, int receiverId)` — marks as read without loading messages; uses `ExecuteUpdateAsync` for a single SQL `UPDATE`

---

## Response\<T\>

Services return `Response<T>` or `Response` instead of raw data or booleans. Repositories return raw data or `null` — they have no business context to determine whether a null result is an error.

```csharp
public class Response
{
    public bool Success { get; set; }
    public string? Message { get; set; }

    public void Ok() => Success = true;
    public virtual void Fail(string message)
    {
        Success = false;
        Message = message;
    }
}

public class Response<T> : Response
{
    public T? Data { get; set; }

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

`Success` defaults to `true` on instantiation. `Fail` sets it to `false`. Menus only display — no business logic. Services interpret repository results, apply business rules, and return `Response` or `Response<T>`.

Empty collections are not a failure. Read operations that return no results call `response.Ok()` with an empty list. The menu is responsible for displaying "no results" messaging.

---

## DTOs

All DTOs used for display are records. Internal IDs are included in display DTOs for operation handling but are never rendered in console output.

```csharp
public record DisplayUserDto(int Id, string Username, string? Bio, DateTime DateOfBirth, DateTime CreatedAt);
public record DisplayPostDto(int Id, string Username, string Content, DateTime CreatedAt, List<DisplayCommentDto>? Comments = null);
public record DisplayCommentDto(int Id, string Username, string Content, DateTime CreatedAt);
public record DisplayMessageDto(int Id, string SenderUsername, string Content, bool IsRead, DateTime CreatedAt);
```

`DisplayPostDto.Comments` is nullable — `null` means comments were not fetched; an empty list means fetched with no results. The menu decides whether to load comments alongside the post or offer it as a separate action.

---

## Mappers

No interfaces — each mapper is a concrete class with whatever method signatures it needs. Mappers are pure property assignments with no injected dependencies and no logic.

### UserMapper
- `ToDisplay(User user) → DisplayUserDto`
- `ToFriendship(int requesterId, int addresseeId) → Friendship`
- `ToEntity(RegisterDto registerDto, string passwordHash, string passwordSalt) → User`

### PostMapper
- `ToDisplay(Post post) → DisplayPostDto` — maps without comments
- `ToDisplay(Post post, List<DisplayCommentDto> comments) → DisplayPostDto` — maps with comments
- `ToEntity(CreatePostDto createPostDto) → Post`

### CommentMapper
- `ToDisplay(Comment comment) → DisplayCommentDto`
- `ToEntity(CreateCommentDto createCommentDto) → Comment`

### MessageMapper
- `ToDisplay(Message message) → DisplayMessageDto`
- `ToEntity(CreateMessageDto createMessageDto) → Message`

---

## Services

### AuthService
- `RegisterAsync(RegisterDto dto) → Task<Response>`
- `LoginAsync(LoginDto dto) → Task<Response<SessionUser>>` — returns `SessionUser` on success; menu stores it as a private field

### PostService
- `CreatePostAsync(CreatePostDto dto) → Task<Response>`
- `DeletePostAsync(int postId, int userId) → Task<Response>`
- `GetFeedAsync(int userId, int? page, int? pageSize) → Task<Response<List<DisplayPostDto>>>`
- `GetByUserIdAsync(int userId, int? page, int? pageSize) → Task<Response<List<DisplayPostDto>>>`

### CommentService
- `AddCommentAsync(CreateCommentDto dto) → Task<Response>`
- `DeleteCommentAsync(int commentId, int userId) → Task<Response>`
- `GetByPostAsync(int postId, int? page, int? pageSize) → Task<Response<List<DisplayCommentDto>>>`

### FriendshipService
- `SendRequestAsync(int requesterId, int addresseeId) → Task<Response>`
    - Guards: cannot send to self, addressee must exist, no duplicate pending request, no accepted friendship
    - If a declined relationship exists (either direction), status is updated to `Pending` rather than inserting a new row (composite PK constraint)
    - Existing relationship handling extracted to `private HandleExistingRelationshipAsync`
- `RespondToRequestAsync(int requesterId, int addresseeId, int currentUserId, FriendshipStatus status) → Task<Response>`
    - Only `Accepted` or `Declined` are valid statuses
    - Only the addressee can respond
- `RemoveFriendAsync(int userId, int friendId) → Task<Response>`
- `GetFriendsAsync(int userId, int? page, int? pageSize) → Task<Response<List<DisplayUserDto>>>`
- `GetPendingRequestsAsync(int userId, int? page, int? pageSize) → Task<Response<List<DisplayUserDto>>>`
- `GetSentRequestsAsync(int userId, int? page, int? pageSize) → Task<Response<List<DisplayUserDto>>>`
- `private GetAsync(int userId, Func<int, int?, int?, Task<List<Friendship>>> query, int? page, int? pageSize)` — shared implementation used by the three Get methods above; takes a repository delegate to avoid duplication

### MessageService
- `SendMessageAsync(CreateMessageDto dto) → Task<Response>`
    - `SenderId` comes from the DTO; assumed valid since the caller is always a logged-in user
    - Validation extracted to `private ValidFriendshipAsync(int senderId, int receiverId)`: checks not self, receiver exists, accepted friendship
    - Messaging is friends-only
- `GetConversationAsync(int currentUserId, int responderUserId, int? page, int? pageSize) → Task<Response<List<DisplayMessageDto>>>`
    - Marks only messages where `ReceiverId == currentUserId` as read after fetch
- `HasUnreadAsync(int userId) → Task<bool>`
    - Returns a plain `bool`, not a `Response` — no failure state possible
    - Called after login to display "You have unread messages" notification

---

## Session Management

`AuthService.LoginAsync` returns a `Response<SessionUser>` on success. Each menu class holds a `private readonly SessionUser _sessionUser` field which is set via DI as a scoped service and its properties (id and username) are updated per login.

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

services.AddScoped<UserMapper>();
services.AddScoped<PostMapper>();
services.AddScoped<CommentMapper>();
services.AddScoped<MessageMapper>();

services.AddScoped<AuthService>();
services.AddScoped<AccountService>();
services.AddScoped<PostService>();
services.AddScoped<CommentService>();
services.AddScoped<FriendshipService>();
services.AddScoped<MessageService>();

services.AddScoped<MainMenu>();
services.AddScoped<UnauthenticatedMenu>();
services.AddScoped<AuthenticatedMenu>();
services.AddScoped<PostMenu>();
services.AddScoped<FriendMenu>();
services.AddScoped<MessageMenu>();

serivces.AddScoped<SessionUser>();

var provider = services.BuildServiceProvider();

var mainMenu = provider.GetRequiredService<MainMenu>();
mainMenu.Run();
```

---

## Key Design Decisions

- No interfaces — concrete repository, service, and mapper classes only
- Repository base split into `BaseRepository<T> where T : class` and `BaseEntityRepository<T> where T : BaseEntity` — allows composite PK entities like `Friendship` to share base methods without forcing a single integer PK
- `protected readonly AppDbContext _dbContext` on `BaseRepository` — camelCase with `_` prefix for protected fields
- `Query()` virtual method on `BaseRepository` — specific repositories override to define default includes; all base methods use `Query()` so includes are applied consistently
- `FriendshipRepository` extends `BaseRepository<Friendship>` directly — composite PK prevents use of `BaseEntityRepository`
- Each entity has its own `IEntityTypeConfiguration` class, applied via `ApplyConfigurationsFromAssembly`
- `Friendship` uses composite PK `(RequesterId, AddresseeId)` — does not inherit `BaseEntity`
- `Friendship` and `Message` both have two FKs to `User` — configured with `OnDelete(DeleteBehavior.Restrict)` to avoid multiple cascade paths
- Messages are standalone (not attached to Friendship) — a conversation is derived via query
- `CreatedAt` defaults to `DateTime.UtcNow` in `BaseEntity`; declared manually on `Friendship`
- Enums stored as strings for DB readability
- `ExistsAsync` uses `AnyAsync` — translates to `SELECT 1 WHERE EXISTS`, more efficient than loading the entity
- `ExecuteUpdateAsync` used in `MarkConversationAsReadAsync` — single SQL `UPDATE` without loading entities; bypasses change tracker
- `UpdateStatusAsync` on `FriendshipRepository` takes the already-fetched `Friendship` entity — avoids a redundant DB fetch since the entity is always in the change tracker at the call site
- `FriendshipRepository.ExistsAsync(int, int, FriendshipStatus?)` — nullable status parameter; when provided, filtered; operator precedence guarded by grouping: `(!status.HasValue || friendship.FriendshipStatus == status)`
- `HasUnreadAsync` returns `bool` directly — no failure state possible, `Response` wrapper unnecessary
- Empty collection results from read operations are `Ok`, not `Fail` — zero results is a valid state; menus handle the "no results" display
- `DisplayPostDto.Comments` is nullable — `null` means not fetched; empty list means fetched with no results
- Mappers are pure property assignments — no injected dependencies, no logic
- `PostMapper.ToDisplay` has two overloads — one without comments, one with `List<DisplayCommentDto>`; menu decides which to use based on context
- No early returns — a single `return` at the end of every method; branching via `if/else`; `Response` declared at the top, mutated through logic, returned once at the end
- Internal IDs never rendered in console output; used only for operation handling after user selection

---
## Menu Structure

`MainMenu` is the top-level controller. It owns the outer loop and decides whether to show `UnauthenticatedMenu` or `AuthenticatedMenu` based on whether `SessionUser` is populated.

```
MainMenu (outer loop)
├── UnauthenticatedMenu
│   ├── Register → returns to UnauthenticatedMenu on failure or success
│   └── Login → on success, MainMenu enters AuthenticatedMenu
└── AuthenticatedMenu
    ├── PostMenu
    ├── FriendMenu
    └── MessageMenu
```

Each menu has a `Run()` method with an internal loop. The loop displays options, reads input, and handles selection. It exits only when the user selects "back" or the equivalent. When `AuthenticatedMenu` calls `_postMenu.Show()`, it waits for `PostMenu` to return before resuming its own loop.

Logout lives in `AuthenticatedMenu`. It clears `SessionUser` properties and breaks the authenticated loop. `MainMenu` sees the session is empty and re-enters `UnauthenticatedMenu`.

### Menu Implementation Order

1. `UnauthenticatedMenu` — Register, Login
2. `MainMenu` — entry point, routes based on session state
3. `PostMenu` — create post, view feed, delete post, view comments
4. `FriendMenu` — send request, view pending/sent requests, accept/decline, view friends, remove friend
5. `MessageMenu` — send message, view conversation
---

## Implementation Steps

1. [x] Create project and install packages
2. [x] Create entities and models
3. [x] Create entity configurations (Fluent API)
4. [x] Create `AppDbContext`
5. [x] First migration and seed data
6. [x] `BaseRepository<T>` and `BaseEntityRepository<T>`
7. [x] Specific repositories
8. [x] `Response<T>`
9. [x] Mappers
10. [x] Services
11. [ ] Menus
12. [ ] Wire DI in `Program.cs`