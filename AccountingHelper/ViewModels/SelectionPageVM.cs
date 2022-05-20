using AccountingHelper.Logic;

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
}

interface ISelectionPageVM
{
    IReadOnlyList<AccountFile> Files { get; }
}