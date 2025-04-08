using AccountService.DTOs;
using AccountService.Models;
using AccountService.Repositories;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace AccountService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountsController : ControllerBase
{
    private readonly IAccountRepository _accountRepository;
    private readonly IMapper _mapper;

    public AccountsController(IAccountRepository accountRepository, IMapper mapper)
    {
        _accountRepository = accountRepository;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AccountDto>>> GetAllAccounts()
    {
        var accounts = await _accountRepository.GetAllAccountsAsync();
        return Ok(_mapper.Map<IEnumerable<AccountDto>>(accounts));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AccountDto>> GetAccountById(Guid id)
    {
        var account = await _accountRepository.GetAccountByIdAsync(id);
        if (account == null)
        {
            return NotFound();
        }
        return Ok(_mapper.Map<AccountDto>(account));
    }

    [HttpPost]
    public async Task<ActionResult<AccountDto>> CreateAccount(CreateAccountDto createAccountDto)
    {
        var account = _mapper.Map<Account>(createAccountDto);
        
        // Generate a random account number (in a real app, this would be more sophisticated)
        account.AccountNumber = GenerateAccountNumber();
        account.CreatedAt = DateTime.UtcNow;
        
        var createdAccount = await _accountRepository.CreateAccountAsync(account);
        
        return CreatedAtAction(
            nameof(GetAccountById), 
            new { id = createdAccount.Id }, 
            _mapper.Map<AccountDto>(createdAccount));
    }

    // Helper method to generate a simple account number
    private string GenerateAccountNumber()
    {
        return $"{DateTime.UtcNow.ToString("yyyyMMdd")}{Guid.NewGuid().ToString("N").Substring(0, 8)}";
    }
}