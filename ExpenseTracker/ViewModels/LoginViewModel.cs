using System.Windows;
using ExpenseTracker.Data;
using ExpenseTracker.Views;

namespace ExpenseTracker.ViewModels;

public class LoginViewModel : ViewModelBase
{
    private readonly AppDbContext _context = new();
    private string _password = "12345";
    private string _username = "romaro";


    public string Username
    {
        get => _username;
        set
        {
            _username = value;
            OnPropertyChanged();
            LoginCommand.RaiseCanExecuteChanged();
        }
    }

    public string Password
    {
        get => _password;
        set
        {
            _password = value;
            OnPropertyChanged();
            LoginCommand.RaiseCanExecuteChanged();
        }
    }

    public RelayCommand<object> LoginCommand => new(Login, CanLogin);
    public RelayCommand<object> RegisterCommand => new(Register);

    private bool CanLogin(object? _)
    {
        return !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password);
    }

    private void Login(object? _)
    {
        var user = _context.Users.FirstOrDefault(u => u.Username == Username && u.Password == Password);
        if (user != null)
        {
            CurrentUser.Id = user.Id;
            var mainWindow = new MainWindow();
            mainWindow.Show();
            App.Current.MainWindow.Close();
        }
        else
        {
            MessageBox.Show("Invalid username or password.", "Login Failed", MessageBoxButton.OK,
                MessageBoxImage.Error);
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