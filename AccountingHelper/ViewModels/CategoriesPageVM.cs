using AccountingHelper.Logic;

namespace AccountingHelper.ViewModels;

class CategoriesPageVM : ICategoriesPageVM
{
    public CategoryModel? SelectedCategory { get; set; }
    public IReadOnlyList<CategoryModel> Categories { get; }

    public CategoriesPageVM(Storage storage)
    {
        Categories = storage.History.Categories;
    }
}

interface ICategoriesPageVM
{
    CategoryModel? SelectedCategory { get; set; }
    IReadOnlyList<CategoryModel> Categories { get; }
}