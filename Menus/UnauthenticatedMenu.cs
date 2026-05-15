using social_media_console_app.BusinessLogic.Dtos;
using social_media_console_app.BusinessLogic.Services;
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

    protected override string Title     => "Social Media Console App";
    protected override string BackLabel => "Exit";

    protected override List<string> MenuOptions { get; } =
    [
        "Register",
        "Log In"
    ];

    
    public override async Task<bool> Run()
    {
        await OnEnter();
        bool run     = true;

        while (run)
        {
            Console.Clear();
            
            Printer.PrintLines(MenuOptions, BackLabel);
            int choice = Prompter.GetIntInput("", 0, MenuOptions.Count);

            if (choice == ExitRoute)
            {
                Console.WriteLine("Exiting..");
                Thread.Sleep(1300);
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
                
                Console.WriteLine("Press any key to reset menu..");
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
                Console.WriteLine("Invalid option");
                break;
        }
    }

    private async Task Register()
    {
        var registerDto = DtoPrompter.Register();
        var response    = await _authService.RegisterAsync(registerDto);
        
        Console.WriteLine(response.Success ? "Registration Successful" : $"Registration failed. {response.Message}");
    }

    private async Task Login()
    {
        var loginDto = DtoPrompter.Login();
        var response = await _authService.LoginAsync(loginDto);
        
        if (response.Success)
        {
            _sessionUser.UserId = response.Data!.UserId;
            _sessionUser.Username = response.Data!.Username;
            Console.WriteLine("Login Successful");
        }
        else
        {
            Console.WriteLine("Login Failed. " + response.Message);
        }
    }

}