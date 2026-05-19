using System.Globalization;
using System.Text.RegularExpressions;
using ProjectHelperLibrary.Response;
using ProjectHelperLibrary.Utilities;
using ProjectHelperLibrary.Validations;
using social_media_console_app.Helpers.Inputs;
using social_media_console_app.ProjectConstants;

namespace social_media_console_app.Helpers;

public static class Prompter
{
    #region Integer
    public static int GetIntInput(string prompt, int min, int max)
    {
        var validation = new DataResponse<int>(false, 0, string.Empty);
        do
        {
            Console.Write(prompt);
            string? input = Console.ReadLine()?.Trim();

            if (int.TryParse(input, out int result))
            {
                validation = result.ProcessValidation(true, min, max);
                if (!validation.Success)
                {
                    Console.WriteLine(validation.Message);
                }
            }
            else if (input is not null && input.Equals(Constants.HomeCommand, StringComparison.OrdinalIgnoreCase))
            {
                throw new NavigateToMainMenuException();
            }
            else
            {
                Console.WriteLine("Input format was invalid");
            }
        } while (!validation.Success);

        return validation.Value;
    }

    public static int? GetOptionalIntInput(string prompt, int min, int max)
    {
        return null;
    }

    public static PaginatedInput GetPaginatedInput(int itemCount, bool hasPrevious, bool hasNext)
    {
        var hints = new List<string> { "0 -> back to menu" };
        
        if (hasPrevious) hints.Add("p -> previous page");
        if (hasNext) hints.Add("n -> next page");
        if (itemCount > 0) hints.Add($"(1-{itemCount}) -> select item");

        do
        {
            Console.WriteLine(string.Join(" | ", hints));
            string? input = Console.ReadLine()?.ToLower().Trim();

            if (string.IsNullOrEmpty(input))
            {
                Console.WriteLine("Input cannot be empty");
            }
            else if (int.TryParse(input, out int index))
            {
                if (index >= 1 && index <= itemCount)
                {
                   return PaginatedInput.Item(index);
                }
                if (index == 0)
                {
                    return PaginatedInput.BackToMenu();
                }
                   
                Console.WriteLine($"Input during selection should be in range: (0-{itemCount})");
            }
            else if (input.Equals("p") && hasPrevious)
            {
                return PaginatedInput.Previous();
            }
            else if (input.Equals("n") && hasNext)
            {
                return PaginatedInput.Next();
            }
            else if (input.Equals(Constants.HomeCommand, StringComparison.OrdinalIgnoreCase))
            {
                throw new NavigateToMainMenuException();
            }
            else
            {
                Console.WriteLine("Invalid input.");
            }

        } while (true);
    }

    public static ConversationInput GetConversationInput(bool hasPrevious, bool hasNext)
    {
        Dictionary<ConsoleKey, string> hints = new()
        {
            { ConsoleKey.D0, "Back to menu" },
            { ConsoleKey.M, "Send a message" },
            { ConsoleKey.P, "View User Profile" }
        };
        
        if (hasPrevious) hints.Add(ConsoleKey.O, "Older Messages");
        if (hasNext) hints.Add(ConsoleKey.N, "Newer Messages");

        do
        {
            Printer.PrintInputHints(hints);
            var keyInfo = ConsoleUtilities.WaitForKey(hints.Keys.ToArray());

            switch (keyInfo.Key)
            {
                case ConsoleKey.M:
                    return ConversationInput.WriteMessage();
                case ConsoleKey.O when hasPrevious:
                    return ConversationInput.Older();
                case ConsoleKey.N when hasNext:
                    return ConversationInput.Newer();
                case ConsoleKey.P:
                    return ConversationInput.UserProfile();
                case ConsoleKey.D0:
                    return ConversationInput.BackToMenu();
                case Constants.HomeKey:
                    throw new NavigateToMainMenuException();
                default:
                    Console.WriteLine("invalid input");
                    break;
            }
        } while (true);
    }

    #endregion
    
    #region String
    public static string GetStringInput(string prompt, int min, int max, string? regexPattern = null)
    {
        do
        {
            Console.Write($"{prompt}: ");
            string? input = Console.ReadLine()?.Trim();
            
            if (string.IsNullOrEmpty(input))
            {
                Console.WriteLine("Input cannot be empty");
            }
            else if (input.Equals(Constants.HomeCommand, StringComparison.OrdinalIgnoreCase))
            {
                throw new NavigateToMainMenuException();
            }
            else if (input.Length < min)
            {
                Console.WriteLine("Input cannot contain less than " + min + " characters");
            }
            else if (input.Length > max)
            {
                Console.WriteLine("Input cannot contain more than " + max + " characters");
            }
            else if (regexPattern is not null && !Regex.IsMatch(input, regexPattern))
            {
                Console.WriteLine("Invalid format");
            }
            else
            {
                return input;
            }

            Console.WriteLine();
        } while (true);
    }

    public static string? GetOptionalStringInput(string prompt, int min, int max, string? regexPattern = null)
    {
        do
        {
            Console.Write($"{prompt} (Optional, press {ConsoleKey.Enter} to skip): ");
            string? input = Console.ReadLine()?.Trim();
        
            if (string.IsNullOrEmpty(input))
            {
                return null;
            }
            
            if (input.Equals(Constants.HomeCommand, StringComparison.OrdinalIgnoreCase))
            {
                throw new NavigateToMainMenuException();
            }
            
            if (input.Length < min)
            {
                Console.WriteLine("Input cannot contain less than " + min + " characters");
            }
            else if (input.Length > max)
            {
                Console.WriteLine("Input cannot contain more than " + max + " characters");
            }
            else if (regexPattern is not null && !Regex.IsMatch(input, regexPattern))
            {
                Console.WriteLine("Invalid format");
            }
            else
            {
                return input;
            }

            Console.WriteLine();

        } while (true);
    }
    
    #endregion
    
    #region Date

    public static DateTime GetDateInput(string prompt, int minAge, int maxAge)
    {
        DateTime? result  = null;
        
        var minDate = DateTime.Today.AddYears(-maxAge);
        var maxDate = DateTime.Today.AddYears(-minAge);

        do
        {
            Console.Write($"{prompt} (yyyy-MM-dd): ");
            string input = Console.ReadLine() ?? string.Empty;

            if (DateTime.TryParseExact(input, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None,
                    out DateTime date))
            {
                if (date > maxDate)
                {
                    Console.WriteLine($"You must be at least {minAge} years old.");
                }
                else if (date < minDate)
                {
                    Console.WriteLine($"You cannot be older than {maxAge} years old.");
                }
                else
                {
                    result = date;
                }
            }
            else if (input.Equals(Constants.HomeCommand, StringComparison.OrdinalIgnoreCase))
            {
                throw new NavigateToMainMenuException();
            }
            else
            {
                Console.WriteLine("Invalid format. Use yyyy-MM-dd.");
            }
        } while (!result.HasValue);

        return result.Value;
    }
    
    #endregion

    public static bool DoubleCheckIntent(string prompt)
    {
        Console.Write(prompt + " (y/n): ");
        var keyInfo = ConsoleUtilities.WaitForKey(ConsoleKey.Y, ConsoleKey.N);
        return keyInfo.Key == ConsoleKey.Y;
    }
}