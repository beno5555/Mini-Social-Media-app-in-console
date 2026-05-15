using social_media_console_app.BusinessLogic.Dtos;
using social_media_console_app.Helpers;

namespace social_media_console_app.Menus.Base;

public abstract class BaseMenu
{
    protected readonly SessionUser  _sessionUser;
    
    protected abstract string       Title       { get; }
    protected abstract List<string> MenuOptions { get; }
    protected virtual  string       BackLabel   => "Back";
    protected virtual  int          ExitRoute   => 0;

    /// <summary>
    /// determines if the parent menu should also be exited.
    /// </summary>
    protected bool _exitOnBack = false;

    public BaseMenu(SessionUser sessionUser)
    {
        _sessionUser = sessionUser;
    }

    protected abstract Task CompleteOperation(int choice);

    protected virtual Task OnEnter()
    {
        Console.Clear();
        Console.WriteLine(Title);
        
        return Task.CompletedTask;
    }

    protected virtual void OnBack()
    {
        
    }

    /// <returns>a boolean to indicate whether a program should exit or not. (Used for UnauthenticatedMenu)</returns>
    public virtual async Task<bool> Run()
    {
        bool run = true;

        while (run)
        {
            await OnEnter();
            
            Printer.PrintLines(MenuOptions, BackLabel);
            int choice = Prompter.GetIntInput("", 0, MenuOptions.Count);

            if (choice == ExitRoute)
            {
                OnBack();
                run = false;
            }
            else
            {
                await CompleteOperation(choice);
                
                Console.WriteLine("Press any key to reset menu..");
                Console.ReadKey();
            }

        }

        return _exitOnBack;
    }
}