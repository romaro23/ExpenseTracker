using System.Collections.ObjectModel;

namespace ExpenseTracker.Models;

public class IncomeGroup
{
    public string Date { get; set; } = string.Empty;
    public ObservableCollection<IncomeEntry> Incomes { get; set; } = new();
}