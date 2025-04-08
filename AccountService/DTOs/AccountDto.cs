using AccountService.Models;

namespace AccountService.DTOs;

public class AccountDto
{
    public Guid Id { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public AccountType Type { get; set; }
    public decimal Balance { get; set; }
    public string Currency { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class CreateAccountDto
{
    public AccountType Type { get; set; }
    public string Currency { get; set; } = "USD";
    public Guid CustomerId { get; set; }
}