using AccountService.DTOs;
using AccountService.Models;
using AccountService.Repositories;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AccountService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountsController : ControllerBase
{
    private readonly IAccountRepository _accountRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<AccountsController> _logger;

    public AccountsController(
        IAccountRepository accountRepository,
        ICustomerRepository customerRepository,
        IMapper mapper,
        ILogger<AccountsController> logger)
    {
        _accountRepository = accountRepository;
        _customerRepository = customerRepository;
        _mapper = mapper;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AccountDto>>> GetAllAccounts()
    {
        try
        {
            _logger.LogInformation("Retrieving all accounts");
            var accounts = await _accountRepository.GetAllAccountsAsync();
            return Ok(_mapper.Map<IEnumerable<AccountDto>>(accounts));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all accounts");
            return StatusCode(500, "An error occurred while retrieving accounts");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AccountDto>> GetAccountById(Guid id)
    {
        try
        {
            _logger.LogInformation("Retrieving account with ID: {AccountId}", id);
            var account = await _accountRepository.GetAccountByIdAsync(id);
            if (account == null)
            {
                _logger.LogWarning("Account with ID {AccountId} not found", id);
                return NotFound();
            }
            return Ok(_mapper.Map<AccountDto>(account));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving account with ID: {AccountId}", id);
            return StatusCode(500, "An error occurred while retrieving the account");
        }
    }

    [HttpGet("number/{accountNumber}")]
    public async Task<ActionResult<AccountDto>> GetAccountByNumber(string accountNumber)
    {
        if (string.IsNullOrWhiteSpace(accountNumber))
        {
            return BadRequest("Account number cannot be empty");
        }

        try
        {
            _logger.LogInformation("Retrieving account with number: {AccountNumber}", accountNumber);
            var account = await _accountRepository.GetAccountByNumberAsync(accountNumber);
            if (account == null)
            {
                _logger.LogWarning("Account with number {AccountNumber} not found", accountNumber);
                return NotFound();
            }
            return Ok(_mapper.Map<AccountDto>(account));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving account with number: {AccountNumber}", accountNumber);
            return StatusCode(500, "An error occurred while retrieving the account");
        }
    }

    [HttpGet("{id}/statement")]
    public async Task<ActionResult<AccountStatementDto>> GetAccountStatement(Guid id, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        try
        {
            var account = await _accountRepository.GetAccountByIdAsync(id);
            if (account == null)
            {
                _logger.LogWarning("Attempted to get statement for non-existent account with ID: {AccountId}", id);
                return NotFound();
            }

            // Set default date range if not provided
            from = from ?? DateTime.UtcNow.AddMonths(-1);
            to = to ?? DateTime.UtcNow;

            _logger.LogInformation("Generating statement for account with ID: {AccountId} from {From} to {To}",
                id, from, to);

            // In a real application, you would fetch transactions from a transaction history
            // For this demo, we'll create a mock account statement
            var statement = new AccountStatementDto
            {
                AccountId = account.Id,
                AccountNumber = account.AccountNumber,
                CustomerName = $"{account.Customer?.FirstName} {account.Customer?.LastName}",
                Currency = account.Currency,
                FromDate = from.Value,
                ToDate = to.Value,
                StartingBalance = account.Balance - 1000, // Mock data
                EndingBalance = account.Balance,
                Transactions = new List<TransactionDto>
            {
                new TransactionDto
                {
                    Id = Guid.NewGuid(),
                    Date = DateTime.UtcNow.AddDays(-15),
                    Description = "Salary deposit",
                    Amount = 1000,
                    Balance = account.Balance - 500
                },
                new TransactionDto
                {
                    Id = Guid.NewGuid(),
                    Date = DateTime.UtcNow.AddDays(-5),
                    Description = "Grocery shopping",
                    Amount = -200,
                    Balance = account.Balance - 300
                },
                new TransactionDto
                {
                    Id = Guid.NewGuid(),
                    Date = DateTime.UtcNow.AddDays(-2),
                    Description = "Monthly subscription",
                    Amount = -50,
                    Balance = account.Balance - 50
                },
                new TransactionDto
                {
                    Id = Guid.NewGuid(),
                    Date = DateTime.UtcNow.AddDays(-1),
                    Description = "Interest payment",
                    Amount = 50,
                    Balance = account.Balance
                }
            }
            };

            return Ok(statement);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating statement for account with ID: {AccountId}", id);
            return StatusCode(500, "An error occurred while generating the account statement");
        }
    }

    [HttpGet("analytics/customer/{customerId}")]
    public async Task<ActionResult<CustomerAccountAnalyticsDto>> GetCustomerAccountAnalytics(Guid customerId)
    {
        try
        {
            var customer = await _customerRepository.GetCustomerByIdAsync(customerId);
            if (customer == null)
            {
                _logger.LogWarning("Attempted to get analytics for non-existent customer with ID: {CustomerId}", customerId);
                return NotFound();
            }

            var accounts = await _accountRepository.GetAccountsByCustomerIdAsync(customerId);
            if (!accounts.Any())
            {
                _logger.LogInformation("No accounts found for customer with ID: {CustomerId}", customerId);
                return NotFound("No accounts found for this customer");
            }

            _logger.LogInformation("Generating account analytics for customer with ID: {CustomerId}", customerId);

            var analytics = new CustomerAccountAnalyticsDto
            {
                CustomerId = customerId,
                CustomerName = $"{customer.FirstName} {customer.LastName}",
                TotalAccounts = accounts.Count(),
                TotalBalance = accounts.Sum(a => a.Balance),
                AccountsByType = accounts.GroupBy(a => a.Type)
                    .Select(g => new AccountTypeAnalytics
                    {
                        Type = g.Key,
                        Count = g.Count(),
                        TotalBalance = g.Sum(a => a.Balance)
                    }).ToList(),
                AccountsByCurrency = accounts.GroupBy(a => a.Currency)
                    .Select(g => new CurrencyAnalytics
                    {
                        Currency = g.Key,
                        Count = g.Count(),
                        TotalBalance = g.Sum(a => a.Balance)
                    }).ToList()
            };

            return Ok(analytics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating analytics for customer with ID: {CustomerId}", customerId);
            return StatusCode(500, "An error occurred while generating customer account analytics");
        }
    }

    [HttpPost]
    public async Task<ActionResult<AccountDto>> CreateAccount(CreateAccountDto createAccountDto)
    {
        try
        {
            // Validate customer exists
            var customer = await _customerRepository.GetCustomerByIdAsync(createAccountDto.CustomerId);
            if (customer == null)
            {
                _logger.LogWarning("Attempted to create account for non-existent customer ID: {CustomerId}",
                    createAccountDto.CustomerId);
                return BadRequest($"Customer with ID {createAccountDto.CustomerId} does not exist");
            }

            // Validate currency
            var supportedCurrencies = new[] { "USD", "EUR", "GBP" };
            if (!Array.Exists(supportedCurrencies, c => c == createAccountDto.Currency))
            {
                _logger.LogWarning("Attempted to create account with unsupported currency: {Currency}",
                    createAccountDto.Currency);
                return BadRequest($"Currency {createAccountDto.Currency} is not supported");
            }

            var account = _mapper.Map<Account>(createAccountDto);

            // Generate a random account number (in a real app, this would be more sophisticated)
            account.AccountNumber = GenerateAccountNumber();
            account.CreatedAt = DateTime.UtcNow;
            account.Balance = 0; // New accounts start with zero balance

            _logger.LogInformation("Creating new account for customer ID: {CustomerId}",
                createAccountDto.CustomerId);
            var createdAccount = await _accountRepository.CreateAccountAsync(account);

            return CreatedAtAction(
                nameof(GetAccountById),
                new { id = createdAccount.Id },
                _mapper.Map<AccountDto>(createdAccount));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating account for customer ID: {CustomerId}",
                createAccountDto.CustomerId);
            return StatusCode(500, "An error occurred while creating the account");
        }
    }

    [HttpPut("{id}/deactivate")]
    public async Task<IActionResult> DeactivateAccount(Guid id)
    {
        try
        {
            var account = await _accountRepository.GetAccountByIdAsync(id);
            if (account == null)
            {
                _logger.LogWarning("Attempted to deactivate non-existent account with ID: {AccountId}", id);
                return NotFound();
            }

            if (!account.IsActive)
            {
                return BadRequest("Account is already deactivated");
            }

            account.IsActive = false;

            _logger.LogInformation("Deactivating account with ID: {AccountId}", id);
            await _accountRepository.UpdateAccountAsync(account);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating account with ID: {AccountId}", id);
            return StatusCode(500, "An error occurred while deactivating the account");
        }
    }

    [HttpPut("{id}/activate")]
    public async Task<IActionResult> ActivateAccount(Guid id)
    {
        try
        {
            var account = await _accountRepository.GetAccountByIdAsync(id);
            if (account == null)
            {
                _logger.LogWarning("Attempted to activate non-existent account with ID: {AccountId}", id);
                return NotFound();
            }

            if (account.IsActive)
            {
                return BadRequest("Account is already active");
            }

            account.IsActive = true;

            _logger.LogInformation("Activating account with ID: {AccountId}", id);
            await _accountRepository.UpdateAccountAsync(account);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating account with ID: {AccountId}", id);
            return StatusCode(500, "An error occurred while activating the account");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAccount(Guid id)
    {
        try
        {
            var account = await _accountRepository.GetAccountByIdAsync(id);
            if (account == null)
            {
                _logger.LogWarning("Attempted to delete non-existent account with ID: {AccountId}", id);
                return NotFound();
            }

            // Only allow deletion if balance is zero
            if (account.Balance != 0)
            {
                _logger.LogWarning("Attempted to delete account with ID: {AccountId} that has non-zero balance", id);
                return BadRequest("Cannot delete account with non-zero balance");
            }

            _logger.LogInformation("Deleting account with ID: {AccountId}", id);
            await _accountRepository.DeleteAccountAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting account with ID: {AccountId}", id);
            return StatusCode(500, "An error occurred while deleting the account");
        }
    }

    [HttpPut("{id}/deposit")]
    public async Task<ActionResult<AccountDto>> DepositFunds(Guid id, [FromBody] TransactionRequest request)
    {
        if (request.Amount <= 0)
        {
            return BadRequest("Deposit amount must be greater than zero");
        }

        try
        {
            var account = await _accountRepository.GetAccountByIdAsync(id);
            if (account == null)
            {
                _logger.LogWarning("Attempted to deposit to non-existent account with ID: {AccountId}", id);
                return NotFound();
            }

            if (!account.IsActive)
            {
                _logger.LogWarning("Attempted to deposit to inactive account with ID: {AccountId}", id);
                return BadRequest("Cannot deposit funds to an inactive account");
            }

            // Perform the deposit
            account.Balance += request.Amount;

            _logger.LogInformation("Depositing {Amount} {Currency} to account with ID: {AccountId}",
                request.Amount, account.Currency, id);
            await _accountRepository.UpdateAccountAsync(account);

            // In a real system, I'd also record this transaction in a transaction history

            return Ok(_mapper.Map<AccountDto>(account));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error depositing to account with ID: {AccountId}", id);
            return StatusCode(500, "An error occurred while processing the deposit");
        }
    }

    [HttpPut("{id}/withdraw")]
    public async Task<ActionResult<AccountDto>> WithdrawFunds(Guid id, [FromBody] TransactionRequest request)
    {
        if (request.Amount <= 0)
        {
            return BadRequest("Withdrawal amount must be greater than zero");
        }

        try
        {
            var account = await _accountRepository.GetAccountByIdAsync(id);
            if (account == null)
            {
                _logger.LogWarning("Attempted to withdraw from non-existent account with ID: {AccountId}", id);
                return NotFound();
            }

            if (!account.IsActive)
            {
                _logger.LogWarning("Attempted to withdraw from inactive account with ID: {AccountId}", id);
                return BadRequest("Cannot withdraw funds from an inactive account");
            }

            // Check sufficient funds
            if (account.Balance < request.Amount)
            {
                _logger.LogWarning("Attempted to withdraw {Amount} from account with ID: {AccountId} with insufficient balance",
                    request.Amount, id);
                return BadRequest("Insufficient funds for this withdrawal");
            }

            // Perform the withdrawal
            account.Balance -= request.Amount;

            _logger.LogInformation("Withdrawing {Amount} {Currency} from account with ID: {AccountId}",
                request.Amount, account.Currency, id);
            await _accountRepository.UpdateAccountAsync(account);

            // In a real system, I'd also record this transaction in a transaction history

            return Ok(_mapper.Map<AccountDto>(account));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error withdrawing from account with ID: {AccountId}", id);
            return StatusCode(500, "An error occurred while processing the withdrawal");
        }
    }

    // Helper method to generate a simple account number
    private string GenerateAccountNumber()
    {
        return $"{DateTime.UtcNow.ToString("yyyyMMdd")}{Guid.NewGuid().ToString("N").Substring(0, 8)}";
    }
}