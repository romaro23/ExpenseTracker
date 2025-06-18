namespace ExpenseTracker.Models;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public double MaxExpenseLimitUAH { get; set; } = 0.0;
    public double MaxExpenseLimitUSD { get; set; } = 0.0;
    public double MaxExpenseLimitEUR { get; set; } = 0.0;
}