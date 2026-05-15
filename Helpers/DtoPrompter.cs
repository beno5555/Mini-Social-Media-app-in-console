using social_media_console_app.BusinessLogic.Dtos.UserDtos;
using social_media_console_app.Constants;

namespace social_media_console_app.Helpers;

public static class DtoPrompter
{
    public static RegisterDto Register()
    {
        string username = Prompter.GetStringInput("Username", Constraints.UsernameMinLength, Constraints.UsernameMaxlength, Constraints.UsernameRegexPattern);
        string email = Prompter.GetStringInput("Email", Constraints.EmailMinLength, Constraints.EmailMaxLength,
            Constraints.EmailRegexPattern);

        string password =
            Prompter.GetStringInput("Password", Constraints.PasswordMinLength, Constraints.PasswordMaxLength);

        string? bio = Prompter.GetOptionalStringInput("Bio", 0, Constraints.BioMaxLength);

        DateTime dob = Prompter.GetDateInput("Date of birth: ", Constraints.MinAge, Constraints.MaxAge);

        var registerDto = new RegisterDto(username, email, password, bio, dob);

        return registerDto;
    }

    public static LoginDto Login()
    {
        string uniqueIdentifier = Prompter.GetStringInput("Username of Email", Constraints.UsernameMinLength, Constraints.UsernameMaxlength);
        string password =
            Prompter.GetStringInput("Password", Constraints.PasswordMinLength, Constraints.PasswordMaxLength);

        var loginDto = new LoginDto(uniqueIdentifier, password);
        return loginDto;
    }
}