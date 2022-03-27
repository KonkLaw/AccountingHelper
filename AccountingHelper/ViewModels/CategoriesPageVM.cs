namespace AccountingHelper.ViewModels;

class CategoriesPageVM : ICategoriesPageVM
{
    public List<string> Categories { get; }

    public CategoriesPageVM()
    {
        Categories = new List<string>
        {
            "Здоровье",
            "Еда/Продукты",
            "Транспорт",
        };
    }
}

interface ICategoriesPageVM
{
    List<string> Categories { get; }
}