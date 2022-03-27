using AccountingHelper.Logic;
using AccountingHelper.Pages;
using Microsoft.AspNetCore.Components;

namespace AccountingHelper.ViewModels;

class SelectionPageVM : ISelectionPageVM
{
    private readonly IServiceProvider serviceProvider;

    public IReadOnlyList<AccountFile> Files { get; }

    public SelectionPageVM(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
        Files = serviceProvider.GetService<Storage>()!.list;
    }

    public void Next()
    {
        serviceProvider.GetService<NavigationManager>()!.NavigateTo(nameof(CategoriesPage));
    }
}

interface ISelectionPageVM
{
    IReadOnlyList<AccountFile> Files { get; }
    void Next();
}