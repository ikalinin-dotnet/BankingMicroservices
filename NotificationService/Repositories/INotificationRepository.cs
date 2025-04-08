using NotificationService.Models;

namespace NotificationService.Repositories;

public interface INotificationRepository
{
    Task<IEnumerable<Notification>> GetAllNotificationsAsync();
    Task<Notification?> GetNotificationByIdAsync(Guid id);
    Task<IEnumerable<Notification>> GetNotificationsByCustomerIdAsync(Guid customerId);
    Task<Notification> CreateNotificationAsync(Notification notification);
    Task UpdateNotificationAsync(Notification notification);
}