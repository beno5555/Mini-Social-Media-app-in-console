using System.ComponentModel;
using System.Globalization;
using ProjectHelperLibrary.Utilities;
using social_media_console_app.BusinessLogic.Dtos.CommentDtos;
using social_media_console_app.BusinessLogic.Dtos.MessageDtos;
using social_media_console_app.BusinessLogic.Dtos.PostDtos;
using social_media_console_app.BusinessLogic.Dtos.UserDtos;
using social_media_console_app.Constants;

namespace social_media_console_app.Helpers;

public static class Printer
{
    public static void PrintUser(DisplayUserDto user, int index)
    {
        PrintLine(index, user.Username);
    }

    public static void PrintPost(DisplayPostDto post, int index)
    {
        string contentPreview = post.Content.Length > Constraints.PostContentPreviewLength
            ? post.Content[..Constraints.PostContentPreviewLength] + "..."
            : post.Content;
        PrintLine(index, $"{post.Title} - {contentPreview}");
    }
    
    public static void PrintComment(DisplayCommentDto comment, int index)
    {
        string commentBody = $"{comment.SenderUsername}: '{comment.Content}'";
        PrintLine(index, commentBody);
    }

    public static void PrintMessage(DisplayMessageDto message, int index)
    {
        Console.WriteLine();
    }

    public static void PrintLine(int index, string content)
    {
        Console.WriteLine($"{index}. {content}");
    }

    public static void PrintList<T>(List<T> list, Action<T, int> printAction)
    {
        if (!NoRecords(list))
        {
            for (int i = 0; i < list.Count; i++)
            {
                printAction(list[i], i + 1);
            }
        }
    }

    public static bool NoRecords<T>(List<T> list)
    {
        if (list.Count == 0)
        {
            Console.WriteLine($"There are no {typeof(T).Name} records.");
            return true;
        }

        return false;
    }

    public static void PrintLines(List<string> lines, string? lastLine)
    {
        PrintList(lines, PrintLine);
        if (lastLine is not null)
        {
            PrintLine(lastLine, 0);
        }
    }

    public static void PrintLine(string message, int index)
    {
        Console.WriteLine(index + ". " + message);
    }

    public static void PrintUserDetails(DisplayUserDto user)
    {
        Console.WriteLine($"\nUsername: {user.Username}");
        Console.WriteLine($"Bio: {(string.IsNullOrEmpty(user.Bio) ? "No bio" : user.Bio)}");
        Console.WriteLine($"Birthday: {user.DateOfBirth:yyyy-MM-dd}");
        Console.WriteLine($"Joined: {user.CreatedAt:yyyy-MM-dd}");
        
    }
    public static void PrintPostDetails(DisplayPostDto post)
    {
        Console.WriteLine($"\nUploaded by: {post.AuthorUsername} at {post.UploadedAt:yyyy-MM-dd}");
        Console.WriteLine($"Title: {post.Title}");
        Console.WriteLine($"Content: {post.Content}");
    }
    
}