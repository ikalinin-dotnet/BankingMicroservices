namespace TransactionService.Models;

public class Transaction
{
    public Guid Id { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public TransactionType Type { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public Guid SourceAccountId { get; set; }
    public string SourceAccountNumber { get; set; } = string.Empty;
    public Guid? DestinationAccountId { get; set; }
    public string? DestinationAccountNumber { get; set; }
    public TransactionStatus Status { get; set; }
    public string? FailureReason { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public enum TransactionType
{
    Deposit,
    Withdrawal,
    Transfer,
    Payment,
    Fee,
    Interest
}

public enum TransactionStatus
{
    Pending,
    Completed,
    Failed,
    Cancelled
}