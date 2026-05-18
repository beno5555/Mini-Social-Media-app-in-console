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
- Send, accept, decline, and cancel friend requests; view friends list; remove friends; browse and search users
- Send and view messages between users

---

## Architecture

```
Entities / Models
DbContext + Configurations
Repositories (concrete classes, no interfaces)
Services
Helpers (Printer, Prompter, PaginatedInput)
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
| User       | Username, Email, PasswordHash, PasswordSalt, Bio, DateOfBirth                 |
| Post       | UserId, Content                                                               |
| Comment    | UserId, PostId, Content                                                       |
| Friendship | RequesterId, AddresseeId, Status (enum: Pending/Accepted/Declined), CreatedAt |
| Message    | SenderId, ReceiverId, Content, IsRead                                         |

### Notes
- `Friendship` does not inherit `BaseEntity` — its PK is `(RequesterId, AddresseeId)`. `CreatedAt` is declared manually.
- Enums (`FriendshipStatus`) are stored as strings in the database.
- `User.DateOfBirth` validated via check constraint and at menu level. Age must be between 13 and 130.

### FK Cascade Behavior

SQL Server disallows multiple cascade paths. Only `User → Posts` uses `Cascade`. All other relationships use `Restrict` with manual cleanup on account deletion.

| Relationship                   | Behaviour |
|--------------------------------|-----------|
| User → Posts                   | Cascade   |
| Post → Comments                | Restrict  |
| User → Comments                | Restrict  |
| User → Messages (Sender)       | Restrict  |
| User → Messages (Receiver)     | Restrict  |
| User → Friendships (Requester) | Restrict  |
| User → Friendships (Addressee) | Restrict  |

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
        AccountService.cs
        PostService.cs
        CommentService.cs
        MessageService.cs
        FriendshipService.cs
/Constants
    Constraints.cs
/Helpers
    PaginatedInput.cs
    Printer.cs
    Prompter.cs
/Menus
    /Base
        BaseMenu.cs
    MainMenu.cs
    UnauthenticatedMenu.cs
    /Authenticated
      AuthenticatedMenu.cs
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

- `Query()` — `protected virtual IQueryable<T>`; override to apply default `Include` chains
- `GetAllAsync()`
- `GetWhereAsync(predicate, page?, pageSize?, orderBy?, track?)`
- `GetFirstAsync(predicate)` — returns `T?`
- `AddAsync(T entity)`
- `DeleteAsync(T entity)`
- `DeleteWhereAsync(predicate)` — uses `ExecuteDeleteAsync`; bypasses change tracker; no `SaveChangesAsync` needed

### BaseEntityRepository\<T\> where T : BaseEntity

Extends `BaseRepository<T>`. Adds:

- `GetByIdAsync(int id)` — uses `FindAsync`
- `ExistsAsync(int id)` — translates to `SELECT 1 WHERE EXISTS`
- `DeleteAsync(int id)`

### Pagination

`GetWhereAsync` accepts optional `page` and `pageSize`. `Skip/Take` applied and translated to SQL when both provided.

### SaveChanges Strategy

`SaveChangesAsync` called in the repository. For atomic multistep operations, wrap in a transaction at the service layer.

---

## Specific Repositories

### UserRepository
Extends `BaseEntityRepository<User>`. `Query()` includes posts and comments only.

- `GetByUsernameAsync(string username)`
- `GetByEmailAsync(string email)`
- `GetWithPostsAsync(Expression<Func<User, bool>> predicate, int recentPostsCount = 10)`
- `GetWithPostsByUsernameAsync(string username, int recentPostsCount = 10)`
- `SearchByUsernameAsync(string query, int excludeUserId, int? page, int? pageSize)`

### PostRepository
Extends `BaseEntityRepository<Post>`. `Query()` includes `User` only — comments are not included by default to avoid change tracker conflicts on deletion.

- `GetByUserIdAsync(int userId, int? page, int? pageSize)`
- `GetFeedAsync(List<int> friendIds, int? page, int? pageSize)`
- `DeletePostCommentsAsync(int postId)` — uses `DeleteWhereAsync`; called before post deletion in service

### CommentRepository
Extends `BaseEntityRepository<Comment>`. `Query()` includes `User` and `Post`.

- `GetByPostIdAsync(int postId, int? page, int? pageSize)`
- `GetByUserIdAsync(int userId, int? page, int? pageSize)`
- `DeleteUserCommentsAsync(int userId)` — uses `DeleteWhereAsync`

### FriendshipRepository
Extends `BaseRepository<Friendship>` (composite PK). `Query()` includes `Requester` and `Addressee`.

- `GetRelationshipAsync(int userA, int userB)`
- `GetAsync(int userId, FriendshipStatus? status)`
- `GetPendingRequestsAsync(int userId)`
- `GetFriendsAsync(int userId)`
- `GetSentRequestsAsync(int userId)`
- `ExistsAsync(int userA, int userB, FriendshipStatus? status)`
- `UpdateStatusAsync(Friendship friendship, FriendshipStatus status)`
- `DeleteUserFriendshipsAsync(int userId)` — uses `DeleteWhereAsync`

### MessageRepository
Extends `BaseEntityRepository<Message>`. `Query()` includes `Sender` and `Receiver`.

- `GetConversationAsync(int userA, int userB, int? page, int? pageSize)`
- `HasUnreadAsync(int userId)` — returns `bool`
- `MarkAsReadAsync(List<Message> messages)`
- `MarkConversationAsReadAsync(int senderId, int receiverId)` — uses `ExecuteUpdateAsync`
- `DeleteUserMessagesAsync(int userId)` — uses `DeleteWhereAsync`

---

## Response\<T\>

Services return `Response<T>` or `Response` only when failure states are possible. When no failure state exists, raw data or `void` is returned directly — same reasoning as `HasUnreadAsync` and `SearchUsersAsync`.

```csharp
public class Response
{
    public bool Success { get; set; } = true;
    public string? Message { get; set; }

    public void Ok() => Success = true;
    public virtual void Fail(string message) { Success = false; Message = message; }
}

public class Response<T> : Response
{
    public T? Data { get; set; }

    public void Ok(T data) { Success = true; Data = data; }
    public override void Fail(string message) { Success = false; Message = message; Data = default; }
}
```

Empty collections are not a failure — `Ok()` with an empty list. Menus handle "no results" display.

Services always populate `Message` on both success and failure paths where `Response` is used.

---

## DTOs

```csharp
public record DisplayUserDto(int Id, string Username, string? Bio, DateTime DateOfBirth, DateTime CreatedAt);
public record DisplayPostDto(int Id, string Username, string Content, DateTime CreatedAt, List<DisplayCommentDto>? Comments = null);
public record DisplayCommentDto(int Id, string Username, string Content, DateTime CreatedAt);
public record DisplayMessageDto(int Id, string SenderUsername, string Content, bool IsRead, DateTime CreatedAt);
```

- `DisplayPostDto.Comments` is nullable — `null` means not fetched; empty list means fetched with no results.
- `Username` is unique and used for ownership checks (e.g. `post.Username == _sessionUser.Username`) — no need to expose `UserId` in display DTOs.
- Internal IDs included in DTOs for operation handling but never rendered in console output.

### SessionUser

Scoped. Mutated on login, cleared on logout.

```csharp
public class SessionUser
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public bool IsLoggedIn => UserId != 0;
}
```

---

## Services

### AuthService
- `RegisterAsync(RegisterDto dto) → Task<Response>`
- `LoginAsync(LoginDto dto) → Task<Response<SessionUser>>`

### AccountService
- `GetByUsernameAsync(string username) → Task<Response<DisplayUserDto>>`
- `SearchUsersAsync(string query, int currentUserId, int? page, int? pageSize) → Task<List<DisplayUserDto>>`
- `DeleteAccountAsync(int userId) → Task` — no failure state; `userId` always comes from `SessionUser`
  - Calls `DeleteUserRelatedDataAsync` before deleting the user
  - `private DeleteUserRelatedDataAsync(int userId)` — deletes comments, messages, friendships via `DeleteWhereAsync` in that order

### PostService
- `CreatePostAsync(CreatePostDto dto) → Task<Response>`
- `DeletePostAsync(int postId, int userId) → Task<Response>` — calls `DeletePostCommentsAsync` before deleting the post
- `GetFeedAsync(int userId, int? page, int? pageSize) → Task<List<DisplayPostDto>>` — no failure state; empty list returned when user has no friends or no posts
- `GetByUserIdAsync(int userId, int? page, int? pageSize) → Task<Response<List<DisplayPostDto>>>`

### CommentService
- `AddCommentAsync(CreateCommentDto dto) → Task<Response>`
- `DeleteCommentAsync(int commentId, int userId) → Task<Response>`
- `GetByPostAsync(int postId, int? page, int? pageSize) → Task<Response<List<DisplayCommentDto>>>`

### FriendshipService
- `SendRequestAsync(int requesterId, int addresseeId) → Task<Response>`
- `RespondToRequestAsync(int requesterId, int addresseeId, int currentUserId, FriendshipStatus status) → Task<Response>`
- `RemoveRelationshipAsync(int userId, int friendId) → Task<Response>` — used for both friend removal and request cancellation
- `GetFriendsAsync(int userId, int? page, int? pageSize) → Task<Response<List<DisplayUserDto>>>`
- `GetPendingRequestsAsync(int userId, int? page, int? pageSize) → Task<Response<List<DisplayUserDto>>>`
- `GetSentRequestsAsync(int userId, int? page, int? pageSize) → Task<Response<List<DisplayUserDto>>>`

### MessageService
- `SendMessageAsync(CreateMessageDto dto) → Task<Response>`
- `GetConversationAsync(int currentUserId, int responderUserId, int? page, int? pageSize) → Task<Response<List<DisplayMessageDto>>>`
- `HasUnreadAsync(int userId) → Task<bool>`

---

## Constants

```csharp
public static class Constraints
{
    public const int EmailMinLength    = 10;
    public const int EmailMaxLength    = 100;
    public const int UsernameMinLength = EmailMinLength;
    public const int UsernameMaxlength = EmailMaxLength;

    public const int PasswordMinLength     = 6;
    public const int PasswordMaxLength     = 100;
    public const int PasswordHashMaxLength = 44;
    public const int PasswordSaltMaxLength = 44;

    public const int BioMaxLength            = 300;
    public const int PostContentMaxLength    = 3000;
    public const int PostContentPreviewLength = 100;
    public const int CommentMaxLength        = 500;
    public const int MessageMaxLength        = 1000;

    public const int MinAge = 13;
    public const int MaxAge = 130;

    public const int DefaultPageSize      = 3;
    public const int ConversationPageSize = 20;

    public const string EmailRegexPattern    = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
    public const string UsernameRegexPattern = "^(?=.*[a-zA-Z])[a-zA-Z0-9._-]+$";

    public const int MenuBackTrackDelayInMilliseconds = 300;
}
```

---

## Helpers

### Printer (static)

- `PrintUser(DisplayUserDto user, int index)`
- `PrintUserDetail(DisplayUserDto user)`
- `PrintPost(DisplayPostDto post, int index)` — truncates content to `PostContentPreviewLength` chars followed by `...`
- `PrintPostDetail(DisplayPostDto post)` — full content; no comments (fetched separately on demand)
- `PrintComment(DisplayCommentDto comment, int index)`
- `PrintMessage(DisplayMessageDto message, int index)`
- `PrintList<T>(List<T> list, Action<T, int> printAction)`
- `PrintLines(List<string> lines, string? lastLine)`
- `PrintLine(string message, int index)`
- `NoRecords<T>(List<T> list)` — prints "no records" and returns `true` if empty

### Prompter (static)

```csharp
public static string   GetStringInput(string prompt, int min, int max, string? regexPattern = null)
public static string?  GetOptionalStringInput(string prompt, int min, int max, string? regexPattern = null)
public static DateTime GetDateInput(string prompt, int minAge, int maxAge)
public static int      GetIntInput(string prompt, int min, int max)
public static PaginatedInput GetPaginatedInput(int itemCount, bool hasPrevious, bool hasNext)
```

### PaginatedInput (readonly struct)

```csharp
public readonly struct PaginatedInput
{
    public enum Kind { Item, Next, Previous, BackToMenu }
    public Kind Type  { get; }
    public int  Index { get; }

    public static PaginatedInput Item(int index) => new(Kind.Item, index);
    public static PaginatedInput Next()          => new(Kind.Next);
    public static PaginatedInput Previous()      => new(Kind.Previous);
    public static PaginatedInput BackToMenu()    => new(Kind.BackToMenu);
}
```

---

## Menu Structure

```
MainMenu (outer loop)
├── UnauthenticatedMenu
│   ├── Register
│   └── Login
└── AuthenticatedMenu
    ├── PostMenu
    ├── FriendMenu (PostMenu injected; ViewUserPostsAsync called from profile views)
    └── MessageMenu [pending]
```

### BaseMenu

- `_sessionUser` on base — every authenticated menu needs it.
- `Run()` — `virtual`; returns `Task<bool>`; `false` for authenticated menus; `UnauthenticatedMenu` returns `true` on Exit.
- `OnBack()` — virtual hook; `AuthenticatedMenu` overrides to clear `SessionUser`.
- `OnEnter(string? sectionTitle)` — virtual hook; `AuthenticatedMenu` overrides to check unread messages.
- `BackLabel` — defaults to `"Back"`; `AuthenticatedMenu` overrides to `"Log Out"`.

#### PaginateAsync

```csharp
protected async Task<T?> PaginateAsync<T>(
    Func<int, int, Task<List<T>>> fetchPage,
    Action<T, int>                printItem,
    int                           pageSize,
    string?                       sectionTitle   = null,
    bool                          shouldSelectItem = true)
```

- Caches pages in `Dictionary<int, List<T>>` per call — previous page navigation does not re-fetch.
- `hasNext` inferred as `items.Count == pageSize`.
- `shouldSelectItem = false` for read-only lists — item selection disabled, pagination only.
- Returns `null` on `BackToMenu`; returns selected item on `Item`.

#### BrowseAndSelectAsync

```csharp
protected async Task BrowseAndSelectAsync<T>(
    Func<int, int, Task<List<T>>> fetchPage,
    Action<T, int>                printItem,
    int                           pageSize,
    Func<T, Task>?                onSelect     = null,
    string?                       sectionTitle = null)
```

- `onSelect` nullable — omit for read-only browsing; `shouldSelectItem` derived from `onSelect is not null`.
- After `onSelect` completes, loop re-enters `PaginateAsync`.
- `onSelect` accepts method groups when signature matches `Func<T, Task>`.

### PostMenu [complete]

Options: Create Post, View Feed, My Posts.

- **Create Post** — prompts content, calls `PostService.CreatePostAsync`.
- **View Feed** — `BrowseAndSelectAsync` → `ViewPostAsync`.
- **My Posts** — `BrowseAndSelectAsync` → `ViewPostAsync`.

**ViewPostAsync(DisplayPostDto post)**
- Prints post detail.
- Options: View Comments, Add Comment, Delete Post (owner only — checked via `post.Username == _sessionUser.Username`).
- Single action per selection; no internal loop — `BrowseAndSelectAsync` handles re-entry.

**ViewPostCommentsAsync(DisplayPostDto post)**
- `BrowseAndSelectAsync` with `onSelect = null` — read-only pagination.

**Public surface for FriendMenu:**
- `ViewUserPostsAsync(int userId)` — preflight call to `GetByUserIdAsync`; on failure prints message and returns; on success enters `BrowseAndSelectAsync`.

### FriendMenu [complete]

Options: View Friends, Pending Requests, Sent Requests, Find Users, Remove Friend.

Injects `PostMenu` — calls `_postMenu.ViewUserPostsAsync(user.Id)` from profile views.

### MessageMenu [pending]

Options: Send Message, View Conversation. Both actions select from friends list first. `ViewConversation` uses `Constraints.ConversationPageSize`.

---

### Cross-menu wiring (not yet implemented)

Profile views in `FriendMenu` currently stub the post-viewing section. A potential future approach is bidirectional menu injection — `FriendMenu` calling into `PostMenu` and vice versa. Deferred indefinitely; may never be implemented if the coupling cost outweighs the benefit.

---

## DI Setup (Program.cs)

```csharp
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

services.AddScoped<SessionUser>();

services.AddScoped<MainMenu>();
services.AddScoped<UnauthenticatedMenu>();
services.AddScoped<AuthenticatedMenu>();
services.AddScoped<PostMenu>();
services.AddScoped<FriendMenu>();
services.AddScoped<MessageMenu>();
```

---

## Key Design Decisions

- No interfaces — concrete repository, service, and mapper classes only.
- `BaseRepository<T>` / `BaseEntityRepository<T>` split — allows composite PK entities to share base methods without forcing integer PK.
- `Query()` virtual on `BaseRepository` — specific repositories override to define default includes.
- `PostRepository.Query()` excludes comments — including them causes EF change tracker conflicts when deleting posts with `Restrict` on `Post → Comments`.
- `DeleteWhereAsync` on `BaseRepository` uses `ExecuteDeleteAsync` — bypasses change tracker; no `SaveChangesAsync` needed; used for bulk cleanup.
- Manual cleanup order on account deletion: comments → messages → friendships → user. Posts cascade automatically.
- `DeletePostAsync` manually deletes comments before the post — required due to `Restrict` on `Post → Comments`.
- `Response` wrapper omitted when no failure state is possible — `GetFeedAsync`, `SearchUsersAsync`, `HasUnreadAsync`, `DeleteAccountAsync` return raw data or `void`.
- `GetFeedAsync` returns empty list for both no-friends and no-posts cases — distinction not surfaced to user.
- `GetByUserIdAsync` keeps `Response` — can be called with an external `userId` (e.g. from `FriendMenu`) where invalidity is possible.
- `Username` used for ownership checks — unique constraint makes it a valid identifier; avoids exposing `UserId` in display DTOs.
- `DisplayPostDto.Comments` nullable — `null` means not fetched; empty list means fetched with no results.
- `PostMapper.ToDisplay` has two overloads — with and without comments.
- `PrintPost` truncates content to `PostContentPreviewLength`; `PrintPostDetail` shows full content.
- `PaginateAsync` `shouldSelectItem = false` for read-only lists — item number input disabled.
- `BrowseAndSelectAsync` `onSelect` nullable — covers both read-only and action flows without separate methods.
- `hasNext` inferred as `items.Count == pageSize` — accepts one empty fetch on exact boundary.
- `Constraints` constants referenced in both check constraints and menu validation — single source of truth.
- No early returns — single `return` at end of every method; branching via `if/else`.

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
11. [x] `Constraints`, `Printer`, `Prompter`, `PaginatedInput`
12. [x] `BaseMenu`
13. [x] `UnauthenticatedMenu`, `MainMenu`, `AuthenticatedMenu`
14. [x] `FriendMenu`
15. [x] `PostMenu`
16. [ ] `MessageMenu`
17. [x] Wire DI in `Program.cs`