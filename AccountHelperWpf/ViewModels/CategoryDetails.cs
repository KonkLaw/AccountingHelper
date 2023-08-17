using System.Text;
using AccountHelperWpf.Utils;
using AccountHelperWpf.ViewUtils;

namespace AccountHelperWpf.ViewModels;

class CategoryDetails: BaseNotifyProperty
{
    private readonly List<(string, decimal)> tags;

    public string Name { get; }
    public decimal Amount { get; }

    private readonly Lazy<string> description;
    public string Description => description.Value;

    private bool isSelected;
    public bool IsSelected
    {
        get => isSelected;
        set => SetProperty(ref isSelected, value);
    }

    public CategoryDetails(string name, decimal amount, List<(string, decimal)> tags)
    {
        Name = name;
        Amount = amount;
        this.tags = tags;
        description = new Lazy<string>(CreateDescription);
    }

    public CategoryDetails(CategoryDetails d1, CategoryDetails d2)
    {
        if (d1.Name != d2.Name)
            throw new ArgumentException("Should have same names");
        Name = d1.Name;
        Amount = d1.Amount + d2.Amount;
        tags = new List<(string, decimal)>(d1.tags);
        tags.AddRange(d2.tags);
        description = new Lazy<string>(CreateDescription);
    }

    private string CreateDescription()
    {
        StringBuilder result = new();
        foreach ((string, decimal) tag in tags)
        {
            if (result.Length != 0)
                result.Append(", ");
            result.Append(tag.Item2.ToGoodString());
            result.Append(' ');
            result.Append(tag.Item1);
        }
        return result.ToString();
    }

    public CategoryDetails Convert(decimal coeff)
        => new CategoryDetails(
            Name,
            Amount * coeff,
            tags.Select(tag => (tag.Item1, tag.Item2 * coeff)).ToList());
}