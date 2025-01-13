using System.ComponentModel;

namespace AccountHelperWpf.Models;

interface IAssociation : INotifyPropertyChanged
{
    OperationDescription Description { get; }
    Category Category { get; }
    bool IsNew { get; }
    string? Comment { get; set; }
}