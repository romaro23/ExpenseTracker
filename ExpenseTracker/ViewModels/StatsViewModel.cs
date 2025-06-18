using System.Collections.ObjectModel;
using System.Globalization;
using ExpenseTracker.Data;
using ScottPlot;
using ScottPlot.TickGenerators;
using ScottPlot.WPF;

namespace ExpenseTracker.ViewModels;

public class FilterOption
{
    public TimeFilter Value { get; set; }
    public string DisplayName { get; set; } = "";
}

public enum TimeFilter
{
    Year,
    Month,
    Week
}

public class StatsViewModel : ViewModelBase
{
    private readonly AppDbContext _context = new();

    private string[] _categories = Array.Empty<string>();
    private string _selectedCurrency = "UAH";
    private FilterOption _selectedFilter;
    private double[] _values = Array.Empty<double>();

    public StatsViewModel() => SelectedFilter = AvailableFilters.FirstOrDefault(f => f.Value == TimeFilter.Year);

    public WpfPlot ExpensePie { get; } = new();
    public WpfPlot ExpenseBar { get; } = new();
    public WpfPlot IncomePie { get; } = new();
    public WpfPlot IncomeBar { get; } = new();

    public string SelectedCurrency
    {
        get => _selectedCurrency;
        set
        {
            _selectedCurrency = value;
            OnPropertyChanged();
            UpdateExpensePieData();
            UpdateExpenseBarData();
            UpdateIncomePieData();
            UpdateIncomeBarData();
        }
    }

    public FilterOption SelectedFilter
    {
        get => _selectedFilter;
        set
        {
            _selectedFilter = value;
            OnPropertyChanged();
            UpdateExpensePieData();
            UpdateExpenseBarData();
            UpdateIncomePieData();
            UpdateIncomeBarData();
        }
    }

    public ObservableCollection<FilterOption> AvailableFilters { get; } = new()
    {
        new FilterOption { Value = TimeFilter.Year, DisplayName = "Рік" },
        new FilterOption { Value = TimeFilter.Month, DisplayName = "Місяць" },
        new FilterOption { Value = TimeFilter.Week, DisplayName = "Тиждень" }
    };

    public ObservableCollection<string> AvailableCurrencies { get; } =
        new() { "UAH", "USD", "EUR" };

    public double[] Values
    {
        get => _values;
        private set
        {
            _values = value;
            OnPropertyChanged();
        }
    }

    public string[] Categories
    {
        get => _categories;
        private set
        {
            _categories = value;
            OnPropertyChanged();
        }
    }

    private void UpdateBar(string[] categories, double[] values, WpfPlot plot, string title)
    {
        plot.Plot.Clear();
        var positions = Enumerable.Range(0, values.Length).Select(x => (double)x).ToArray();
        var bar = plot.Plot.Add.Bars(values);
        for (var i = 0; i < bar.Bars.Count; i++)
        {
            bar.Bars[i].FillColor = title.Contains("витрат") ? Colors.Red : Colors.SeaGreen;
            bar.Bars[i].Label = values[i].ToString("0.00");
        }

        bar.ValueLabelStyle.FontSize = 16;
        bar.ValueLabelStyle.FontName = "Verdana";
        bar.ValueLabelStyle.Bold = true;

        plot.Plot.Axes.Bottom.TickGenerator = new NumericManual(positions, categories);
        plot.Plot.Axes.Title.Label.Text = title;
        plot.Plot.Axes.Bottom.Label.Text = "Час";
        plot.Plot.Axes.Left.Label.Text = "Сума";

        plot.Plot.Axes.Margins(0.05, 0.1);
        plot.Plot.HideGrid();
        plot.Refresh();
    }


    private void UpdateExpenseBarData()
    {
        var now = DateTime.Now;
        DateTime startDate;
        string[] labels;
        double[] totals;

        switch (SelectedFilter?.Value)
        {
            case TimeFilter.Year:
                startDate = now.AddYears(-1);
                labels = Enumerable.Range(1, 12)
                    .Select(i => new DateTime(now.Year, i, 1).ToString("MMM", new CultureInfo("uk-UA")))
                    .ToArray();

                totals = Enumerable.Range(1, 12)
                    .Select(i =>
                    {
                        var monthStart = new DateTime(now.Year, i, 1);
                        var monthEnd = monthStart.AddMonths(1);
                        return _context.Expenses
                            .Where(e => e.Date >= monthStart && e.Date < monthEnd &&
                                        e.CurrencyType == SelectedCurrency && e.UserId == CurrentUser.Id)
                            .Sum(e => e.Amount);
                    }).ToArray();
                break;

            case TimeFilter.Month:
                var currentYear = now.Year;
                var currentMonth = now.Month;
                var today = now.Day;

                startDate = new DateTime(currentYear, currentMonth, 1);
                labels = Enumerable.Range(1, today)
                    .Select(d => d.ToString())
                    .ToArray();

                totals = Enumerable.Range(1, today)
                    .Select(d =>
                    {
                        var dayStart = new DateTime(currentYear, currentMonth, d);
                        var dayEnd = dayStart.AddDays(1);
                        return _context.Expenses
                            .Where(e => e.Date >= dayStart && e.Date < dayEnd && e.CurrencyType == SelectedCurrency &&
                                        e.UserId == CurrentUser.Id)
                            .Sum(e => e.Amount);
                    }).ToArray();
                break;


            case TimeFilter.Week:
                startDate = now.AddDays(-7);
                labels = Enumerable.Range(0, 7)
                    .Select(i => now.AddDays(-6 + i).ToString("ddd", new CultureInfo("uk-UA")))
                    .ToArray();

                totals = Enumerable.Range(0, 7)
                    .Select(i =>
                    {
                        var day = now.AddDays(-6 + i).Date;
                        return _context.Expenses
                            .Where(e => e.Date.Date == day && e.CurrencyType == SelectedCurrency &&
                                        e.UserId == CurrentUser.Id)
                            .Sum(e => e.Amount);
                    }).ToArray();
                break;

            default:
                return;
        }

        Categories = labels;
        Values = totals;
        var totalExpenses = _context.Expenses
            .Where(x => x.CurrencyType == SelectedCurrency && x.UserId == CurrentUser.Id).Sum(x => x.Amount);
        UpdateBar(Categories, Values, ExpenseBar, $"Сума витрат (загальна - {totalExpenses})");
    }

    private void UpdateExpensePieData()
    {
        var startDate = SelectedFilter?.Value switch
        {
            TimeFilter.Year => DateTime.Now.AddYears(-1),
            TimeFilter.Month => DateTime.Now.AddMonths(-1),
            TimeFilter.Week => DateTime.Now.AddDays(-7),
            _ => DateTime.Now.AddYears(-1)
        };

        var expenses = _context.Expenses
            .Where(e => e.Date >= startDate && e.CurrencyType == SelectedCurrency && e.UserId == CurrentUser.Id)
            .GroupBy(e => e.Category)
            .Select(g => new
            {
                Category = g.Key,
                Total = g.Sum(x => x.Amount)
            })
            .OrderByDescending(x => x.Total)
            .ToList();

        Categories = expenses.Select(x => x.Category).ToArray();
        Values = expenses.Select(x => x.Total).ToArray();
        UpdatePie(Categories, Values, ExpensePie, "Категорії витрат");
    }

    private Color GetColorByIndex(int index)
    {
        Color[] palette = { Colors.Red, Colors.Blue, Colors.Green, Colors.Orange, Colors.Purple };
        return palette[index % palette.Length];
    }

    private void UpdatePie(string[] categories, double[] values, WpfPlot plot, string title)
    {
        plot.Plot.Clear();
        if (values.Length == 0)
        {
            plot.Plot.RenderInMemory();
            return;
        }

        var slices = values
            .Select((value, index) => new PieSlice
            {
                Value = value,
                Label = categories[index] + $" ({value:0.00})",
                LabelFontSize = 16,
                LabelBold = true,
                LabelFontName = "Verdana",
                FillColor = GetColorByIndex(index)
            })
            .ToList();

        var pie = plot.Plot.Add.Pie(slices);
        pie.ExplodeFraction = 0.1;
        plot.Plot.Axes.Title.Label.Text = title;
        plot.Plot.HideGrid();
        plot.Plot.Axes.Margins(0, 0);
        plot.Plot.PlotControl.Refresh();
    }

    private void UpdateIncomePieData()
    {
        var startDate = SelectedFilter?.Value switch
        {
            TimeFilter.Year => DateTime.Now.AddYears(-1),
            TimeFilter.Month => DateTime.Now.AddMonths(-1),
            TimeFilter.Week => DateTime.Now.AddDays(-7),
            _ => DateTime.Now.AddYears(-1)
        };

        var incomes = _context.Incomes
            .Where(e => e.Date >= startDate && e.CurrencyType == SelectedCurrency && e.UserId == CurrentUser.Id)
            .GroupBy(e => e.Category)
            .Select(g => new
            {
                Category = g.Key,
                Total = g.Sum(x => x.Amount)
            })
            .OrderByDescending(x => x.Total)
            .ToList();

        string[] labels = incomes.Select(x => x.Category).ToArray();
        var values = incomes.Select(x => x.Total).ToArray();

        UpdatePie(labels, values, IncomePie, "Категорії надходжень");
    }

    private void UpdateIncomeBarData()
    {
        var now = DateTime.Now;
        DateTime startDate;
        string[] labels;
        double[] totals;

        switch (SelectedFilter?.Value)
        {
            case TimeFilter.Year:
                startDate = now.AddYears(-1);
                labels = Enumerable.Range(1, 12)
                    .Select(i => new DateTime(now.Year, i, 1).ToString("MMM", new CultureInfo("uk-UA")))
                    .ToArray();

                totals = Enumerable.Range(1, 12)
                    .Select(i =>
                    {
                        var monthStart = new DateTime(now.Year, i, 1);
                        var monthEnd = monthStart.AddMonths(1);
                        return _context.Incomes
                            .Where(e => e.Date >= monthStart && e.Date < monthEnd &&
                                        e.CurrencyType == SelectedCurrency && e.UserId == CurrentUser.Id)
                            .Sum(e => e.Amount);
                    }).ToArray();
                break;

            case TimeFilter.Month:
                var currentYear = now.Year;
                var currentMonth = now.Month;
                var today = now.Day;

                startDate = new DateTime(currentYear, currentMonth, 1);
                labels = Enumerable.Range(1, today)
                    .Select(d => d.ToString())
                    .ToArray();

                totals = Enumerable.Range(1, today)
                    .Select(d =>
                    {
                        var dayStart = new DateTime(currentYear, currentMonth, d);
                        var dayEnd = dayStart.AddDays(1);
                        return _context.Incomes
                            .Where(e => e.Date >= dayStart && e.Date < dayEnd && e.CurrencyType == SelectedCurrency &&
                                        e.UserId == CurrentUser.Id)
                            .Sum(e => e.Amount);
                    }).ToArray();
                break;

            case TimeFilter.Week:
                startDate = now.AddDays(-7);
                labels = Enumerable.Range(0, 7)
                    .Select(i => now.AddDays(-6 + i).ToString("ddd", new CultureInfo("uk-UA")))
                    .ToArray();

                totals = Enumerable.Range(0, 7)
                    .Select(i =>
                    {
                        var day = now.AddDays(-6 + i).Date;
                        return _context.Incomes
                            .Where(e => e.Date.Date == day && e.CurrencyType == SelectedCurrency &&
                                        e.UserId == CurrentUser.Id)
                            .Sum(e => e.Amount);
                    }).ToArray();
                break;

            default:
                return;
        }

        var totalIncomes = _context.Incomes.Where(x => x.CurrencyType == SelectedCurrency && x.UserId == CurrentUser.Id)
            .Sum(x => x.Amount);
        UpdateBar(labels, totals, IncomeBar, $"Сума надходжень (загальна - {totalIncomes})");
    }
}