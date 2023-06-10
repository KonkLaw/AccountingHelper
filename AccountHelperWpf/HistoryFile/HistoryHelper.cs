using AccountHelperWpf.Models;
using AccountHelperWpf.ViewModels;
using System.Collections.ObjectModel;

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
        new ObservableCollection<CategoryVM>
        {
            new() { Name = "Здоровье", Description = "Траты на здоровье" },
            new() { Name = "Подарки", Description = "Подарки" },
            new() { Name = "Пополнения", Description = "Пополнения" },
            new() { Name = "Транспорт", Description = "Транспорт" },
        },
        new ObservableCollection<AssociationVM>(),
        new ObservableCollection<string>());

    public static void Save(string path, InitData initData)
        => HistoryStorageHelper.Save(DataConverter.ConvertTo(initData), path);
}

class DataConverter
{
    public static InitData ConvertFrom(HistoryData historyData)
    {
        List<CategoryVM> categories = historyData.Categories!.Select(c => new CategoryVM
        {
            Description = c.Description!,
            Name = c.Name!
        }).ToList();
        List<AssociationVM> associations = historyData.Associations!.Select(a => new AssociationVM(
            a.OperationDescription!,
            categories.Find(c => c.Name == a.Category!)!,
            false)).ToList();
        return new InitData(
            new ObservableCollection<CategoryVM>(categories),
            new ObservableCollection<AssociationVM>(associations),
            new ObservableCollection<string>(historyData.ExcludedOperations!));
    }

    public static HistoryData ConvertTo(InitData initData) => new HistoryData
        {
            Categories = initData.Categories.Select(c => new CategoryRecord
            {
                Description = c.Description,
                Name = c.Name
            }).ToList(),
            Associations = initData.Associations.Collection.Select(a => new AssociationRecord
            {
                OperationDescription = a.OperationDescription,
                Category = a.CategoryVM.Name
            }).ToList(),
            ExcludedOperations = initData.ExcludedOperations.Collection.ToList(),
        };
}