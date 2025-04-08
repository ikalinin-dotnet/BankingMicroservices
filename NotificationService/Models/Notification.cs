namespace NotificationService.Models;

public class Notification
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerEmail { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? SentAt { get; set; }
}

public enum NotificationType
{
    AccountCreated,
    AccountClosed,
    TransactionCompleted,
    BalanceThreshold,
    SecurityAlert,
    GeneralInformation
}

public enum NotificationStatus
{
    Pending,
    Sent,
    Failed
}