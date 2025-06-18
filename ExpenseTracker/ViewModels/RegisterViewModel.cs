using System.Windows;
using ExpenseTracker.Data;
using ExpenseTracker.Models;
using ExpenseTracker.Views;

namespace ExpenseTracker.ViewModels;

public class RegisterViewModel : ViewModelBase
{
    private readonly AppDbContext _context = new();
    private string _password = string.Empty;
    private string _username = string.Empty;

    public string Username
    {
        get => _username;
        set
        {
            _username = value;
            OnPropertyChanged();
            RegisterCommand.RaiseCanExecuteChanged();
        }
    }

    public string Password
    {
        get => _password;
        set
        {
            _password = value;
            OnPropertyChanged();
            RegisterCommand.RaiseCanExecuteChanged();
        }
    }

    public RelayCommand<object> LoginCommand => new(Login);
    public RelayCommand<object> RegisterCommand => new(Register, CanRegister);

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
            MessageBox.Show("Username already exists.", "Registration Failed",
                MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        var newUser = new User
        {
            Username = Username,
            Password = Password
        };
        _context.Users.Add(newUser);
        _context.SaveChanges();
        MessageBox.Show("Registration successful!", "Success", MessageBoxButton.OK,
            MessageBoxImage.Information);
        App.Current.MainWindow.Close();
        var loginWindow = new LoginWindow();
        loginWindow.DataContext = new LoginViewModel();
        App.Current.MainWindow = loginWindow;
        loginWindow.Show();
    }
}