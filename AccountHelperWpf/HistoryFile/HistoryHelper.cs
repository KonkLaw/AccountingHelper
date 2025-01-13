using System.Collections.ObjectModel;
using AccountHelperWpf.Models;

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
        new List<Category>
        {
            Category.Default,
            new() { Name = "Здоровье", Description = "Траты на здоровье" },
            new() { Name = "Подарки", Description = "Подарки" },
            new() { Name = "Пополнения", Description = "Пополнения" },
            new() { Name = "Транспорт", Description = "Транспорт" },
        },
        new List<IAssociation>());

    public static void Save(string path, InitData initData)
        => HistoryStorageHelper.Save(DataConverter.ConvertTo(initData), path);
}

class DataConverter
{
    public static InitData ConvertFrom(HistoryData historyData) => AssociationsManager.PrepareInitData(historyData);

    public static HistoryData ConvertTo(InitData initData)
    {
        return new HistoryData
        {
            Categories = initData.Categories.Where(c => !c.IsDefault).Select(c => new CategoryRecord
            {
                Description = c.Description,
                Name = c.Name
            }).ToList(),
            Associations = initData.AssociationStorage.Associations.Select(association =>
            {
                association.Description.GetData(out string? bankId, out ReadOnlyDictionary<string, string> tagsContentsOut);
                return new AssociationRecord
                {
                    BankId = bankId,
                    Category = association.Category.IsDefault ? null : association.Category.Name,
                    Comment = association.Comment,
                    TagsToContents = tagsContentsOut,
                };
            }).ToList()
        };
    }
}