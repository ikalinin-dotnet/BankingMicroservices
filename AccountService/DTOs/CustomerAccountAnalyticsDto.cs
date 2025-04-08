namespace AccountService.DTOs;

public class CustomerAccountAnalyticsDto
{
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public int TotalAccounts { get; set; }
    public decimal TotalBalance { get; set; }
    public ICollection<AccountTypeAnalytics> AccountsByType { get; set; } = new List<AccountTypeAnalytics>();
    public ICollection<CurrencyAnalytics> AccountsByCurrency { get; set; } = new List<CurrencyAnalytics>();
}