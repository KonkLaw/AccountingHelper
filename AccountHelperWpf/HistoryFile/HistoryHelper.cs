using AccountHelperWpf.Models;
using AccountHelperWpf.ViewModels;

namespace AccountHelperWpf.HistoryFile;

class HistoryHelper
{
    public static bool Load(string path, out InitData? history, out string errorMessage)
    {
        if (HistoryStorageHelper.TryLoadFromFile(path, out HistoryData? historyData, out errorMessage))
        {
            history = DataConverter.ConvertFrom(historyData!);
            return true;
        }
        history = null;
        return false;
    }

    public static InitData GetEmpty() => new (
        new List<CategoryVM>
        {
            CategoryVM.Default,
            new() { Name = "Здоровье", Description = "Траты на здоровье" },
            new() { Name = "Подарки", Description = "Подарки" },
            new() { Name = "Пополнения", Description = "Пополнения" },
            new() { Name = "Транспорт", Description = "Транспорт" },
        },
        new List<AssociationVM>());

    public static void Save(string path, InitData initData)
        => HistoryStorageHelper.Save(DataConverter.ConvertTo(initData), path);
}

class DataConverter
{
    public static InitData ConvertFrom(HistoryData historyData)
    {
        List<CategoryVM> categories = [CategoryVM.Default];
        categories.AddRange(historyData.Categories!.Select(c => new CategoryVM
        {
            Description = c.Description!,
            Name = c.Name!
        }));
        var dictionary = new Dictionary<string, CategoryVM>(categories.Select(c => new KeyValuePair<string, CategoryVM>(c.Name, c)));

        List<AssociationVM> associations = historyData.Associations!.Select(a => new AssociationVM(
            a.OperationDescription!,
            a.Category == null ? CategoryVM.Default : dictionary[a.Category!],
            false)).ToList();
        return new InitData(categories, associations);
    }

    public static HistoryData ConvertTo(InitData initData) => new HistoryData
    {
        Categories = initData.Categories.Where(c => !c.IsDefault).Select(c => new CategoryRecord
        {
            Description = c.Description,
            Name = c.Name
        }).ToList(),
        Associations = initData.AssociationStorage.Associations.Select(a => new AssociationRecord
        {
            OperationDescription = a.OperationDescription,
            Category = a.CategoryVM.IsDefault ? null : a.CategoryVM.Name
        }).ToList()
    };
}