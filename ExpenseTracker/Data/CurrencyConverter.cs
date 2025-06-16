using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using ExpenseTracker.Models;

namespace ExpenseTracker.Data
{
    public class CurrencyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ExpenseEntry entry)
            {
                string symbol = entry.CurrencyType switch
                {
                    "USD" => "$",
                    "EUR" => "€",
                    "UAH" => "₴",
                    _ => entry.CurrencyType
                };

                return $"{entry.Amount:0.00} {symbol}";
            }
            else if (value is IncomeEntry income)
            {
                string symbol = income.CurrencyType switch
                {
                    "USD" => "$",
                    "EUR" => "€",
                    "UAH" => "₴",
                    _ => income.CurrencyType
                };

                return $"{income.Amount:0.00} {symbol}";
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }
}
