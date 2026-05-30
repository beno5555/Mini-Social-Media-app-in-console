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
        PrintIndexPrefix(index);
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(user.Username);
        Console.ResetColor();
    }

    public static void PrintPostPreview(DisplayPostDto post, int index)
    {
        string contentPreview = post.Content.Length > Constants.PostContentPreviewLength
            ? post.Content[..Constants.PostContentPreviewLength] + "..."
            : post.Content;
        
        PrintIndexPrefix(index);
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write(post.Title);
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write(" - ");
        Console.Write(contentPreview);
        Console.ResetColor();
        Console.WriteLine();
    }

    public static void PrintCommentPreview(DisplayCommentDto comment, int index)
    {
        string commentContent = comment.Content.Length > Constants.CommentContentPreviewLength
            ? comment.Content[..Constants.CommentContentPreviewLength] + "..."
            : comment.Content;
        
        PrintIndexPrefix(index);
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write(comment.SenderUsername);
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write(": '");
        Console.ResetColor();
        Console.Write(commentContent);
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write("'");
        Console.ResetColor();
        Console.WriteLine();
    }

    #endregion

    #region Detailed

    public static void PrintUserDetails(DisplayUserDto user)
    {
        Console.WriteLine();
        PrintDetailLine("Username", user.Username);
        PrintDetailLine("Bio",      string.IsNullOrEmpty(user.Bio) ? "No bio" : user.Bio);
        PrintDetailLine("Birthday", user.DateOfBirth.ToString("yyyy-MM-dd"));
        PrintDetailLine("Joined",   user.CreatedAt.ToString("yyyy-MM-dd"));

    }

    public static void PrintPostDetails(DisplayPostDto post)
    {
        Console.WriteLine();
        
        PrintDetailLine("Uploaded by", $"{post.AuthorUsername} at {post.UploadedAt:yyyy-MM-dd}");
        PrintDetailLine("Title",   post.Title);
        PrintDetailLine("Content", post.Content);
    }

    public static void PrintCommentDetails(DisplayCommentDto comment)
    {
        Console.WriteLine();
        
        PrintDetailLine("Uploaded by", $"{comment.SenderUsername} at {comment.SentAt:yyyy-MM-dd}");
        PrintDetailLine("Content", comment.Content);
    }

    #endregion
    
    #region Menu
    public static void PrintSuccess(string message)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(message);
        Console.ResetColor();
    }
 
    public static void PrintError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(message);
        Console.ResetColor();
    }
 
    public static void PrintInfo(string message)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(message);
        Console.ResetColor();
    }

    public static void PrintWarning(string message)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(message);
        Console.ResetColor();
    }
    
    public static void PrintPage(int currentPage)
    {
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine($"Page - {currentPage}");
        Console.ResetColor();
        Console.WriteLine();
    }
 
    // ── Title header ──────────────────────────────────────────────────────────
 
    public static void PrintHeader(string title)
    {
        int    width  = Math.Max(title.Length + 4, 36);
        string top    = "╔" + new string('═', width) + "╗";
        string bottom = "╚" + new string('═', width) + "╝";
        string padded = title.PadLeft((width + title.Length) / 2).PadRight(width);
        string middle = "║" + padded + "║";
 
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(top);
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine(middle);
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(bottom);
        Console.ResetColor();
        Console.WriteLine();
    }

    
    #endregion

    #region ConsoleMessages

    public static (string? previousUsername, DateTime? previousDate) PrintMessages(
        List<DisplayMessageDto> messages,
        string currentUsername,
        string? previousUsername = null,
        DateTime? previousDate = null)
    {

        if (!NoRecords(messages, "You do not have any messages with this user."))
        {
            foreach (var message in messages)
            {
                DateTime messageDate = message.SentAt.ToLocalTime().Date;

                if (previousDate is null || messageDate != previousDate.Value)
                {
                    PrintDaySeparator(messageDate);
                }

                PrintMessage(message, currentUsername, previousUsername);
                previousUsername = message.SenderUsername;
                previousDate = messageDate;
                
            }
        }
            
        return (previousUsername, previousDate);
    }

    private static void PrintMessage(
        DisplayMessageDto message,
        string currentMessageUsername,
        string? previousMessageUsername = null)
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

        string content = message.MessageContent;
        string border  = Constants.ChatBorder.ToString();

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
        PrintIndexPrefix(index);
        Console.WriteLine(content);
    }

    public static void PrintLine(string message, int index)
    {
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write($"{index}. ");
        
        Console.ResetColor();
        Console.WriteLine(message);
    }

    #endregion

    #region Collections

    public static void PrintList<T>(
        List<T>        list,
        Action<T, int> printAction,
        bool           showIndex        = true,
        string?        emptyListMessage = null,
        bool           printIfNoRecords = true
        )
    {
        if (!NoRecords(list, emptyListMessage, printIfNoRecords))
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
    public static void PrintLines(List<string> lines, string? lastLine, bool showIndex = true,
        bool                                   printIfNoRecords = true)
    {
        PrintList(lines, PrintLine, showIndex, "No options.", printIfNoRecords);
        if (lastLine is not null)
        {
            PrintLine(lastLine, 0);
        }
    }

    private static bool NoRecords<T>(List<T> list, string? emptyListMessage = null, bool printIfNoRecords = true)
    {
        if (list.Count == 0)
        {
            if (printIfNoRecords)
            {
                Console.WriteLine(emptyListMessage ?? $"There are no {typeof(T).Name} records.");
            }

            return true;
        }

        return false;
    }

    #endregion

    #region Console key printing

    public static void PrintInputHints(Dictionary<ConsoleKey, string> hints)
    {
        bool first = true;
        foreach (var kvp in hints)
        {
            if (!first)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write(" | ");
                Console.ResetColor();
            }

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(GetKeyName(kvp.Key));
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(" -> ");
            Console.ResetColor();
            Console.Write(kvp.Value);
            first = false;
        }


        Console.WriteLine();
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
    
    #region Private Helpers

    private static void PrintDetailLine(string label, string value)
    {
        Console.ForegroundColor = ConsoleColor.DarkGray;
        
        Console.Write($"{label}: ");
        
        Console.ResetColor();
        Console.WriteLine(value);
    }
    private static void PrintIndexPrefix(int index)
    {
        if (index > 0)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write($"{index}. ");
            Console.ResetColor();
        }
    }
    
    #endregion

    #endregion

}