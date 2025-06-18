namespace ExpenseTracker.Models;

public class ExpenseEntry
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public DateTime Date { get; set; }
    public string Category { get; set; } = string.Empty;
    public double Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public string CurrencyType { get; set; } = string.Empty;
}