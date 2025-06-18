using System.Windows;
using ExpenseTracker.Data;
using ExpenseTracker.Views;

namespace ExpenseTracker.ViewModels;

internal class MainViewModel : ViewModelBase
{
    private readonly AppDbContext _context;
    private double? _maxExpenseLimitEUR;
    private double? _maxExpenseLimitUAH;
    private double? _maxExpenseLimitUSD;

    public MainViewModel(BalanceService balanceService)
    {
        _context = new AppDbContext();
        UpdateLimits();
        BalanceService = balanceService;
        BalanceService.PropertyChanged += (_, __) => OnPropertyChanged(nameof(BalanceUAH));
        BalanceService.PropertyChanged += (_, __) => OnPropertyChanged(nameof(BalanceUSD));
        BalanceService.PropertyChanged += (_, __) => OnPropertyChanged(nameof(BalanceEUR));
    }

    public BalanceService BalanceService { get; }

    public double BalanceUAH => BalanceService.BalanceUAH;
    public double BalanceUSD => BalanceService.BalanceUSD;
    public double BalanceEUR => BalanceService.BalanceEUR;

    public double? MaxExpenseLimitUAH
    {
        get => _maxExpenseLimitUAH;
        set
        {
            _maxExpenseLimitUAH = value;
            OnPropertyChanged(nameof(MaxExpenseLimitEUR));
        }
    }

    public double? MaxExpenseLimitUSD
    {
        get => _maxExpenseLimitUSD;
        set
        {
            _maxExpenseLimitUSD = value;
            OnPropertyChanged(nameof(MaxExpenseLimitEUR));
        }
    }

    public double? MaxExpenseLimitEUR
    {
        get => _maxExpenseLimitEUR;
        set
        {
            _maxExpenseLimitEUR = value;
            OnPropertyChanged();
        }
    }

    public RelayCommand<object> ChangeAccountCommand => new(ChangeAccount);
    public RelayCommand<object> SaveLimitCommand => new(SaveLimit);

    private void UpdateLimits()
    {
        var eurLimit = _context.Users.Find(CurrentUser.Id)?.MaxExpenseLimitEUR ?? 0;
        MaxExpenseLimitEUR = eurLimit == 0 ? null : eurLimit;
        var usdLimit = _context.Users.Find(CurrentUser.Id)?.MaxExpenseLimitUSD ?? 0;
        MaxExpenseLimitUSD = usdLimit == 0 ? null : usdLimit;
        var uahLimit = _context.Users.Find(CurrentUser.Id)?.MaxExpenseLimitUAH ?? 0;
        MaxExpenseLimitUAH = uahLimit == 0 ? null : uahLimit;
    }

    private void SaveLimit(object? _)
    {
        var dialog = new ValidationWindow();
        if (dialog.ShowDialog() == true)
        {
            var user = _context.Users.Find(CurrentUser.Id);
            if (user == null)
                return;

            user.MaxExpenseLimitUAH = MaxExpenseLimitUAH ?? 0;
            user.MaxExpenseLimitUSD = MaxExpenseLimitUSD ?? 0;
            user.MaxExpenseLimitEUR = MaxExpenseLimitEUR ?? 0;
            _context.SaveChanges();
            MessageBox.Show("Ліміти витрат збережено.", "Збереження лімітів", MessageBoxButton.OK,
                MessageBoxImage.Information);
            UpdateLimits();
        }
    }

    private void ChangeAccount(object? _)
    {
        var loginWindow = new LoginWindow();
        loginWindow.DataContext = new LoginViewModel();
        App.Current.MainWindow = loginWindow;
        loginWindow.Show();
        App.Current.Windows.OfType<MainWindow>().FirstOrDefault()?.Close();
    }
}