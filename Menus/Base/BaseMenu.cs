using social_media_console_app.BusinessLogic.Dtos;
using social_media_console_app.ProjectConstants;
using social_media_console_app.Helpers;
using social_media_console_app.Helpers.Inputs;

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

    protected virtual Task OnEnter(string? currentMenuMessage = null)
    {
        Console.Clear();
        string title = currentMenuMessage    ?? Title;
        Console.WriteLine($"--- {title} ---");
        
        return Task.CompletedTask;
    }

    protected virtual void OnBack()
    {
        
    }

    protected async Task<T?> PaginateAsync<T>(
        Func<int, int, Task<List<T>>> fetchPage,
        Action<T, int>                printItem,
        int                           pageSize,
        string? sectionTitle = null,
        bool shouldSelectItem = true)
    {
        int  currentPage  = 1;
        bool run   = true;
        var  cache = new Dictionary<int, List<T>>();

        while (run)
        {
            await OnEnter(sectionTitle);
            Console.WriteLine($"Page - {currentPage}");
            
            if (!cache.TryGetValue(currentPage, out var items))
            {
                items = await fetchPage(currentPage, pageSize);
                cache[currentPage] = items;
            }
            
            Printer.PrintList(items, printItem, shouldSelectItem); // do not need numbering if no need to select

            bool hasPrevious = currentPage > 1;
            bool hasNext     = pageSize == items.Count; // there is a chance that pageSize and last few items count matched and this boolean value is misleading

            PaginatedInput input = Prompter.GetPaginatedInput(shouldSelectItem ? items.Count : 0, hasPrevious, hasNext);

            switch (input.Type)
            {
                case PaginatedInput.Kind.Item:
                    return items[input.Index - 1]; 
                case PaginatedInput.Kind.Next:
                    currentPage++;
                    break;
                case PaginatedInput.Kind.Previous:
                    currentPage--;
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
        Action<T, int>                printItem,
        int                           pageSize,
        Func<T, Task>?                onSelect             = null,
        string?                       sectionTitle         = null,
        string?                       selectedSectionTitle = null
        )
    {
        T? selectedItem;
        do
        {
            bool shouldSelectItem = onSelect is not null;
            selectedItem = await PaginateAsync(fetchPage, printItem, pageSize, sectionTitle, shouldSelectItem);
            
            if (selectedItem is not null)
            {
                await OnEnter(selectedSectionTitle);
                await onSelect!(selectedItem);
                Thread.Sleep(Constants.MenuBackTrackDelayInMilliseconds);
            }
        } while (selectedItem is not null);

        Console.WriteLine("Routing back...");
        Thread.Sleep(Constants.MenuBackTrackDelayInMilliseconds);
    }
    protected async Task ConfirmAction(string prompt, Func<Task> action)
    {
        bool wantsToProceed = Prompter.DoubleCheckIntent(prompt);

        if (wantsToProceed)
        {
            await action();
        }
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
                
                Thread.Sleep(Constants.MenuBackTrackDelayInMilliseconds);
            }

        }

        return _exitOnBack;
    }
}