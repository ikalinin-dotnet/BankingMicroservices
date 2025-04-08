namespace AccountService.DTOs;

public class TransactionRequest
{
    public decimal Amount { get; set; }
    public string? Description { get; set; }
}