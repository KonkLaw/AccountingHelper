using AccountingHelper.Logic;

namespace AccountingHelper.ViewModels;

class SortingPageVM : ISortingPageVM
{
    public IReadOnlyList<AccountFile> Files { get; }

    public SortingPageVM(Storage storage)
    {
        Files = storage.list;
    }

    public void Test()
    {
        ;
    }
}

interface ISortingPageVM
{
    IReadOnlyList<AccountFile> Files { get; }
    void Test();
}