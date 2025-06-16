using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ExpenseTracker.Data;

namespace ExpenseTracker.Views
{
    /// <summary>
    /// Interaction logic for ExpensePage.xaml
    /// </summary>
    public partial class ExpensePage : Page
    {
        private readonly BalanceService _balanceService;
        public ExpensePage(BalanceService balanceService)
        {
            InitializeComponent();
            _balanceService = balanceService;
            DataContext = new ExpenseTracker.ViewModels.ExpenseViewModel(_balanceService);
        }
    }
}
