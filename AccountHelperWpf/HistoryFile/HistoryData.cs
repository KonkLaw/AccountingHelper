namespace AccountHelperWpf.HistoryFile;

class HistoryData
{
    public List<AssociationRecord>? Associations { get; set; }
    public List<CategoryRecord>? Categories { get; set; }
    public List<string>? ExcludedOperations { get; set; }
}

class AssociationRecord
{
    public string? OperationDescription { get; set; }
    public string? Category { get; set; }
}

class CategoryRecord
{
    public string? Name { get; set; }
    public string? Description { get; set; }
}