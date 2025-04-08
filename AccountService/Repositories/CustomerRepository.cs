using AccountService.Data;
using AccountService.Models;
using Microsoft.EntityFrameworkCore;

namespace AccountService.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly AccountDbContext _context;

    public CustomerRepository(AccountDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Customer>> GetAllCustomersAsync()
    {
        return await _context.Customers
            .Include(c => c.Accounts)
            .ToListAsync();
    }

    public async Task<Customer?> GetCustomerByIdAsync(Guid id)
    {
        return await _context.Customers
            .Include(c => c.Accounts)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Customer?> GetCustomerByEmailAsync(string email)
    {
        return await _context.Customers
            .Include(c => c.Accounts)
            .FirstOrDefaultAsync(c => c.Email == email);
    }

    public async Task<Customer> CreateCustomerAsync(Customer customer)
    {
        customer.CreatedAt = DateTime.UtcNow;
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();
        return customer;
    }

    public async Task UpdateCustomerAsync(Customer customer)
    {
        _context.Entry(customer).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteCustomerAsync(Guid id)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer != null)
        {
            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();
        }
    }
}