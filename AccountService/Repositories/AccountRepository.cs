using AccountService.Data;
using AccountService.Models;
using Microsoft.EntityFrameworkCore;

namespace AccountService.Repositories;

public class AccountRepository : IAccountRepository
{
    private readonly AccountDbContext _context;

    public AccountRepository(AccountDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Account>> GetAllAccountsAsync()
    {
        return await _context.Accounts.ToListAsync();
    }

    public async Task<Account?> GetAccountByIdAsync(Guid id)
    {
        return await _context.Accounts.FindAsync(id);
    }

    public async Task<Account?> GetAccountByNumberAsync(string accountNumber)
    {
        return await _context.Accounts
            .FirstOrDefaultAsync(a => a.AccountNumber == accountNumber);
    }

    public async Task<IEnumerable<Account>> GetAccountsByCustomerIdAsync(Guid customerId)
    {
        return await _context.Accounts
            .Where(a => a.CustomerId == customerId)
            .ToListAsync();
    }

    public async Task<Account> CreateAccountAsync(Account account)
    {
        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();
        return account;
    }

    public async Task UpdateAccountAsync(Account account)
    {
        _context.Entry(account).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAccountAsync(Guid id)
    {
        var account = await _context.Accounts.FindAsync(id);
        if (account != null)
        {
            _context.Accounts.Remove(account);
            await _context.SaveChangesAsync();
        }
    }
}