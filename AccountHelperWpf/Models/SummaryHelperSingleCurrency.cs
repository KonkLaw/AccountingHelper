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
        IEnumerable<CategoryVM> categoriesVM,
        IEnumerable<OperationVM> operationsVM,
        bool groupWithSameComment,
        out bool isSorted, out ICollection<CategoryDetails> details)
    {
        CategorySummaryTemp notAssigned = new("Not assigned");
        Dictionary<CategoryVM, CategorySummaryTemp> dictionary = categoriesVM.
            ToDictionary(c => c, c => new CategorySummaryTemp(c.Name));
        isSorted = true;
        foreach (OperationVM operation in operationsVM)
        {
            if (!operation.IsApproved)
                isSorted = false;
            if (operation.Category == null)
            {
                notAssigned.Add(operation, groupWithSameComment);
                isSorted = false;
            }
            else
                dictionary[operation.Category].Add(operation, groupWithSameComment);

            if (operation.AssociationStatus == AssociationStatus.NotMatch)
                isSorted = false;
        }
        List<CategoryDetails> list = new (dictionary.Values.Select(c => c.GetDetails()))
        {
            notAssigned.GetDetails()
        };
        details = list;
    }
}