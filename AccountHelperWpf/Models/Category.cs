using AccountHelperWpf.ViewUtils;

namespace AccountHelperWpf.Models;

class Category : BaseNotifyProperty, IComparable<Category>, IComparable
{
    public static Category Default { get; } = new()
    {
        Name = "# Not assigned",
        Description = "Default category for all not assigned operations"
    };

    private string name = string.Empty;
    public string Name
    {
        get => name;
        set => SetProperty(ref name, value);
    }

    private string description = string.Empty;
    public string Description
    {
        get => description;
        set => SetProperty(ref description, value);
    }

    public bool IsDefault => ReferenceEquals(this, Default);

    public int CompareTo(Category? other)
        => other == null ? 1 : string.Compare(Name, other.Name, StringComparison.Ordinal);

    public int CompareTo(object? obj) => CompareTo(obj as Category);

    public override string ToString() => Name;
}