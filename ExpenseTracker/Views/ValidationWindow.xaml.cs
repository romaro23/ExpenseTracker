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
using System.Windows.Shapes;
using ExpenseTracker.Data;
using ExpenseTracker.ViewModels;

namespace ExpenseTracker.Views
{
    /// <summary>
    /// Interaction logic for ValidationWindow.xaml
    /// </summary>
    public partial class ValidationWindow : Window
    {
        
        public ValidationWindow()
        {
            InitializeComponent();
            ValidateCommand = new RelayCommand<object>(Validate, CanValidate);
            CancelCommand = new RelayCommand<object>(Cancel);
            DataContext = this;
        }
        private readonly AppDbContext _context = new AppDbContext();
        public RelayCommand<object> ValidateCommand { get; }
        public RelayCommand<object> CancelCommand { get; }
        private string _password;

        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                ValidateCommand.RaiseCanExecuteChanged();
            }

        }


        private bool CanValidate(object? _)
        {
            return !string.IsNullOrWhiteSpace(Password);
        }

        private void Validate(object? _)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == CurrentUser.Id);
            if (user.Password == Password)
            {
                DialogResult = true;
                this.Close();
            }
            else
            {
                System.Windows.MessageBox.Show("Invalid password.", "Validation Failed",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                DialogResult = false;
                this.Close();
            }
        }
        private void Cancel(object? _)
        {
            DialogResult = false;
        }
    }
}
