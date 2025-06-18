using System.ComponentModel;

namespace ExpenseTracker.Data;

public class BalanceService : INotifyPropertyChanged
{
    private readonly AppDbContext _context = new();

    public double BalanceUAH => CalculateBalance("UAH");
    public double BalanceUSD => CalculateBalance("USD");
    public double BalanceEUR => CalculateBalance("EUR");

    public event PropertyChangedEventHandler? PropertyChanged;

    public void NotifyBalanceChanged()
    {
        OnPropertyChanged(nameof(BalanceUAH));
        OnPropertyChanged(nameof(BalanceUSD));
        OnPropertyChanged(nameof(BalanceEUR));
    }

    private double CalculateBalance(string currency)
    {
        var income = _context.Incomes.Where(x => x.CurrencyType == currency && x.UserId == CurrentUser.Id)
            .Sum(x => x.Amount);
        var expense = _context.Expenses.Where(x => x.CurrencyType == currency && x.UserId == CurrentUser.Id)
            .Sum(x => x.Amount);
        return income - expense;
    }

    protected void OnPropertyChanged(string name)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}