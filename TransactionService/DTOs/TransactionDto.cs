using TransactionService.Models;

namespace TransactionService.DTOs;

public class TransactionDto
{
    public Guid Id { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public TransactionType Type { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public Guid SourceAccountId { get; set; }
    public string SourceAccountNumber { get; set; } = string.Empty;
    public Guid? DestinationAccountId { get; set; }
    public string? DestinationAccountNumber { get; set; }
    public TransactionStatus Status { get; set; }
    public string? FailureReason { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class CreateTransactionDto
{
    public TransactionType Type { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public Guid SourceAccountId { get; set; }
    public string SourceAccountNumber { get; set; } = string.Empty;
    public Guid? DestinationAccountId { get; set; }
    public string? DestinationAccountNumber { get; set; }
    public string Description { get; set; } = string.Empty;
}