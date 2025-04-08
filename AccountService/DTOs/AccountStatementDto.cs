namespace AccountService.DTOs;

public class AccountStatementDto
{
    public Guid AccountId { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public decimal StartingBalance { get; set; }
    public decimal EndingBalance { get; set; }
    public ICollection<TransactionDto> Transactions { get; set; } = new List<TransactionDto>();
}