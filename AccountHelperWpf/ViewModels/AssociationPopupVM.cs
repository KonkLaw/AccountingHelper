using System.Windows.Input;
using AccountHelperWpf.Models;
using AccountHelperWpf.ViewUtils;

namespace AccountHelperWpf.ViewModels;

class AssociationPopupVM : BaseNotifyProperty
{
    private readonly OperationDescription description;
    private readonly Category category;
    private readonly IAssociationsManager associationsManager;

    public string OperationName => description.DisplayName;
    public string Category => category.Name;

    private string? comment;
    public string? Comment
    {
        get => comment;
        set => SetProperty(ref comment, value);
    }

    private bool isOpen = true;
    public bool IsOpen
    {
        get => isOpen;
        set => SetProperty(ref isOpen, value);
    }

    public ICommand Ok { get; }
    public ICommand Cancel { get; }
    public ICommand KeyUpCommand { get; }

    public AssociationPopupVM(
        OperationDescription description, Category category,
        IAssociationsManager associationsManager)
    {
        this.description = description;
        this.category = category;
        this.associationsManager = associationsManager;

        Ok = new DelegateCommand(OkHandler);
        Cancel = new DelegateCommand(CancelHandler);
        KeyUpCommand = new DelegateCommand<KeyEventArgs>(KeyUpHandler);
    }

    private void OkHandler()
    {
        IsOpen = false;

        IAssociation association = associationsManager.AddAssociation(description, category);
        association.Comment = Comment;
    }

    private void CancelHandler()
    {
        IsOpen = false;
    }

    private void KeyUpHandler(KeyEventArgs arg)
    {
        if (arg.Key == Key.Enter)
            OkHandler();
        else if (arg.Key == Key.Escape)
            CancelHandler();
    }
}