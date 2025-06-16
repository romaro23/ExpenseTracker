using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpenseTracker.Data
{
    public class BalanceService : INotifyPropertyChanged
    {
        private readonly AppDbContext _context;

        public double BalanceUAH => CalculateBalance("UAH");
        public double BalanceUSD => CalculateBalance("USD");
        public double BalanceEUR => CalculateBalance("EUR");

        public BalanceService()
        {
            _context = new AppDbContext();
        }

        public void NotifyBalanceChanged()
        {
            OnPropertyChanged(nameof(BalanceUAH));
            OnPropertyChanged(nameof(BalanceUSD));
            OnPropertyChanged(nameof(BalanceEUR));
        }

        private double CalculateBalance(string currency)
        {
            var income = _context.Incomes.Where(x => x.CurrencyType == currency && x.UserId == CurrentUser.Id).Sum(x => x.Amount);
            var expense = _context.Expenses.Where(x => x.CurrencyType == currency && x.UserId == CurrentUser.Id).Sum(x => x.Amount);
            return income - expense;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
