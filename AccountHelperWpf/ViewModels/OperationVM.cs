using AccountHelperWpf.Models;
using AccountHelperWpf.ViewUtils;

namespace AccountHelperWpf.ViewModels;

class OperationVM : BaseNotifyProperty
{
    public BaseOperation Operation { get; }

    private CategoryVM category;
    public CategoryVM Category
    {
        get => category;
        set
        {
            if (!SetProperty(ref category, value))
                return;
            OnPropertyChanged(nameof(IsAddAssociationPossible));
            OnPropertyChanged(nameof(AssociationStatus));
            OnPropertyChanged(nameof(AssociationStatusComment));
        }
    }

    private string comment = string.Empty;
    public string Comment
    {
        get => comment;
        set => SetProperty(ref comment, value);
    }

    private bool isAutoMappedNotApproved;
    public bool IsAutoMappedNotApproved
    {
        get => isAutoMappedNotApproved;
        set => SetProperty(ref isAutoMappedNotApproved, value);
    }

    private Association? association;
    public Association? Association
    {
        get => association;
        set
        {
            if (!SetProperty(ref association, value))
                return;
            OnPropertyChanged(nameof(IsAddAssociationPossible));
            OnPropertyChanged(nameof(AssociationStatus));
            OnPropertyChanged(nameof(AssociationStatusComment));
        }
    }

    public bool IsAddAssociationPossible => !Category.IsDefault && Association == null;

    public AssociationStatus AssociationStatus
    {
        get
        {
            if (Association == null)
                return AssociationStatus.None;
            if (Association.Category.IsDefault)
                return AssociationStatus.Excluded;
            if (Association.Category == Category)
                return AssociationStatus.Associated;
            else
                return AssociationStatus.NotMatch;
        }
    }

    public string AssociationStatusComment
    {
        get
        {
            string associationComment = AssociationStatus switch
            {
                AssociationStatus.None => string.Empty,
                AssociationStatus.Associated
                    => "Selected, category corresponds to category in auto-mapping.",
                AssociationStatus.NotMatch
                    => "Selected, category do not correspond to category in auto-mapping." +
                                                   "Use other category or add to exceptions",
                AssociationStatus.Excluded => "For description of operation auto-mapping is disabled.",
                _ => throw new ArgumentOutOfRangeException()
            };

            return associationComment;
        }
    }

    private bool isHighlighted;
    public bool IsHighlighted
    {
        get => isHighlighted;
        set => SetProperty(ref isHighlighted, value);
    }

    public OperationVM(BaseOperation operation, CategoryVM category)
    {
        Operation = operation;
        this.category = category;
    }

    public override string ToString() => Operation.ToString();
}

enum AssociationStatus
{
    None,
    Associated,
    NotMatch,
    Excluded
}