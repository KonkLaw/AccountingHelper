namespace AccountHelperWpf.HistoryFile;

class HistoryData
{
    public List<AssociationRecord>? Associations { get; set; }
    public List<CategoryRecord>? Categories { get; set; }
}

class AssociationRecord
{
    public string? BankId { get; set; }
    public IDictionary<string, string>? TagsToContents { get; set; }
    public string? Category { get; set; }
    public string? Comment { get; set; }
}

class CategoryRecord
{
    public string? Name { get; set; }
    public string? Description { get; set; }
}