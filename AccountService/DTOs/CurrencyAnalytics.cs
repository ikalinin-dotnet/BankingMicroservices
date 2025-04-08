namespace AccountService.DTOs;

public class CurrencyAnalytics
{
    public string Currency { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal TotalBalance { get; set; }
}