using System.Text;
using AccountHelperWpf.Utils;
using AccountHelperWpf.ViewUtils;

namespace AccountHelperWpf.ViewModels;

class CategoryDetails: BaseNotifyProperty
{
    public record struct OperationInfo(string Comment, decimal Amount);

    public IReadOnlyCollection<OperationInfo> Tags { get; }
    public string Name { get; }
    public decimal Amount { get; }

    private readonly Lazy<string> description;
    public string AdditionalDescription => description.Value;

    private bool isSelected;
    public bool IsSelected
    {
        get => isSelected;
        set => SetProperty(ref isSelected, value);
    }

    public CategoryDetails(string name, decimal amount, IReadOnlyCollection<OperationInfo> tags)
    {
        Name = name;
        Amount = amount;
        Tags = tags;
        description = new Lazy<string>(CreateTagsDescription);
    }

    private string CreateTagsDescription()
    {
        if (Tags.Count == 0)
            return string.Empty;

        StringBuilder result = new();
        foreach (OperationInfo operationInfo in Tags)
        {
            if (result.Length != 0)
                result.Append(", ");
            result.Append(operationInfo.Amount.ToGoodString());
            result.Append(' ');
            result.Append(operationInfo.Comment);
        }
        return result.ToString();
    }
}