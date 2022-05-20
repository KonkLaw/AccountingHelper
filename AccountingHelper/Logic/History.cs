namespace AccountingHelper.Logic;

class History
{
    public string Name { get; }
    public IReadOnlyList<CategoryModel> Categories { get; }

    private History(string name, IEnumerable<CategoryModel> additioanlCategories)
    {
        Name = name;
        var categories = new List<CategoryModel>(GetPredefinedcategories());
        categories.AddRange(additioanlCategories);
        Categories = categories;
    }

    public static History CreateNewHistory()
    {
        return new History("New empty history file", new CategoryModel[]
        {
            new CategoryModel("Food", "Food"),
            new CategoryModel("Entertainment","Entertainment"),
            new CategoryModel("Heals","Heals"),
            new CategoryModel("Transport", "Transport"),
            new CategoryModel("Rent","Rent"),
            new CategoryModel("UtilityBills", "Utility bills"),
        });
    }

    public static CategoryModel[] GetPredefinedcategories() => new CategoryModel[]
    {
        new CategoryModel("Income", "Income"),
        new CategoryModel("Debts", "Debts"),
    };
}
