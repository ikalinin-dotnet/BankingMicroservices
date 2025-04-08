namespace AccountService.Models;

public class Account
{
    public Guid Id { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public AccountType Type { get; set; }
    public decimal Balance { get; set; }
    public string Currency { get; set; } = "USD";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public Guid CustomerId { get; set; }
    public Customer? Customer { get; set; }
}

public enum AccountType
{
    Checking,
    Savings,
    Investment
}