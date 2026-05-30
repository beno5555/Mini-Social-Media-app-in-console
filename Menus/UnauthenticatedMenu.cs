using social_media_console_app.BusinessLogic.Dtos;
using social_media_console_app.BusinessLogic.Services;
using social_media_console_app.ProjectConstants;
using social_media_console_app.Helpers;
using social_media_console_app.Menus.Base;

namespace social_media_console_app.Menus;

public class UnauthenticatedMenu : BaseMenu
{
    private readonly AuthService _authService;

    public UnauthenticatedMenu(AuthService authService, SessionUser sessionUser) : base (sessionUser)
    {
        _authService = authService;
    }

    protected override string Title     => "SESSION";
    protected override string BackLabel => "Exit";

    protected override List<string> MenuOptions { get; } =
    [
        "Register",
        "Log In"
    ];

    
    public override async Task<bool> Run()
    {
        bool run = true;

        while (run)
        {
            await OnEnter();
            
            Printer.PrintLines(MenuOptions, BackLabel);
            int choice = Prompter.GetIntInput("", 0, MenuOptions.Count);

            if (choice == ExitRoute)
            {
                Printer.PrintWarning("Exiting...");
                Thread.Sleep(Constants.MenuBackTrackDelayInMilliseconds);
                _exitOnBack = true;
                run = false;
            }
            else
            {
                await CompleteOperation(choice);

                if (_sessionUser.IsLoggedIn)
                {
                    run = false;
                }
                
                Printer.PrintInfo("Press any key to reset menu..");
                Console.ReadKey();
            }

        }

        return _exitOnBack;
    }
    protected override async Task CompleteOperation(int choice)
    {
        switch (choice)
        {
            case 1:
                await Register();
                break;
            case 2:
                await Login();
                break;
            default:
                Printer.PrintError("Invalid option");
                break;
        }
    }

    private async Task Register()
    {
        var registerDto = DtoPrompter.Register();

        Printer.PrintInfo("Registering...");
        var response    = await _authService.RegisterAsync(registerDto);

        if (response.Success)
        {
            Printer.PrintSuccess("Registration successful. You can now log in.");
        }
        else
        {
            Printer.PrintError("Registration failed. " + response.Message);
        }
    }

    private async Task Login()
    {
        var loginDto = DtoPrompter.Login();

        Printer.PrintInfo("Validating Credentials...");
        var response = await _authService.LoginAsync(loginDto);
        
        if (response.Success)
        {
            _sessionUser.UserId = response.Data!.UserId;
            _sessionUser.Username = response.Data!.Username;
            Printer.PrintSuccess("Login successful");
        }
        else
        {
            Printer.PrintError("Login Failed. " + response.Message);
        }
    }

}