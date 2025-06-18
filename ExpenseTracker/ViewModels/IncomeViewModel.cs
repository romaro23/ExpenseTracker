using System.Collections.ObjectModel;
using System.Globalization;
using ExpenseTracker.Data;
using ExpenseTracker.Models;
using ExpenseTracker.Views;

namespace ExpenseTracker.ViewModels;

internal class IncomeViewModel : ViewModelBase
{
    private readonly BalanceService _balanceService;
    private readonly AppDbContext _context = new();

    private string _amount = string.Empty;
    private string _buttonText = "Додати";
    private string _category = string.Empty;
    private string _currency = string.Empty;
    private DateTime? _date;

    private string _description = string.Empty;
    private string _filterCategory = string.Empty;

    private DateTime? _filterDateFrom;

    private DateTime? _filterDateTo;
    private IncomeEntry? _incomeEntryToEdit;

    public IncomeViewModel(BalanceService balanceService)
    {
        LoadIncomes();
        ClearCommand = new RelayCommand<object>(_ =>
        {
            FilterCategory = string.Empty;
            FilterDateFrom = null;
            FilterDateTo = null;
            LoadIncomes();
        });
        _balanceService = balanceService;
    }

    public ObservableCollection<IncomeGroup> GroupedIncomes { get; } = new();

    public ObservableCollection<string> AvailableCurrencies { get; } =
        new() { "UAH", "USD", "EUR" };

    public ObservableCollection<string> AvailableCategories { get; } =
        new() { "Зарплата", "Стипендія", "Подарунок", "Інше", "+ Додати категорію" };

    public ObservableCollection<string> AvailableFilterCategories { get; } =
        new() { "Зарплата", "Стипендія", "Подарунок", "Інше" };

    public string Currency
    {
        get => _currency;
        set
        {
            _currency = value;
            OnPropertyChanged();
            AddIncomeCommand.RaiseCanExecuteChanged();
        }
    }

    public string Category
    {
        get => _category;
        set
        {
            _category = value;
            OnPropertyChanged();
            AddIncomeCommand.RaiseCanExecuteChanged();
            if (value == "+ Додати категорію")
            {
                _category = string.Empty;
                OnPropertyChanged();
                OpenAddCategoryDialog();
            }
        }
    }

    public string Description
    {
        get => _description;
        set
        {
            _description = value;
            OnPropertyChanged();
            AddIncomeCommand.RaiseCanExecuteChanged();
        }
    }

    public string Amount
    {
        get => _amount;
        set
        {
            _amount = value;
            OnPropertyChanged();
            AddIncomeCommand.RaiseCanExecuteChanged();
            ;
        }
    }

    public DateTime? Date
    {
        get => _date;
        set
        {
            _date = value;
            OnPropertyChanged();
            AddIncomeCommand.RaiseCanExecuteChanged();
        }
    }

    public string FilterCategory
    {
        get => _filterCategory;
        set
        {
            _filterCategory = value;
            OnPropertyChanged();
            LoadIncomes();
        }
    }

    public DateTime? FilterDateTo
    {
        get => _filterDateTo;
        set
        {
            _filterDateTo = value;
            OnPropertyChanged();
            LoadIncomes();
        }
    }

    public DateTime? FilterDateFrom
    {
        get => _filterDateFrom;
        set
        {
            _filterDateFrom = value;
            OnPropertyChanged();
            LoadIncomes();
        }
    }

    public IncomeEntry? IncomeEntryToEdit
    {
        get => _incomeEntryToEdit;
        set
        {
            _incomeEntryToEdit = value;
            OnPropertyChanged();
        }
    }

    public string ButtonText
    {
        get => _buttonText;
        set
        {
            _buttonText = value;
            OnPropertyChanged();
        }
    }

    public RelayCommand<object> AddIncomeCommand => new(AddIncome, CanAddIncome);
    public RelayCommand<object> ClearCommand { get; }
    public RelayCommand<IncomeEntry> DeleteIncomeCommand => new(DeleteIncome);
    public RelayCommand<IncomeEntry> FillIncomeCommand => new(FillIncome);

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

    private void FillIncome(IncomeEntry income)
    {
        if (income == null)
            return;
        Amount = income.Amount.ToString("F2", CultureInfo.InvariantCulture);
        Category = income.Category;
        Description = income.Description;
        Date = income.Date;
        Currency = income.CurrencyType;
        IncomeEntryToEdit = income;
        ButtonText = "Редагувати";
    }

    private void EditIncome(object? _)
    {
        if (IncomeEntryToEdit == null)
            return;
        var income = _context.Incomes.Find(IncomeEntryToEdit.Id);
        if (income == null)
            return;
        if (!double.TryParse(Amount, out var amount))
            return;
        income.Amount = amount;
        income.Category = Category;
        income.Description = Description;
        income.Date = Date.Value;
        income.CurrencyType = Currency;
        _context.SaveChanges();
        LoadIncomes();
        _balanceService.NotifyBalanceChanged();
    }

    private void ClearFields()
    {
        Amount = string.Empty;
        Category = string.Empty;
        Description = string.Empty;
        Date = null;
        Currency = string.Empty;
        IncomeEntryToEdit = null;
        ButtonText = "Додати";
    }

    private void DeleteIncome(IncomeEntry income)
    {
        if (income == null)
            return;

        _context.Incomes.Remove(income);
        _context.SaveChanges();
        LoadIncomes();
        _balanceService.NotifyBalanceChanged();
    }

    private void LoadIncomes()
    {
        GroupedIncomes.Clear();

        var incomes = _context.Incomes.Where(i => i.UserId == CurrentUser.Id).AsQueryable();

        if (!string.IsNullOrWhiteSpace(FilterCategory))
            incomes = incomes.Where(i => i.Category == FilterCategory);

        if (FilterDateFrom.HasValue)
            incomes = incomes.Where(i => i.Date.Date >= FilterDateFrom.Value.Date);

        if (FilterDateTo.HasValue)
            incomes = incomes.Where(i => i.Date.Date <= FilterDateTo.Value.Date);

        var grouped = incomes
            .OrderByDescending(i => i.Date)
            .ToList()
            .GroupBy(i => i.Date.Date)
            .Select(g => new IncomeGroup
            {
                Date = g.Key.ToString("dd MMMM", new CultureInfo("uk-UA")),
                Incomes = new ObservableCollection<IncomeEntry>(g)
            });

        foreach (var group in grouped)
            GroupedIncomes.Add(group);
    }


    private bool CanAddIncome(object? _)
    {
        return double.TryParse(Amount, out var d) && !string.IsNullOrWhiteSpace(Category) &&
               !string.IsNullOrWhiteSpace(Currency) && Date != null;
    }

    private void AddIncome(object? _)
    {
        if (!double.TryParse(Amount, out var amount))
            return;
        if (ButtonText == "Редагувати")
        {
            EditIncome(_);
            ClearFields();
            return;
        }

        var income = new IncomeEntry
        {
            Amount = amount,
            Category = Category,
            Description = Description,
            Date = Date.Value,
            CurrencyType = Currency,
            UserId = CurrentUser.Id
        };

        _context.Incomes.Add(income);
        _context.SaveChanges();

        Amount = "";
        Category = "";
        Description = "";
        LoadIncomes();
        _balanceService.NotifyBalanceChanged();
        ClearFields();
    }
}