using AccountHelperWpf.Common;

namespace AccountHelperWpf.ViewModels;

class MainWindowModel : BaseNotifyProperty
{
    private object viewContent;
    public object ViewContent
    {
        get => viewContent;
        set => SetProperty(ref viewContent, value);
    }

    public MainWindowModel(IViewResolver viewResolver) =>
        viewContent = viewResolver.ResolveView(new FilesSortingViewModel(viewResolver, new List<CategoryVm>
        {
            new () { Name = "Подарки", Description = "Подарки"},
            new () { Name = "Здоровье", Description = "Траты на здоровье"}
        }));
}