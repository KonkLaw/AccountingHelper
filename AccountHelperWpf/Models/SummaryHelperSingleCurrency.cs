using AccountHelperWpf.ViewModels;

namespace AccountHelperWpf.Models;

class SummaryHelperSingleCurrency
{
    class CategorySummaryTemp
    {
        private readonly string name;
        private decimal totalAmount;
        private readonly List<CategoryDetails.OperationInfo> tags = new();

        public CategorySummaryTemp(string name)
        {
            this.name = name;
        }

        public void Add(OperationVM operationVM, bool groupWithSameComment)
        {
            decimal amount = operationVM.Operation.Amount;
            string comment = operationVM.Comment;

            totalAmount += amount;

            if (string.IsNullOrEmpty(comment))
                return;

            if (groupWithSameComment)
            {
                int index = tags.FindIndex(tag => tag.Comment == comment);
                if (index < 0)
                    tags.Add(new CategoryDetails.OperationInfo(comment, amount));
                else
                {
                    CategoryDetails.OperationInfo oldValue = tags[index];
                    oldValue = oldValue with { Amount = oldValue.Amount + amount };
                    tags[index] = oldValue;
                }
            }
            else
                tags.Add(new CategoryDetails.OperationInfo(comment, amount));
        }

        public CategoryDetails GetDetails() => new CategoryDetails(name, totalAmount, tags);
    }

    public static void PrepareSummary(
        IEnumerable<Category> categoriesVM,
        IEnumerable<OperationVM> operationsVM,
        bool groupWithSameComment, out ICollection<CategoryDetails> details)
    {
        Dictionary<Category, CategorySummaryTemp> dictionary = categoriesVM.
            ToDictionary(c => c, c => new CategorySummaryTemp(c.Name));
        foreach (OperationVM operation in operationsVM)
        {
            dictionary[operation.Category].Add(operation, groupWithSameComment);
        }
        List<CategoryDetails> list = [..dictionary.Values.Select(c => c.GetDetails())];
        details = list;
    }

    public static bool GetIsSorted(IEnumerable<OperationVM> operationsVM)
    {
        foreach (OperationVM operation in operationsVM)
        {
            if (operation.IsAutoMappedNotApproved
                || operation.Category.IsDefault
                || operation.AssociationStatus == AssociationStatus.NotMatch)
                return false;
        }
        return true;
    }
}