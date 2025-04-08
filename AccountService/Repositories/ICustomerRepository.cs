using AccountService.Models;

namespace AccountService.Repositories;

public interface ICustomerRepository
{
    Task<IEnumerable<Customer>> GetAllCustomersAsync();
    Task<Customer?> GetCustomerByIdAsync(Guid id);
    Task<Customer?> GetCustomerByEmailAsync(string email);
    Task<Customer> CreateCustomerAsync(Customer customer);
    Task UpdateCustomerAsync(Customer customer);
    Task DeleteCustomerAsync(Guid id);
}