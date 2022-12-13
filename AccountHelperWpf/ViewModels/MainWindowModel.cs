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

    public MainWindowModel(IViewResolver viewResolver, object initialViewModel)
    {
        viewContent = viewResolver.ResolveView(initialViewModel);
    }
}