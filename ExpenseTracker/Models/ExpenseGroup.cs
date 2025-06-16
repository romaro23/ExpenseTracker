using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpenseTracker.Models
{
    public class ExpenseGroup
    {
        public string Date { get; set; } = string.Empty;
        public ObservableCollection<ExpenseEntry> Expenses { get; set; } = new();
    }
}
