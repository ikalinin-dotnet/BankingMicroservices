using TransactionService.Models;

namespace TransactionService.Repositories;

public interface ITransactionRepository
{
    Task<IEnumerable<Transaction>> GetAllTransactionsAsync();
    Task<Transaction?> GetTransactionByIdAsync(Guid id);
    Task<Transaction?> GetTransactionByReferenceNumberAsync(string referenceNumber);
    Task<IEnumerable<Transaction>> GetTransactionsByAccountIdAsync(Guid accountId);
    Task<Transaction> CreateTransactionAsync(Transaction transaction);
    Task UpdateTransactionAsync(Transaction transaction);
}