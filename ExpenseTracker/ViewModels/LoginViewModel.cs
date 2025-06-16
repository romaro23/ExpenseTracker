using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ExpenseTracker.Data;
using ExpenseTracker.Views;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ExpenseTracker.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {

        private readonly AppDbContext _context;
        private string _username = string.Empty;
        private string _password = string.Empty;

        public string Username
        {

            get => _username;
            set { _username = value; OnPropertyChanged(); LoginCommand.RaiseCanExecuteChanged(); }
        }

        public string Password
        {
            get => _password;
            set { _password = value; OnPropertyChanged(); LoginCommand.RaiseCanExecuteChanged(); }
        }

        public RelayCommand<object> LoginCommand { get; }
        public RelayCommand<object> RegisterCommand { get; }

        private bool CanLogin(object? _)
        {
            return !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password);
        }
        public LoginViewModel()
        {
            _context = new AppDbContext();
            LoginCommand = new RelayCommand<object>(Login, CanLogin);
            RegisterCommand = new RelayCommand<object>(Register);
        }
        private void Login(object? _)
        {
            var user = _context.Users.FirstOrDefault(u => u.Username == Username && u.Password == Password);
            if(user != null)
            {
                CurrentUser.Id = user.Id;
                var mainWindow = new MainWindow();
                mainWindow.Show();
                App.Current.MainWindow.Close();
            }
            else
            {
                System.Windows.MessageBox.Show("Invalid username or password.", "Login Failed", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }

        }
        private void Register(object? _)
        {
            var registerWindow = new RegisterWindow();
            registerWindow.DataContext = new RegisterViewModel();
            App.Current.MainWindow.Close();
            App.Current.MainWindow = registerWindow;
            registerWindow.Show();
        }
    }
}
