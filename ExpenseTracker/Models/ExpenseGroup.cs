using System.Collections.ObjectModel;

namespace ExpenseTracker.Models;

public class ExpenseGroup
{
    public string Date { get; set; } = string.Empty;
    public ObservableCollection<ExpenseEntry> Expenses { get; set; } = new();
}