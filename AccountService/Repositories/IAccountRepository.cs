using AccountService.Models;

namespace AccountService.Repositories;

public interface IAccountRepository
{
    Task<IEnumerable<Account>> GetAllAccountsAsync();
    Task<Account?> GetAccountByIdAsync(Guid id);
    Task<Account?> GetAccountByNumberAsync(string accountNumber);
    Task<IEnumerable<Account>> GetAccountsByCustomerIdAsync(Guid customerId);
    Task<Account> CreateAccountAsync(Account account);
    Task UpdateAccountAsync(Account account);
    Task DeleteAccountAsync(Guid id);
}