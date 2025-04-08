using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using TransactionService.DTOs;
using TransactionService.Models;
using TransactionService.Repositories;
using TransactionService.Services;

namespace TransactionService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TransactionsController : ControllerBase
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IAccountService _accountService;
    private readonly IMapper _mapper;
    private readonly ILogger<TransactionsController> _logger;

    public TransactionsController(
        ITransactionRepository transactionRepository,
        IAccountService accountService,
        IMapper mapper,
        ILogger<TransactionsController> logger)
    {
        _transactionRepository = transactionRepository;
        _accountService = accountService;
        _mapper = mapper;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TransactionDto>>> GetAllTransactions()
    {
        try
        {
            var transactions = await _transactionRepository.GetAllTransactionsAsync();
            return Ok(_mapper.Map<IEnumerable<TransactionDto>>(transactions));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all transactions");
            return StatusCode(500, "An error occurred while retrieving transactions");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TransactionDto>> GetTransactionById(Guid id)
    {
        try
        {
            var transaction = await _transactionRepository.GetTransactionByIdAsync(id);
            if (transaction == null)
            {
                return NotFound();
            }
            return Ok(_mapper.Map<TransactionDto>(transaction));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving transaction with ID: {TransactionId}", id);
            return StatusCode(500, "An error occurred while retrieving the transaction");
        }
    }

    [HttpGet("account/{accountId}")]
    public async Task<ActionResult<IEnumerable<TransactionDto>>> GetTransactionsByAccountId(Guid accountId)
    {
        try
        {
            var transactions = await _transactionRepository.GetTransactionsByAccountIdAsync(accountId);
            return Ok(_mapper.Map<IEnumerable<TransactionDto>>(transactions));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving transactions for account ID: {AccountId}", accountId);
            return StatusCode(500, "An error occurred while retrieving transactions");
        }
    }

    [HttpPost]
    public async Task<ActionResult<TransactionDto>> CreateTransaction(CreateTransactionDto createTransactionDto)
    {
        try
        {
            // Validate the request
            if (createTransactionDto.Amount <= 0)
            {
                return BadRequest("Transaction amount must be greater than zero");
            }

            // Get source account
            var sourceAccount = await _accountService.GetAccountByIdAsync(createTransactionDto.SourceAccountId);
            if (sourceAccount == null)
            {
                return BadRequest("Source account not found");
            }

            if (!sourceAccount.IsActive)
            {
                return BadRequest("Source account is inactive");
            }

            // Check currency match for transfers
            if (createTransactionDto.Type == TransactionType.Transfer && createTransactionDto.DestinationAccountId.HasValue)
            {
                var destinationAccount = await _accountService.GetAccountByIdAsync(createTransactionDto.DestinationAccountId.Value);
                if (destinationAccount == null)
                {
                    return BadRequest("Destination account not found");
                }

                if (!destinationAccount.IsActive)
                {
                    return BadRequest("Destination account is inactive");
                }

                if (sourceAccount.Currency != destinationAccount.Currency)
                {
                    return BadRequest("Currency mismatch: Cross-currency transfers are not supported");
                }
            }

            // Create transaction object
            var transaction = _mapper.Map<Transaction>(createTransactionDto);
            transaction.ReferenceNumber = GenerateReferenceNumber();
            transaction.Status = TransactionStatus.Pending;
            transaction.CreatedAt = DateTime.UtcNow;

            // Process transaction based on type
            switch (transaction.Type)
            {
                case TransactionType.Deposit:
                    if (!await ProcessDeposit(transaction, sourceAccount))
                    {
                        transaction.Status = TransactionStatus.Failed;
                        transaction.FailureReason = "Failed to update account balance";
                    }
                    break;

                case TransactionType.Withdrawal:
                    if (sourceAccount.Balance < transaction.Amount)
                    {
                        transaction.Status = TransactionStatus.Failed;
                        transaction.FailureReason = "Insufficient funds";
                    }
                    else if (!await ProcessWithdrawal(transaction, sourceAccount))
                    {
                        transaction.Status = TransactionStatus.Failed;
                        transaction.FailureReason = "Failed to update account balance";
                    }
                    break;

                case TransactionType.Transfer:
                    if (sourceAccount.Balance < transaction.Amount)
                    {
                        transaction.Status = TransactionStatus.Failed;
                        transaction.FailureReason = "Insufficient funds";
                    }
                    else if (!transaction.DestinationAccountId.HasValue)
                    {
                        transaction.Status = TransactionStatus.Failed;
                        transaction.FailureReason = "Destination account is required for transfers";
                    }
                    else if (!await ProcessTransfer(transaction, sourceAccount))
                    {
                        transaction.Status = TransactionStatus.Failed;
                        transaction.FailureReason = "Failed to complete transfer";
                    }
                    break;

                default:
                    transaction.Status = TransactionStatus.Failed;
                    transaction.FailureReason = $"Unsupported transaction type: {transaction.Type}";
                    break;
            }

            // Mark as completed if not failed
            if (transaction.Status != TransactionStatus.Failed)
            {
                transaction.Status = TransactionStatus.Completed;
            }

            // Save transaction
            var createdTransaction = await _transactionRepository.CreateTransactionAsync(transaction);
            
            return CreatedAtAction(
                nameof(GetTransactionById), 
                new { id = createdTransaction.Id }, 
                _mapper.Map<TransactionDto>(createdTransaction));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating transaction");
            return StatusCode(500, "An error occurred while processing the transaction");
        }
    }

    private async Task<bool> ProcessDeposit(Transaction transaction, AccountDto sourceAccount)
    {
        return await _accountService.UpdateAccountBalanceAsync(sourceAccount.Id, transaction.Amount);
    }

    private async Task<bool> ProcessWithdrawal(Transaction transaction, AccountDto sourceAccount)
    {
        return await _accountService.UpdateAccountBalanceAsync(sourceAccount.Id, -transaction.Amount);
    }

    private async Task<bool> ProcessTransfer(Transaction transaction, AccountDto sourceAccount)
    {
        if (transaction.DestinationAccountId == null)
        {
            return false;
        }

        // Withdraw from source account
        if (!await _accountService.UpdateAccountBalanceAsync(sourceAccount.Id, -transaction.Amount))
        {
            return false;
        }

        // Deposit to destination account
        return await _accountService.UpdateAccountBalanceAsync(transaction.DestinationAccountId.Value, transaction.Amount);
    }

    private string GenerateReferenceNumber()
    {
        return $"TXN-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N").Substring(0, 8)}";
    }
}