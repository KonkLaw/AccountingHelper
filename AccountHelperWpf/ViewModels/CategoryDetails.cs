using System.Text;
using AccountHelperWpf.Utils;
using AccountHelperWpf.ViewUtils;

namespace AccountHelperWpf.ViewModels;

class CategoryDetails: BaseNotifyProperty
{
    private readonly List<(string, decimal)> tags;

    public string Name { get; }
    public decimal Amount { get; }
    public string Description { get; }
    
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
        Description = CreateDescription();
    }

    private string CreateDescription()
    {
        StringBuilder description = new();
        foreach ((string, decimal) tag in tags)
        {
            if (description.Length != 0)
                description.Append(", ");
            description.Append(tag.Item2.ToGoodString());
            description.Append(' ');
            description.Append(tag.Item1);
        }
        return description.ToString();
    }
}