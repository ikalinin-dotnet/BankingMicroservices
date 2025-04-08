using AccountService.Models;

namespace AccountService.DTOs;

public class AccountTypeAnalytics
{
    public AccountType Type { get; set; }
    public int Count { get; set; }
    public decimal TotalBalance { get; set; }
}