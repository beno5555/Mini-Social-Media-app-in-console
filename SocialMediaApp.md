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

- Register and login with password hashing
- Create, view, and delete posts
- Comment on posts; delete own comments
- Send, accept, decline, and cancel friend requests; view friends list; remove friends; browse and search users
- Send and view messages between friends; paginated conversation view with chat-style rendering
- View any user's profile with context-aware actions based on friendship status
- Cross-menu navigation — jump between posts, profiles, and conversations without retracing steps
- Universal home key — press `Escape` or type `exit` to return to the main menu from anywhere
- Delete account with full data cleanup

---

## Architecture

```
Models
DbContext + Configurations
Repositories
Services
Helpers
Menus
Program.cs
```

### Layers

**Models** — plain entity classes and enums.

**Repositories** — concrete classes, no interfaces. `BaseRepository<T>` and `BaseEntityRepository<T>` provide shared query, pagination, and transaction logic. Specific repositories extend these with domain queries.

**Services** — business logic. Consume repositories and mappers. Return `Response<T>` or `Response` when failure states are possible; raw values or `void` otherwise.

**Helpers** — `Printer` (static, console output), `Prompter` (static, input handling), `DtoPrompter` (static, DTO construction from user input), `PaginatedInput` and `ConversationInput` (input shape structs).

**Menus** — `BaseMenu` provides the run loop, pagination, and navigation scaffolding. Specific menus handle user-facing operations.

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
        /MessageDtos
        /PostDtos
        /UserDtos
    /Mappers
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

## Entities

All entities except `Friendship` and `Message` inherit from `BaseEntity`:

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

- `Friendship` does not inherit `BaseEntity` — its PK is `(RequesterId, AddresseeId)`.
- `FriendshipStatus` is stored as a string in the database.
- `User.DateOfBirth` is validated via check constraint and at menu level. Age must be between 13 and 130.
- `Message` uses `SentAt` and `MessageContent` instead of `BaseEntity` naming conventions for domain clarity.

### FK Cascade Behavior

SQL Server disallows multiple cascade paths. `User → Posts` and `Post → Comments` use `Cascade`. All other relationships use `Restrict` with manual cleanup on account deletion.

| Relationship                   | Behaviour |
|--------------------------------|-----------|
| User → Posts                   | Cascade   |
| Post → Comments                | Cascade   |
| User → Comments                | Restrict  |
| User → Messages (Sender)       | Restrict  |
| User → Messages (Receiver)     | Restrict  |
| User → Friendships (Requester) | Restrict  |
| User → Friendships (Addressee) | Restrict  |

Account deletion cleanup order: comments on other users' posts → messages → friendships → user. Posts and their comments cascade automatically.

---

## Repository Layer

### BaseRepository\<T\> where T : class

- `Query()` — `protected virtual IQueryable<T>`; override to apply default `Include` chains
- `GetAllAsync()`
- `GetWhereAsync(predicate, page?, pageSize?, orderBy?, track?)`
- `GetFirstAsync(predicate)` — returns `T?`
- `AddAsync(T entity)`
- `DeleteAsync(T entity)`
- `DeleteWhereAsync(predicate)` — uses `ExecuteDeleteAsync`; bypasses change tracker
- `ExecuteInTransactionAsync(Func<Task> operation)` — wraps operations in a DB transaction; safe to call from any repository since all share the same scoped `DbContext`
- `ClearTracker()` — detaches all tracked entities; used before bulk delete sequences to avoid change tracker conflicts

### BaseEntityRepository\<T\> where T : BaseEntity

Extends `BaseRepository<T>`. Adds:

- `GetByIdAsync(int id)`
- `FindAsync(int id)` — uses `FindAsync` directly without includes; used when a clean untracked instance is needed
- `ExistsAsync(int id)`
- `DeleteAsync(int id)`

### Pagination

`GetWhereAsync` accepts optional `page` and `pageSize`. `Skip/Take` is applied and translated to SQL when both are provided.

### SaveChanges Strategy

`SaveChangesAsync` is called in the repository. For atomic multistep operations, wrap in a transaction via `ExecuteInTransactionAsync` at the service layer. `ExecuteDeleteAsync` bypasses the change tracker and does not require `SaveChangesAsync`.

---

## Response\<T\>

Services return `Response<T>` or `Response` only when failure states are possible.

```csharp
public class Response
{
    public bool Success { get; set; } = true;
    public string? Message { get; set; }
}

public class Response<T> : Response
{
    public T? Data { get; set; }
}
```

- Empty collections are not failures — `Ok()` with an empty list.
- `Response` is omitted when no failure state is possible — methods return raw data or `void`.
- Services always populate `Message` on both success and failure paths where `Response` is used.

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

All menus except `MainMenu` inherit from `BaseMenu`.

- `Run()` — prints options, reads input, calls `CompleteOperation`; returns `_exitOnBack`
- `OnEnter(string? currentMenuMessage)` — clears console and prints title; `AuthenticatedMenu` overrides to check unread messages
- `OnBack()` — hook called on exit; `AuthenticatedMenu` overrides to clear `SessionUser`
- `PaginateAsync<T>` — generic paginator with per-call page cache
- `BrowseAndSelectAsync<T>` — wraps `PaginateAsync` with optional item selection and re-entry after action

### Cross-Menu Navigation

Menus expose nullable delegate properties for capabilities they need but do not own. `AuthenticatedMenu` wires all delegates in its constructor.

```
PostMenu.OnViewUserProfile    → FriendMenu.ViewUserProfileAsync
FriendMenu.OnViewUserPosts    → PostMenu.ViewUserPostsAsync
FriendMenu.OnOpenConversation → MessageMenu.OpenConversationAsync
MessageMenu.OnViewUserProfile → FriendMenu.ViewUserProfileAsync
```

`FriendMenu.ViewUserProfileAsync` resolves the correct profile view at call time by checking friendship status, then routing to `ViewFriendProfileAsync` or `ViewSearchedUserProfileAsync` accordingly. Options in each profile view are built dynamically — including context-aware friend request actions (send / cancel / accept / decline) based on the current relationship state.

### Navigation Exceptions

Two exceptions handle stack-unwinding navigation:

- `NavigateToRootException` — thrown from input handlers on `Escape` keypress or `exit` text input; caught by `AuthenticatedMenu.Run()` to snap back to the authenticated menu from any depth
- `AccountDeletedException` — thrown after successful account deletion; caught by `AuthenticatedMenu.Run()` to clear the session and return to `UnauthenticatedMenu`

---

## Helpers

### Printer (static)

Handles all console output. Print methods for users, posts, comments, and messages. Chat rendering uses percentage-based layout constants — own messages are right-indented, other messages are left-capped. `PrintMessages` handles day separators and sender grouping. `PrintList` accepts a `showIndex` flag — passes `0` as index when disabled; individual print methods treat `0` as no index.

### Prompter (static)

Handles all console input. `GetPaginatedInput` and `GetConversationInput` use single-keypress reads. All input methods check for `Escape` or `exit` and throw `NavigateToRootException` where applicable.

### DtoPrompter (static)

Wraps prompting of specific DTO fields and returns the constructed DTO.

---

## DI Setup

All repositories, mappers, services, and menus are registered as `Scoped`. `SessionUser` is scoped — mutated on login, cleared on logout or account deletion.

```csharp
services.AddDbContext<AppDbContext>();
// repositories, mappers, services, menus — all AddScoped
services.AddScoped<SessionUser>();
```

---

## Key Design Decisions

- No interfaces — concrete repository, service, and mapper classes only.
- `BaseRepository<T>` / `BaseEntityRepository<T>` split — composite PK entities share base methods without forcing an integer PK.
- `Query()` virtual override — specific repositories define their own default includes.
- `PostRepository.Query()` excludes comments — including them causes EF change tracker conflicts when deleting posts with `ExecuteDeleteAsync`.
- `DeleteWhereAsync` uses `ExecuteDeleteAsync` — bypasses change tracker; used for all bulk cleanup.
- `ExecuteInTransactionAsync` is public — callable from any injected repository in a service; safe because all repositories share the same scoped `DbContext`.
- `ClearTracker()` called before account deletion — prevents change tracker conflicts from navigation properties loaded earlier in the session.
- `Response` wrapper omitted when no failure state is possible.
- `GetFriendsByConversationStatusAsync` in `UserRepository` — single method with a `bool hasConversation` flag to avoid duplication.
- `GetConversationAsync` fetches descending, reversed per page in service — page 1 shows most recent; within a page messages print oldest-to-newest.
- `ConversationInput` separate from `PaginatedInput` — different input shape, different navigation semantics, scoped to `MessageMenu` only.
- `DisplayMessageDto` has no `Id` — no per-message actions exist.
- Cross-menu navigation via delegate properties — menus declare what they need, `AuthenticatedMenu` wires it. No circular dependencies.
- Navigation exceptions for stack unwinding — cleaner than threading a return flag or cancellation token through every method signature.
- No early returns — single `return` at end of every method; branching via `if/else`.
