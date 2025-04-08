namespace AccountService.DTOs;

public class TransactionDto
{
    public Guid Id { get; set; }
    public DateTime Date { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal Balance { get; set; }
}
