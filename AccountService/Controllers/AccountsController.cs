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

    // Helper method to generate a simple account number
    private string GenerateAccountNumber()
    {
        return $"{DateTime.UtcNow.ToString("yyyyMMdd")}{Guid.NewGuid().ToString("N").Substring(0, 8)}";
    }
}