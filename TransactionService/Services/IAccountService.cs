using TransactionService.DTOs;

namespace TransactionService.Services;

public interface IAccountService
{
    Task<AccountDto?> GetAccountByIdAsync(Guid accountId);
    Task<AccountDto?> GetAccountByNumberAsync(string accountNumber);
    Task<bool> UpdateAccountBalanceAsync(Guid accountId, decimal amount);
}