using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpenseTracker.Models
{
    public class IncomeGroup
    {
        public string Date { get; set; } = string.Empty;
        public ObservableCollection<IncomeEntry> Incomes { get; set; } = new();
    }
}
