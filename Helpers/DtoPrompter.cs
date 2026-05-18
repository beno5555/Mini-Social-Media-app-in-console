using social_media_console_app.BusinessLogic.Dtos.CommentDtos;
using social_media_console_app.BusinessLogic.Dtos.MessageDtos;
using social_media_console_app.BusinessLogic.Dtos.PostDtos;
using social_media_console_app.BusinessLogic.Dtos.UserDtos;
using social_media_console_app.ProjectConstants;

namespace social_media_console_app.Helpers;

public static class DtoPrompter
{
    public static RegisterDto Register()
    {
        string username = Prompter.GetStringInput("Username", Constants.UsernameMinLength, Constants.UsernameMaxlength, Constants.UsernameRegexPattern);
        string email = Prompter.GetStringInput("Email", Constants.EmailMinLength, Constants.EmailMaxLength,
            Constants.EmailRegexPattern);

        string password =
            Prompter.GetStringInput("Password", Constants.PasswordMinLength, Constants.PasswordMaxLength);

        string? bio = Prompter.GetOptionalStringInput("Bio", 0, Constants.BioMaxLength);

        DateTime dob = Prompter.GetDateInput("Date of birth", Constants.MinAge, Constants.MaxAge);

        var registerDto = new RegisterDto(username, email, password, bio, dob);

        return registerDto;
    }

    public static LoginDto Login()
    {
        string uniqueIdentifier = Prompter.GetStringInput("Username of Email", Constants.UsernameMinLength, Constants.UsernameMaxlength);
        string password =
            Prompter.GetStringInput("Password", Constants.PasswordMinLength, Constants.PasswordMaxLength);

        var loginDto = new LoginDto(uniqueIdentifier, password);
        return loginDto;
    }

    public static CreateCommentDto Comment(int commenterId, int postId)
    {
        string content          = Prompter.GetStringInput("Write a comment", 1, Constants.CommentMaxLength);
        var    createCommentDto = new CreateCommentDto(content, commenterId, postId);
        
        return createCommentDto;
    }

    public static CreatePostDto Post(int creatorId)
    {
        string title   = Prompter.GetStringInput("Title",   1, Constants.PostTitleMaxLength);
        string content = Prompter.GetStringInput("Content", 1, Constants.PostContentMaxLength);

        var createPostDto = new CreatePostDto(creatorId, title, content);
        return createPostDto;
    }

    public static CreateMessageDto Message(int senderId, int receiverId)
    {
        string content          = Prompter.GetStringInput("Write a message", 1, Constants.MessageMaxLength);
        var    createMessageDto = new CreateMessageDto(senderId, receiverId, content);

        return createMessageDto;
    }
}