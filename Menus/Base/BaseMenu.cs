using ProjectHelperLibrary.Utilities;
using social_media_console_app.BusinessLogic.Dtos;
using social_media_console_app.Constants;
using social_media_console_app.Helpers;

namespace social_media_console_app.Menus.Base;

public abstract class BaseMenu
{
    protected readonly SessionUser  _sessionUser;
    
    protected abstract string       Title       { get; }
    protected abstract List<string> MenuOptions { get; }

    protected virtual string BackLabel => "Back to menu";
    protected         int    ExitRoute => 0;

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

    protected async Task<T?> PaginateAsync<T>(
        Func<int, int, Task<List<T>>> fetchPage,
        Action<T, int>                printItem,
        int                           pageSize
    )
    {
        int  pageNumber  = 1;
        bool run   = true;
        var  cache = new Dictionary<int, List<T>>();

        while (run)
        {
            await OnEnter();
            Console.WriteLine($"Page - {pageNumber}");
            
            if (!cache.TryGetValue(pageNumber, out var items))
            {
                items = await fetchPage(pageNumber, pageSize);
                cache[pageNumber] = items;
            }
            
            Printer.PrintList(items, printItem);

            bool hasPrevious = pageNumber > 1;
            bool hasNext     = pageSize == items.Count; // there is a chance that pageSize and last few items count matched and this boolean value is misleading

            PaginatedInput input = Prompter.GetPaginatedInput(items.Count, hasPrevious, hasNext);

            switch (input.Type)
            {
                case PaginatedInput.Kind.Item:
                    return items[input.Index - 1]; 
                case PaginatedInput.Kind.Next:
                    pageNumber++;
                    break;
                case PaginatedInput.Kind.Previous:
                    pageNumber--;
                    break;
                case PaginatedInput.Kind.BackToMenu:
                    run = false;
                    break;
            }
        }

        return default;
    }
    protected async Task BrowseAndSelectAsync<T>(
        Func<int, int, Task<List<T>>> fetchPage,
        Action<T, int> printItem,
        int pageSize,
        Func<T, Task> onSelect)
    {
        T? selectedItem;
        do
        {
            selectedItem = await PaginateAsync(fetchPage, printItem, pageSize);
            if (selectedItem is not null)
            {
                await onSelect(selectedItem);
                ConsoleUtilities.ResetMenu();
            }
        } while (selectedItem is not null);

        Console.WriteLine("Routing back...");
        Thread.Sleep(Constraints.MenuBackTrackDelayInMilliseconds);
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