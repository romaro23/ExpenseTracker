using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExpenseTracker.Data;
using ExpenseTracker.Views;
using Microsoft.Win32;

namespace ExpenseTracker.ViewModels
{
    public class RegisterViewModel : ViewModelBase
    {
        private readonly AppDbContext _context;
        private string _username = string.Empty;
        private string _password = string.Empty;

        public string Username
        {

            get => _username;
            set { _username = value; OnPropertyChanged(); RegisterCommand.RaiseCanExecuteChanged(); }
        }

        public string Password
        {
            get => _password;
            set { _password = value; OnPropertyChanged(); RegisterCommand.RaiseCanExecuteChanged(); }
        }

        public RelayCommand<object> LoginCommand { get; }
        public RelayCommand<object> RegisterCommand { get; }

        public RegisterViewModel()
        {
            _context = new AppDbContext();
            RegisterCommand = new RelayCommand<object>(Register, CanRegister);
            LoginCommand = new RelayCommand<object>(Login);
        }

        private bool CanRegister(object? _)
        {
            return !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password);
        }
        private void Login(object? _)
        {
            var loginWindow = new LoginWindow();
            loginWindow.DataContext = new LoginViewModel();
            App.Current.MainWindow = loginWindow;
            loginWindow.Show();
            App.Current.Windows.OfType<RegisterWindow>().FirstOrDefault()?.Close();
        }
        private void Register(object? _)
        {
            if (_context.Users.Any(u => u.Username == Username))
            {
                System.Windows.MessageBox.Show("Username already exists.", "Registration Failed",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return;
            }

            var newUser = new Models.User
            {
                Username = Username,
                Password = Password
            };
            _context.Users.Add(newUser);
            _context.SaveChanges();
            System.Windows.MessageBox.Show("Registration successful!", "Success", System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Information);
            App.Current.MainWindow.Close();
            var loginWindow = new LoginWindow();
            loginWindow.DataContext = new LoginViewModel();
            App.Current.MainWindow = loginWindow;
            loginWindow.Show();
        }
    }
}
