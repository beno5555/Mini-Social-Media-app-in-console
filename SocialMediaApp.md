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
- Comment on posts; delete own comments
- Send, accept, decline, and cancel friend requests; view friends list; remove friends; browse and search users
- Send and view messages between users; paginated conversation view with chat-style rendering

---

## Architecture

```
Entities / Models
DbContext + Configurations
Repositories (concrete classes, no interfaces)
Services
Helpers (Printer, Prompter, DtoPrompter, PaginatedInput, ConversationInput)
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
| Message    | SenderId, ReceiverId, MessageContent, IsRead, SentAt                          |

### Notes
- `Friendship` does not inherit `BaseEntity` — its PK is `(RequesterId, AddresseeId)`. `CreatedAt` is declared manually.
- Enums (`FriendshipStatus`) are stored as strings in the database.
- `User.DateOfBirth` validated via check constraint and at menu level. Age must be between 13 and 130.
- `Message` uses `SentAt` instead of `CreatedAt` and `MessageContent` instead of `Content` — not inheriting `BaseEntity` naming conventions for domain clarity.

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
/ProjectConstants
    Constants.cs
/Helpers
    /Inputs
        PaginatedInput.cs
        ConversationInput.cs
    Printer.cs
    Prompter.cs
    DtoPrompter.cs
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
- `ExecuteInTransactionAsync(Func<Task> operation)` — public; wraps operations in a DB transaction; safe to call from any repository since all share the same scoped `DbContext`

### BaseEntityRepository\<T\> where T : BaseEntity

Extends `BaseRepository<T>`. Adds:

- `GetByIdAsync(int id)` — uses `FindAsync`
- `ExistsAsync(int id)` — translates to `SELECT 1 WHERE EXISTS`
- `DeleteAsync(int id)`

### Pagination

`GetWhereAsync` accepts optional `page` and `pageSize`. `Skip/Take` applied and translated to SQL when both provided.

### SaveChanges Strategy

`SaveChangesAsync` called in the repository. For atomic multistep operations, wrap in a transaction via `ExecuteInTransactionAsync` at the service layer.

---

## Specific Repositories

### UserRepository
Extends `BaseEntityRepository<User>`. `Query()` includes posts and comments only.

- `GetByEmailAsync(string email)`
- `GetWithPostsAsync(Expression<Func<User, bool>> predicate, int recentPostsCount = 10)`
- `GetWithPostsByUsernameAsync(string username, int recentPostsCount = 10)`
- `SearchByUsernameAsync(string query, int excludeUserId, int? page, int? pageSize)`
- `GetFriendsByConversationStatusAsync(int userId, bool hasConversation, int? page, int? pageSize)` — single method with a flag; `true` returns friends with existing conversations ordered by most recently messaged; `false` returns friends with no conversation. Uses a single `GetWhereAsync` predicate querying `Friendships` and `Messages` directly — no in-memory filtering.

### PostRepository
Extends `BaseEntityRepository<Post>`. `Query()` includes `User` only — comments excluded to avoid change tracker conflicts on deletion.

- `GetByUserIdAsync(int userId, int? page, int? pageSize)`
- `GetFeedAsync(List<int> friendIds, int? page, int? pageSize)`
- `DeletePostCommentsAsync(int postId)` — uses `DeleteWhereAsync` (`ExecuteDeleteAsync`); called before post deletion in service

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

- `GetConversationAsync(int userA, int userB, int? page, int? pageSize)` — ordered by `SentAt` descending
- `HasUnreadAsync(int userId)` — returns `bool`
- `MarkAsReadAsync(List<Message> messages)`
- `MarkConversationAsReadAsync(int senderId, int receiverId)` — uses `ExecuteUpdateAsync`
- `DeleteUserMessagesAsync(int userId)` — uses `DeleteWhereAsync`

---

## Response\<T\>

Services return `Response<T>` or `Response` only when failure states are possible.

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

Empty collections are not a failure — `Ok()` with an empty list. Menus handle "no results" display. Services always populate `Message` on both success and failure paths where `Response` is used.

---

## DTOs

```csharp
public record DisplayUserDto(int Id, string Username, string? Bio, DateTime DateOfBirth, DateTime CreatedAt);
public record DisplayPostDto(int Id, string Username, string Content, DateTime CreatedAt, List<DisplayCommentDto>? Comments = null);
public record DisplayCommentDto(int Id, string Username, string Content, DateTime CreatedAt);
public record DisplayMessageDto(string MessageContent, string SenderUsername, DateTime SentAt, bool IsRead);
```

- `DisplayMessageDto` has no `Id` — no per-message actions exist in the menu layer.
- `DisplayPostDto.Comments` is nullable — `null` means not fetched; empty list means fetched with no results.
- `Username` is unique and used for ownership checks — no need to expose `UserId` in display DTOs.
- Internal IDs included in other DTOs for operation handling but never rendered in console output.

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
- `DeleteAccountAsync(int userId) → Task` — no failure state
  - Calls `DeleteUserRelatedDataAsync(int userId)` — deletes comments, messages, friendships via `DeleteWhereAsync` in that order

### PostService
- `CreatePostAsync(CreatePostDto dto) → Task<Response>`
- `DeletePostAsync(int postId) → Task<Response>` — wraps comment deletion and post deletion in `ExecuteInTransactionAsync`; both use `ExecuteDeleteAsync`
- `GetFeedAsync(int userId, int? page, int? pageSize) → Task<List<DisplayPostDto>>` — no failure state
- `GetByUserIdAsync(int userId, int? page, int? pageSize) → Task<Response<List<DisplayPostDto>>>`

### CommentService
- `AddCommentAsync(CreateCommentDto dto) → Task<Response>`
- `DeleteCommentAsync(int commentId, int userId) → Task<Response>`
- `GetByPostAsync(int postId, int? page, int? pageSize) → Task<Response<List<DisplayCommentDto>>>`

### FriendshipService
- `SendRequestAsync(int requesterId, int addresseeId) → Task<Response>`
- `RespondToRequestAsync(int requesterId, int addresseeId, int currentUserId, FriendshipStatus status) → Task<Response>`
- `RemoveRelationshipAsync(int userId, int friendId) → Task<Response>`
- `GetFriendsAsync(int userId, int? page, int? pageSize) → Task<Response<List<DisplayUserDto>>>`
- `GetPendingRequestsAsync(int userId, int? page, int? pageSize) → Task<Response<List<DisplayUserDto>>>`
- `GetSentRequestsAsync(int userId, int? page, int? pageSize) → Task<Response<List<DisplayUserDto>>>`

### MessageService
- `SendMessageAsync(CreateMessageDto dto) → Task<Response>`
- `GetConversationAsync(int currentUserId, int responderUserId, int? page, int? pageSize) → Task<Response<List<DisplayMessageDto>>>` — results reversed in memory per page so messages print oldest-to-newest within each page
- `HasUnreadAsync(int userId) → Task<bool>`
- `GetConversationFriendsAsync(int userId, int? page, int? pageSize) → Task<List<DisplayUserDto>>` — delegates to `UserRepository.GetFriendsByConversationStatusAsync(..., hasConversation: true)`
- `GetNonConversationFriendsAsync(int userId, int? page, int? pageSize) → Task<List<DisplayUserDto>>` — delegates to `UserRepository.GetFriendsByConversationStatusAsync(..., hasConversation: false)`

---

## Constants

```csharp
public static class Constants
{
    public const int EmailMinLength    = 10;
    public const int EmailMaxLength    = 100;
    public const int UsernameMinLength = EmailMinLength;
    public const int UsernameMaxLength = EmailMaxLength;

    public const int PasswordMinLength     = 6;
    public const int PasswordMaxLength     = 100;
    public const int PasswordHashMaxLength = 44;
    public const int PasswordSaltMaxLength = 44;

    public const int BioMaxLength             = 300;
    public const int PostContentMaxLength     = 3000;
    public const int PostContentPreviewLength = 100;
    public const int CommentMaxLength         = 500;
    public const int MessageMaxLength         = 1000;

    public const int MinAge = 13;
    public const int MaxAge = 130;

    public const int DefaultPageSize            = 3;
    public const int DefaultConversationPageSize = 20;

    public const string EmailRegexPattern    = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
    public const string UsernameRegexPattern = "^(?=.*[a-zA-Z])[a-zA-Z0-9._-]+$";

    public const int MenuBackTrackDelayInMilliseconds = 300;

    // Chat rendering
    public const int    ChatWidth                = 80;
    public const char   ChatBorder               = '|';
    public const double OwnMessageIndentPercent      = 0.3;
    public const double OtherMessageMaxWidthPercent  = 0.6;

    // Console colors
    public const ConsoleColor OwnMessageColor     = ConsoleColor.Cyan;
    public const ConsoleColor OtherMessageColor   = ConsoleColor.Green;
    public const ConsoleColor TimestampColor      = ConsoleColor.DarkGray;
    public const ConsoleColor MessageContentColor = ConsoleColor.White;
}
```

---

## Helpers

### Printer (static)

- `PrintUser(DisplayUserDto user, int index)`
- `PrintUserDetail(DisplayUserDto user)`
- `PrintPost(DisplayPostDto post, int index)`
- `PrintPostDetail(DisplayPostDto post)`
- `PrintComment(DisplayCommentDto comment, int index)`
- `PrintMessage(DisplayMessageDto message, string currentUsername, string? previousMessageUsername = null)` — chat-style aligned output; own messages indented by `OwnMessageIndentPercent`; other messages capped at `OtherMessageMaxWidthPercent`; content on new line below author + timestamp; long content wraps within bounds; colored output via `PrintColored`
- `PrintMessages(List<DisplayMessageDto> messages, string currentUsername)` — iterates messages, tracks previous sender for grouping separator and day separator
- `PrintChatBorder()` — prints `|---|` borderline at `ChatWidth`
- `PrintDaySeparator(DateTime date)` — centered date label inside chat borders; printed by `PrintMessages` when day changes between consecutive messages
- `PrintList<T>(List<T> list, Action<T, int> printAction, bool showIndex = true)` — passes `0` as index when `showIndex` is false; individual print methods treat `0` as "no index"
- `PrintLines(List<string> lines, string? lastLine)`
- `PrintLine(string message, int index)`
- `NoRecords<T>(List<T> list)` — prints "no records" and returns `true` if empty
- `PrintColored(string text, ConsoleColor color)` — private helper

### Prompter (static)

```csharp
public static string   GetStringInput(string prompt, int min, int max, string? regexPattern = null)
public static string?  GetOptionalStringInput(string prompt, int min, int max, string? regexPattern = null)
public static DateTime GetDateInput(string prompt, int minAge, int maxAge)
public static int      GetIntInput(string prompt, int min, int max)
public static bool     DoubleCheckIntent(string prompt)
public static PaginatedInput    GetPaginatedInput(int itemCount, bool hasPrevious, bool hasNext)
public static ConversationInput GetConversationInput(bool hasPrevious, bool hasNext)
```

`GetConversationInput` uses `Console.ReadKey(intercept: true)` — single keypress, no Enter required. Keys: `r` reply, `n` next/older, `p` previous/newer, `0` back.

### DtoPrompter (static)

Wraps prompting of specific DTO properties and returns the constructed DTO. Example:

- `Message(int senderId, int receiverId) → CreateMessageDto`

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

### ConversationInput (readonly struct)

Used exclusively by `MessageMenu.PaginateMessagesAsync`.

```csharp
public readonly struct ConversationInput
{
    public enum Kind { WriteMessage = 1, Next, Previous, BackToMenu }
    public Kind Type { get; }

    public static ConversationInput WriteMessage() => new(Kind.WriteMessage);
    public static ConversationInput Next()         => new(Kind.Next);
    public static ConversationInput Previous()     => new(Kind.Previous);
    public static ConversationInput BackToMenu()   => new(Kind.BackToMenu);
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
    ├── FriendMenu
    └── MessageMenu
```

### BaseMenu

Abstract base. All menus except for MainMenu inherit from it.

- `_sessionUser` — available to all menus
- `Title` — abstract; used as default section header
- `MenuOptions` — abstract `List<string>`; rendered by `Run()`
- `BackLabel` — virtual; defaults to `"Back to menu"`
- `ExitRoute` — always `0`
- `_exitOnBack` — `bool`; `Run()` returns this; `UnauthenticatedMenu` sets to `true` on exit
- `CompleteOperation(int choice)` — abstract; called by `Run()` for each non-exit choice
- `OnEnter(string? currentMenuMessage)` — virtual; clears console and prints title; `AuthenticatedMenu` overrides to check unread messages
- `OnBack()` — virtual hook; `AuthenticatedMenu` overrides to clear `SessionUser`
- `Run()` — virtual; prints options, reads choice, calls `CompleteOperation`; returns `_exitOnBack`
- `ConfirmAction(string prompt, Func<Task> action)` — calls `Prompter.DoubleCheckIntent`; executes action on confirmation

#### PaginateAsync

```csharp
protected async Task<T?> PaginateAsync<T>(
    Func<int, int, Task<List<T>>> fetchPage,
    Action<T, int>                printItem,
    int                           pageSize,
    string?                       sectionTitle     = null,
    bool                          shouldSelectItem = true)
```

- Caches pages in `Dictionary<int, List<T>>` per call
- `hasNext` inferred as `items.Count == pageSize`
- `shouldSelectItem = false` — disables item selection, numbering omitted via `PrintList(showIndex: false)`
- Returns `null` on `BackToMenu`; returns selected item on `Item`

#### BrowseAndSelectAsync

```csharp
protected async Task BrowseAndSelectAsync<T>(
    Func<int, int, Task<List<T>>> fetchPage,
    Action<T, int>                printItem,
    int                           pageSize,
    Func<T, Task>?                onSelect             = null,
    string?                       sectionTitle         = null,
    string?                       selectedSectionTitle = null)
```

- `onSelect` nullable — omit for read-only browsing
- After `onSelect` completes, loop re-enters `PaginateAsync` with a fresh cache
- `shouldSelectItem` derived from `onSelect is not null`

### PostMenu

Options: Create Post, View Feed, My Posts.

- **Create Post** — prompts content, calls `PostService.CreatePostAsync`
- **View Feed** — `BrowseAndSelectAsync` → `ViewPostAsync`
- **My Posts** — `BrowseAndSelectAsync` → `ViewPostAsync`

**ViewPostAsync(DisplayPostDto post)**
- Options: View Comments, Add Comment, Delete Post (owner only)
- **View Comments** — `BrowseAndSelectAsync` → `ViewCommentAsync`

**ViewCommentAsync(DisplayCommentDto comment)**
- Shows comment; if owner, offers Delete option; if not owner, shows Back only

### FriendMenu

Options: View Friends, Pending Requests, Sent Requests, Find Users, Remove Friend.

### MessageMenu

Options: See Conversations, Start a New Conversation.

- **See Conversations** — `BrowseAndSelectAsync` over friends with existing conversations → `OpenConversationAsync`
- **Start a New Conversation** — `BrowseAndSelectAsync` over friends with no conversation → `SendMessageAsync`

**OpenConversationAsync(DisplayUserDto otherUser)**
- Calls `BrowseMessagesAsync`

**BrowseMessagesAsync**
- Loop: calls `PaginateMessagesAsync`; if user chose to write, calls `onWriteMessage`; re-enters on reply, exits on back

**PaginateMessagesAsync**
- Own cache per call — fresh on each re-entry (ensures sent message appears on page 1 after reply)
- Prints chat borders and messages via `Printer.PrintChatBorder` and `Printer.PrintMessages`
- Input via `Prompter.GetConversationInput` — single keypress
- Returns `bool` — `true` if user chose to write a message

**SendMessageAsync(DisplayUserDto receiverUser)**
- Prompts via `DtoPrompter.Message`; calls `MessageService.SendMessageAsync`; no manual message print — conversation re-fetches on next `PaginateMessagesAsync` entry

---

## Menu flexibility

Right now different menus (post, message and friend menus) do not really talk to each other. I have not yet implemented a way to chain them bidirectionally without nested dependency hell. 

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
- `PostRepository.Query()` excludes comments — including them causes EF change tracker conflicts when deleting posts with `Restrict` on `Post → Comments`. `ExecuteDeleteAsync` bypasses the change tracker; wrapping in `ExecuteInTransactionAsync` ensures atomicity.
- `DeleteWhereAsync` uses `ExecuteDeleteAsync` — bypasses change tracker; no `SaveChangesAsync` needed; used for bulk cleanup.
- Manual cleanup order on account deletion: comments → messages → friendships → user. Posts cascade automatically.
- `ExecuteInTransactionAsync` is public on `BaseRepository` — callable from any injected repository in a service; safe because all repositories share the same scoped `DbContext`.
- `Response` wrapper omitted when no failure state is possible — `GetFeedAsync`, `SearchUsersAsync`, `HasUnreadAsync`, `DeleteAccountAsync`, `GetConversationFriendsAsync`, `GetNonConversationFriendsAsync` return raw data or `void`.
- `GetFriendsByConversationStatusAsync` in `UserRepository` — not in `MessageRepository`; the query fetches `User` entities and belongs to `UserRepository`. Not split into two methods — a `bool hasConversation` flag keeps the predicate and ordering in one place with minimal duplication.
- `GetConversationAsync` fetches descending, reversed per page in service — page 1 shows most recent messages; within a page messages print oldest-to-newest.
- `ConversationInput` separate from `PaginatedInput` — different input shape (single keypress vs numeric), different navigation semantics (older/newer vs next/previous), scoped to `MessageMenu` only.
- `DisplayMessageDto` has no `Id` — no per-message actions in the menu layer.
- `PrintList` `showIndex` flag — controls numbering without changing `Action<T, int>` delegate signature; print methods treat index `0` as "no index".
- Chat rendering uses percentage-based layout constants — `OwnMessageIndentPercent` and `OtherMessageMaxWidthPercent` overlap intentionally for a natural chat appearance. `ChatWidth` capped at `Math.Min(Console.WindowWidth, Constants.ChatWidth)`.
- `DtoPrompter` separates DTO construction prompting from general input prompting — `Prompter` handles primitives, `DtoPrompter` handles domain objects.
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
11. [x] `Constants`, `Printer`, `Prompter`, `DtoPrompter`, `PaginatedInput`, `ConversationInput`
12. [x] `BaseMenu`
13. [x] `UnauthenticatedMenu`, `MainMenu`, `AuthenticatedMenu`
14. [x] `FriendMenu`
15. [x] `PostMenu`
16. [x] `MessageMenu`
17. [ ] `Bidirectional Menu chaining`
18. [x] Wire DI in `Program.cs`