using System.Windows;
using ExpenseTracker.Data;
using ExpenseTracker.Views;

namespace ExpenseTracker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly BalanceService _balanceService = new();
        public MainWindow()
        {
            InitializeComponent();
            MainFrame.Navigate(new ExpensePage(_balanceService));
            DataContext = new ExpenseTracker.ViewModels.MainViewModel(_balanceService);
        }
        private void OnExpenseClick(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new ExpensePage(_balanceService));
        }
        private void OnIncomeClick(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new IncomePage(_balanceService));
        }
        private void OnStatsClick(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new StatsPage());
        }
        
    }
}