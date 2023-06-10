using AccountHelperWpf.Models;
using AccountHelperWpf.ViewUtils;

namespace AccountHelperWpf.ViewModels;

class OperationVM : BaseNotifyProperty
{
    public BaseOperation Operation { get; }

    private CategoryVM? category;
    public CategoryVM? Category
    {
        get => category;
        set
        {
            SetProperty(ref category, value);
            IsApproved = true;
        }
    }

    private bool isApproved = true;
    public bool IsApproved
    {
        get => isApproved;
        set
        {
            if (SetProperty(ref isApproved, value))
                OnPropertyChanged(nameof(ApprovedComment));
        }
    }

    private string comment = string.Empty;
    public string Comment
    {
        get => comment;
        set => SetProperty(ref comment, value);
    }

    private AssociationStatus associationStatus = AssociationStatus.None;
    public AssociationStatus AssociationStatus
    {
        get => associationStatus;
        set
        {
            if (SetProperty(ref associationStatus, value))
                OnPropertyChanged(nameof(AssociationStatusComment));
        }
    }

    public string ApprovedComment => isApproved
            ? string.Empty
            : "Category was mapped automatically, not approved";

    public string AssociationStatusComment
    {
        get
        {
            string associationComment = associationStatus switch
            {
                AssociationStatus.None => string.Empty,
                AssociationStatus.NotCorrespond
                    => "Selected, category do not correspond to category in auto-mapping." +
                                                   "Use other category or add to exceptions",
                AssociationStatus.Excluded => "For description of operation auto-mapping is disabled.",
                _ => throw new ArgumentOutOfRangeException()
            };

            return associationComment;
        }
    }

    public OperationVM(BaseOperation operation) => Operation = operation;
}

enum AssociationStatus
{
    None,
    NotCorrespond,
    Excluded,
}