using System.ComponentModel;
using System.Globalization;
using ProjectHelperLibrary.Utilities;
using social_media_console_app.BusinessLogic.Dtos.CommentDtos;
using social_media_console_app.BusinessLogic.Dtos.MessageDtos;
using social_media_console_app.BusinessLogic.Dtos.PostDtos;
using social_media_console_app.BusinessLogic.Dtos.UserDtos;

namespace social_media_console_app.Helpers;

public static class Printer
{
    public static void PrintUser(DisplayUserDto user, int index)
    {
        Console.WriteLine($"{index}. {user.Username}");
    }

    public static void PrintComment(DisplayCommentDto comment, int index)
    {
        Console.WriteLine();
    }

    public static void PrintPost(DisplayPostDto post, int index)
    {
        
    }

    public static void PrintMessage(DisplayMessageDto message, int index)
    {
        Console.WriteLine();
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

    public static void PrintUserDetails(DisplayUserDto friend)
    {
        Console.WriteLine($"\nUsername: {friend.Username}");
        Console.WriteLine($"Bio: {(string.IsNullOrEmpty(friend.Bio) ? "No bio" : friend.Bio)}");
        Console.WriteLine($"Birthday: {friend.DateOfBirth:yyyy-MM-dd}");
        Console.WriteLine($"Joined: {friend.CreatedAt:yyyy-MM-dd}");
        
    }
}