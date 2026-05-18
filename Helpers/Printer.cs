using social_media_console_app.BusinessLogic.Dtos.CommentDtos;
using social_media_console_app.BusinessLogic.Dtos.MessageDtos;
using social_media_console_app.BusinessLogic.Dtos.PostDtos;
using social_media_console_app.BusinessLogic.Dtos.UserDtos;
using social_media_console_app.ProjectConstants;

namespace social_media_console_app.Helpers;

public static class Printer
{
    #region Previews
    public static void PrintUserPreview(DisplayUserDto user, int index)
    {
        PrintLine(index, user.Username);
    }

    public static void PrintPostPreview(DisplayPostDto post, int index)
    {
        string contentPreview = post.Content.Length > Constants.PostContentPreviewLength
            ? post.Content[..Constants.PostContentPreviewLength] + "..."
            : post.Content;
        PrintLine(index, $"{post.Title} - {contentPreview}");
    }
    
    public static void PrintCommentPreview(DisplayCommentDto comment, int index)
    {
        string commentContent = comment.Content.Length > Constants.CommentContentPreviewLength
            ? comment.Content[..Constants.CommentContentPreviewLength] + "..."
            : comment.Content;
        string commentBody = $"{comment.SenderUsername}: '{commentContent}'";
        PrintLine(index, commentBody);
    }
    
    #endregion
    
    #region Detailed
    
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

    public static void PrintCommentDetails(DisplayCommentDto comment)
    {
        Console.WriteLine($"\nUploaded by: {comment.SenderUsername} at {comment.SentAt:yyyy-MM-dd}");
        Console.WriteLine($"Content: {comment.Content}");
    }

    #endregion

    #region Messages
    
    public static void PrintMessages(List<DisplayMessageDto> messages, string currentUsername)
    {
        string?   previousUsename = null;
        DateTime? previousDate    = null;
        
        if (!NoRecords(messages))
        {
            foreach (var message in messages)
            {
                DateTime messageDate = message.SentAt.ToLocalTime().Date;

                if (previousDate is null || messageDate != previousDate.Value)
                {
                    PrintDaySeparator(messageDate);
                }
                
                PrintMessage(message, currentUsername, previousUsename);
                previousUsename = message.SenderUsername;
                previousDate = messageDate;
            }
        }
    }
    private static void PrintMessage(DisplayMessageDto message, string currentMessageUsername, string? previousMessageUsername = null) 
    {
        bool otherUser    = previousMessageUsername is not null && message.SenderUsername != previousMessageUsername;
        bool isOwnMessage = message.SenderUsername == currentMessageUsername;
        int  consoleWidth = Math.Min(Console.WindowWidth, Constants.ChatWidth);
        int  innerWidth   = consoleWidth - 4;
        string ownMessageIndent = isOwnMessage
            ? new string(' ', (int)(innerWidth * Constants.OwnMessageIndentPercent))
            : string.Empty;
        
        int maxWidth = isOwnMessage
            ? innerWidth
            : (int)(innerWidth * Constants.OtherMessageMaxWidthPercent);
        int indentLength = ownMessageIndent.Length;
        int contentWidth = maxWidth - indentLength;

        string authorPrefix  = isOwnMessage ? $"You ({message.SenderUsername})" : message.SenderUsername;
        string date          = $"[{message.SentAt.ToLocalTime():HH:mm}]";
        string separator     = " ";
        string header        = authorPrefix + separator + date;
        string headerPadding = new string(' ', innerWidth - indentLength - header.Length);
        
        string content       = message.MessageContent;
        string border        = Constants.ChatBorder.ToString();

        if (otherUser)
            Console.WriteLine(border + new string(' ', consoleWidth - 2) + border);

        // header line
        Console.Write(border + " " + ownMessageIndent);
        if (isOwnMessage)
        {
            PrintColored(date,         Constants.TimestampColor);
            PrintColored(separator,    Constants.MessageContentColor);
            PrintColored(authorPrefix, Constants.OwnMessageColor);
        }
        else
        {
            PrintColored(authorPrefix, Constants.OtherMessageColor);
            PrintColored(separator,    Constants.MessageContentColor);
            PrintColored(date,         Constants.TimestampColor);
        }

        Console.WriteLine(headerPadding + " " + border);

        // content lines
        int charsWritten = 0;
        while (charsWritten < content.Length)
        {
            int    charsToWrite = Math.Min(contentWidth, content.Length - charsWritten);
            string chunk        = content.Substring(charsWritten, charsToWrite);
            string padding      = new string(' ', innerWidth - indentLength - chunk.Length);
            charsWritten += charsToWrite;
            
            Console.Write(border + " " + ownMessageIndent);
            PrintColored(chunk, Constants.MessageContentColor);
            Console.WriteLine(padding + " " + border);
        }
    }

    public static void PrintChatBorder()
    {
        int consoleWidth = Math.Min(Console.WindowWidth, Constants.ChatWidth);
        Console.WriteLine(Constants.ChatBorder + new string('-', consoleWidth - 2) + Constants.ChatBorder);
    }
    
    private static void PrintColored(string text, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.Write(text);
        Console.ResetColor();
    }

    private static void PrintDaySeparator(DateTime date)
    {
        int    consoleWidth = Math.Min(Console.WindowWidth, Constants.ChatWidth);
        int    innerWidth   = consoleWidth - 4;
        string label        = date.ToLocalTime().ToString("dddd, MMMM d");
        int    totalPadding = innerWidth - label.Length;
        int    leftPadding  = totalPadding / 2;
        int    rightPadding = totalPadding - leftPadding;
        string border       = Constants.ChatBorder.ToString();

        Console.Write(border + " " + new string(' ', leftPadding));
        PrintColored(label, Constants.TimestampColor);
        Console.WriteLine(new string(' ', rightPadding) + " " + border);
    }
    
    #endregion

    #region Helpers
    
    #region Single line
    
    public static void PrintLine(int index, string content)
    {
        string indexTxt = index > 0 ? $"{index}. " : string.Empty;
        Console.WriteLine($"{indexTxt}{content}");
    }
    
    public static void PrintLine(string message, int index)
    {
        Console.WriteLine(index + ". " + message);
    }
    
    #endregion
    
    #region Collections

    public static void PrintList<T>(List<T> list, Action<T, int> printAction, bool showIndex = true, bool printIfNoRecords = true)
    {
        if (!NoRecords(list, printIfNoRecords))
        {
            for (int i = 0; i < list.Count; i++)
            {
                printAction(list[i], showIndex ? i + 1 : 0);
            }
        }
    }

    /// <summary>
    /// User for displaying menu options.
    /// </summary>
    public static void PrintLines(List<string> lines, string? lastLine, bool showIndex = true, bool printIfNoRecords = true)
    {
        PrintList(lines, PrintLine, showIndex, printIfNoRecords);
        if (lastLine is not null)
        {
            PrintLine(lastLine, 0);
        }
    }
    
    public static bool NoRecords<T>(List<T> list, bool printIfNoRecords = true)
    {
        if (list.Count == 0)
        {
            if (printIfNoRecords)
            {
                Console.WriteLine($"There are no {typeof(T).Name} records.");
            }
            return true;
        }

        return false;
    }
    
    #endregion
    
    #region Console key printing

    public static void PrintInputHints(Dictionary<ConsoleKey, string> hints)
    {
        var    hintsFormat = hints.Select(kvp => $"{GetKeyName(kvp.Key)} -> {kvp.Value}");
        string text        = string.Join(" | ", hintsFormat);
        
        Console.WriteLine(text);
    }

    private static string GetKeyName(ConsoleKey key)
    {
        string result = key.ToString();

        if (result.Length == 2 && result[0] == 'D' && char.IsDigit(result[1]))
        {
            result = result[1].ToString();
        }

        return result;
    }
    
    #endregion

    #endregion
}