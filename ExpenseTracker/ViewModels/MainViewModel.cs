using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExpenseTracker.Data;
using ExpenseTracker.Views;

namespace ExpenseTracker.ViewModels
{
    internal class MainViewModel : ViewModelBase
    {
        public BalanceService BalanceService { get; }

        public double BalanceUAH => BalanceService.BalanceUAH;
        public double BalanceUSD => BalanceService.BalanceUSD;
        public double BalanceEUR => BalanceService.BalanceEUR;

        public RelayCommand<object> ChangeAccountCommand => new RelayCommand<object>(ChangeAccount);

        public MainViewModel(BalanceService balanceService)
        {
            BalanceService = balanceService;
            BalanceService.PropertyChanged += (_, __) => OnPropertyChanged(nameof(BalanceUAH));
            BalanceService.PropertyChanged += (_, __) => OnPropertyChanged(nameof(BalanceUSD));
            BalanceService.PropertyChanged += (_, __) => OnPropertyChanged(nameof(BalanceEUR));
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
}
