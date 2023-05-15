using System.Collections.ObjectModel;
using AccountHelperWpf.ViewModels;

namespace AccountHelperWpf.Models;

record InitData(
    ObservableCollection<CategoryVM> Categories,
    ObservableCollection<AssociationVM> Associations,
    ObservableCollection<string> ExcludedOperations);