using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DocumentFormat.OpenXml.VariantTypes;
using ExpenseTracker.Data;
using ExpenseTracker.Models;
using ExpenseTracker.Views;

namespace ExpenseTracker.ViewModels
{

    internal class ExpenseViewModel : ViewModelBase
    {

        private readonly AppDbContext _context;
        private readonly BalanceService _balanceService;

        public ObservableCollection<ExpenseGroup> GroupedExpenses { get; } = new();
        public ObservableCollection<string> AvailableCurrencies { get; } =
            new() { "UAH", "USD", "EUR" };
        public ObservableCollection<string> AvailableCategories { get; } =
            new() { "Їжа", "Транспорт", "Розваги", "Здоров'я", "Авто", "Відпочинок", "Платежі", "Подарунки", "Освіта", "Проїзд", "Одяг", "Продукти", "Інше", "+ Додати категорію" };
        public ObservableCollection<string> AvailableFilterCategories { get; } =
            new() { "Їжа", "Транспорт", "Розваги", "Здоров'я", "Авто", "Відпочинок", "Платежі", "Подарунки", "Освіта", "Проїзд", "Одяг", "Продукти", "Інше" };
        private string _currency = string.Empty;
        public string Currency
        {
            get => _currency;
            set { _currency = value; OnPropertyChanged(); AddExpenseCommand.RaiseCanExecuteChanged(); }
        }
        private string _category = string.Empty;
        public string Category
        {
            get => _category;
            set
            {
                _category = value;
                OnPropertyChanged(); AddExpenseCommand.RaiseCanExecuteChanged(); 
                if (value == "+ Додати категорію")
                {
                    _category = string.Empty;
                    OnPropertyChanged();
                    OpenAddCategoryDialog();
                }
            }
        }

        private string _description = string.Empty;
        public string Description
        {
            get => _description;
            set { _description = value; OnPropertyChanged(); AddExpenseCommand.RaiseCanExecuteChanged(); }
        }

        private string _amount = string.Empty;
        public string Amount
        {
            get => _amount;
            set { _amount = value; OnPropertyChanged(); AddExpenseCommand.RaiseCanExecuteChanged(); ; }
        }
        private DateTime? _date;
        public DateTime? Date
        {
            get => _date;
            set { _date = value; OnPropertyChanged(); AddExpenseCommand.RaiseCanExecuteChanged(); }
        }
        private string _filterCategory = string.Empty;
        public string FilterCategory
        {
            get => _filterCategory;
            set
            {
                _filterCategory = value;
                OnPropertyChanged();
                LoadExpenses();
            }
        }

        private DateTime? _filterDateTo;
        public DateTime? FilterDateTo
        {
            get => _filterDateTo;
            set
            {
                _filterDateTo = value;
                OnPropertyChanged();
                LoadExpenses();
            }
        }

        private DateTime? _filterDateFrom;
        public DateTime? FilterDateFrom
        {
            get => _filterDateFrom;
            set
            {
                _filterDateFrom = value;
                OnPropertyChanged();
                LoadExpenses();
            }
        }
        private ExpenseEntry? _expenseEntryToEdit;
        public ExpenseEntry? ExpenseEntryToEdit
        {
            get => _expenseEntryToEdit;
            set
            {
                _expenseEntryToEdit = value;
                OnPropertyChanged();
            }
        }
        private string _buttonText = "Додати";
        public string ButtonText
        {
            get => _buttonText;
            set
            {
                _buttonText = value;
                OnPropertyChanged();
            }
        }
        
        public RelayCommand<object> AddExpenseCommand { get; }
        public RelayCommand<object> ClearCommand { get; }
        public RelayCommand<ExpenseEntry> DeleteExpenseCommand { get; }
        public RelayCommand<ExpenseEntry> FillExpenseCommand { get; }
        
        public ExpenseViewModel(BalanceService balanceService)
        {
            _context = new AppDbContext();
            _balanceService = balanceService;
            LoadExpenses();
            AddExpenseCommand = new RelayCommand<object>(AddExpense, CanAddExpense);
            ClearCommand = new RelayCommand<object>(_ =>
            {
                FilterCategory = string.Empty;
                FilterDateFrom = null;
                FilterDateTo = null;
                LoadExpenses();
            });
            DeleteExpenseCommand = new RelayCommand<ExpenseEntry>(DeleteExpense);
            FillExpenseCommand = new RelayCommand<ExpenseEntry>(FillExpense);
            
        }

        
        private void OpenAddCategoryDialog()
        {
            var dialog = new AddCategoryWindow();
            if (dialog.ShowDialog() == true)
            {
                var newCategory = dialog.NewCategoryName;
                if (!string.IsNullOrWhiteSpace(newCategory) && !AvailableCategories.Contains(newCategory))
                {
                    AvailableCategories.Insert(AvailableCategories.Count - 1, newCategory);
                    Category = newCategory;
                }
            }
        }
        private void FillExpense(ExpenseEntry expense)
        {
            if (expense == null)
                return;
            Amount = expense.Amount.ToString("F2", CultureInfo.InvariantCulture);
            Category = expense.Category;
            Description = expense.Description;
            Date = expense.Date;
            Currency = expense.CurrencyType;
            ExpenseEntryToEdit = expense;
            ButtonText = "Редагувати";
        }
        private void EditExpense(object? _)
        {
            if (ExpenseEntryToEdit == null)
                return;
            var expense = _context.Expenses.Find(ExpenseEntryToEdit.Id);
            if (expense == null)
                return;
            if (!double.TryParse(Amount, out double amount))
                return;
            expense.Amount = amount;
            expense.Category = Category;
            expense.Description = Description;
            expense.Date = Date.Value;
            expense.CurrencyType = Currency;
            _context.SaveChanges();
            LoadExpenses();
            _balanceService.NotifyBalanceChanged();
        }
        private void DeleteExpense(ExpenseEntry expense)
        {
            if (expense == null)
                return;

            _context.Expenses.Remove(expense);
            _context.SaveChanges();
            LoadExpenses();
            _balanceService.NotifyBalanceChanged();
        }
        private void ClearFields()
        {
            Amount = string.Empty;
            Category = string.Empty;
            Description = string.Empty;
            Date = null;
            Currency = string.Empty;
            ExpenseEntryToEdit = null;
            ButtonText = "Додати";
        }
        private void LoadExpenses()
        {

            GroupedExpenses.Clear();

            var expenses = _context.Expenses.Where(e => e.UserId == CurrentUser.Id).AsQueryable();

            if (!string.IsNullOrWhiteSpace(FilterCategory))
                expenses = expenses.Where(e => e.Category == FilterCategory);

            if (FilterDateFrom.HasValue)
                expenses = expenses.Where(e => e.Date.Date >= FilterDateFrom.Value.Date);

            if (FilterDateTo.HasValue)
                expenses = expenses.Where(e => e.Date.Date <= FilterDateTo.Value.Date);

            var grouped = expenses
                .OrderByDescending(e => e.Date)
                .ToList()
                .GroupBy(e => e.Date.Date)
                .Select(g => new ExpenseGroup
                {
                    Date = g.Key.ToString("dd MMMM", new CultureInfo("uk-UA")),
                    Expenses = new ObservableCollection<ExpenseEntry>(g)
                });

            foreach (var group in grouped)
                GroupedExpenses.Add(group);
        }

        private bool CanAddExpense(object? _)
        {
            return double.TryParse(Amount, out double d) && !string.IsNullOrWhiteSpace(Category) && !string.IsNullOrWhiteSpace(Currency) && Date != null;
        }
        private void AddExpense(object? _)
        {
            if (!double.TryParse(Amount, out double amount))
                return;
            if (ButtonText == "Редагувати")
            {
                EditExpense(_);
                ClearFields();
                return;
            }
            var user = _context.Users.Find(CurrentUser.Id);
            if (user == null)
                return;
            var maxUAH = user.MaxExpenseLimitUAH;
            var maxUSD = user.MaxExpenseLimitUSD;
            var maxEUR = user.MaxExpenseLimitEUR;
            var expenseUAH = _context.Expenses.Where(x => x.CurrencyType == "UAH" && x.UserId == CurrentUser.Id).Sum(x => x.Amount);
            var expenseUSD = _context.Expenses.Where(x => x.CurrencyType == "USD" && x.UserId == CurrentUser.Id).Sum(x => x.Amount);
            var expenseEUR = _context.Expenses.Where(x => x.CurrencyType == "EUR" && x.UserId == CurrentUser.Id).Sum(x => x.Amount);
            switch (Currency) 
            {
                case "UAH":
                    if (expenseUAH + amount > maxUAH && maxUAH != 0)
                    {
                        MessageBox.Show($"Сума витрати перевищує встановлений ліміт: {maxUAH} ₴", "Ліміт витрат", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    break;
                case "USD":
                    if (expenseUSD + amount > maxUSD && maxUSD != 0)
                    {
                        MessageBox.Show($"Сума витрати перевищує встановлений ліміт: {maxUSD} $", "Ліміт витрат", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    break;
                case "EUR":
                    if (expenseEUR + amount > maxEUR && maxEUR != 0)
                    {
                        MessageBox.Show($"Сума витрати перевищує встановлений ліміт: {maxEUR} €", "Ліміт витрат", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    break;
                default:
                    return;
            }
            var expense = new ExpenseEntry
            {
                Amount = amount,
                Category = Category,
                Description = Description,
                Date = Date.Value,
                CurrencyType = Currency,
                UserId = CurrentUser.Id
            };

            _context.Expenses.Add(expense);
            _context.SaveChanges();

            Amount = "";
            Category = "";
            Description = "";
            LoadExpenses();

            _balanceService.NotifyBalanceChanged();
            ClearFields();
        }
    }
}
