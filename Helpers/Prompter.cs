using System.Globalization;
using System.Text.RegularExpressions;
using ProjectHelperLibrary.Response;
using ProjectHelperLibrary.Validations;

namespace social_media_console_app.Helpers;

public static class Prompter
{
    public static int GetIntInput(string prompt, int min, int max)
    {
        var validation = new DataResponse<int>(false, 0, string.Empty);
        do
        {
            Console.WriteLine(prompt);
            
            if (int.TryParse(Console.ReadLine(), out int result))
            {
                validation = result.ProcessValidation(true, min, max);
                if (!validation.Success)
                {
                    Console.WriteLine(validation.Message);
                }
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

    public static string GetStringInput(string prompt, int min, int max, string? regexPattern = null)
    {
        do
        {
            Console.Write($"{prompt}: ");
            string? result = Console.ReadLine()?.Trim();
            
            if (string.IsNullOrEmpty(result))
            {
                Console.WriteLine("Input cannot be empty");
            }
            else if (result.Length < min)
            {
                Console.WriteLine("Input cannot contain less than " + min + " characters");
            }
            else if (result.Length > max)
            {
                Console.WriteLine("Input cannot contain more than " + max + " characters");
            }
            else if (regexPattern is not null && !Regex.IsMatch(result, regexPattern))
            {
                Console.WriteLine("Invalid format");
            }
            else
            {
                return result;
            }

            Console.WriteLine();
        } while (true);
    }

    public static string? GetOptionalStringInput(string prompt, int min, int max, string? regexPattern = null)
    {
        do
        {
            Console.Write($"{prompt} (Optional, press {ConsoleKey.Enter} to skip): ");
            string? result = Console.ReadLine()?.Trim();
        
            if (string.IsNullOrEmpty(result))
            {
                return null;
            }
            
            if (result.Length < min)
            {
                Console.WriteLine("Input cannot contain less than " + min + " characters");
            }
            else if (result.Length > max)
            {
                Console.WriteLine("Input cannot contain more than " + max + " characters");
            }
            else if (regexPattern is not null && !Regex.IsMatch(result, regexPattern))
            {
                Console.WriteLine("Invalid format");
            }
            else
            {
                return result;
            }

            Console.WriteLine();

        } while (true);
    }

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
            else
            {
                Console.WriteLine("Invalid format. Use yyyy-MM-dd.");
            }
        } while (!result.HasValue);

        return result.Value;
    }
    
}