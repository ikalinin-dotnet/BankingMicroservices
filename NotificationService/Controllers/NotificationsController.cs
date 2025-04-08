using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NotificationService.DTOs;
using NotificationService.Models;
using NotificationService.Repositories;
using NotificationService.Services;

namespace NotificationService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IEmailService _emailService;
    private readonly IMapper _mapper;
    private readonly ILogger<NotificationsController> _logger;

    public NotificationsController(
        INotificationRepository notificationRepository,
        IEmailService emailService,
        IMapper mapper,
        ILogger<NotificationsController> logger)
    {
        _notificationRepository = notificationRepository;
        _emailService = emailService;
        _mapper = mapper;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<NotificationDto>>> GetAllNotifications()
    {
        try
        {
            var notifications = await _notificationRepository.GetAllNotificationsAsync();
            return Ok(_mapper.Map<IEnumerable<NotificationDto>>(notifications));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all notifications");
            return StatusCode(500, "An error occurred while retrieving notifications");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<NotificationDto>> GetNotificationById(Guid id)
    {
        try
        {
            var notification = await _notificationRepository.GetNotificationByIdAsync(id);
            if (notification == null)
            {
                return NotFound();
            }
            return Ok(_mapper.Map<NotificationDto>(notification));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving notification with ID: {NotificationId}", id);
            return StatusCode(500, "An error occurred while retrieving the notification");
        }
    }

    [HttpGet("customer/{customerId}")]
    public async Task<ActionResult<IEnumerable<NotificationDto>>> GetNotificationsByCustomerId(Guid customerId)
    {
        try
        {
            var notifications = await _notificationRepository.GetNotificationsByCustomerIdAsync(customerId);
            return Ok(_mapper.Map<IEnumerable<NotificationDto>>(notifications));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving notifications for customer ID: {CustomerId}", customerId);
            return StatusCode(500, "An error occurred while retrieving notifications");
        }
    }

    [HttpPost]
    public async Task<ActionResult<NotificationDto>> CreateNotification(CreateNotificationDto createNotificationDto)
    {
        try
        {
            // Validate the request
            if (string.IsNullOrWhiteSpace(createNotificationDto.CustomerEmail))
            {
                return BadRequest("Customer email is required");
            }

            if (string.IsNullOrWhiteSpace(createNotificationDto.Subject))
            {
                return BadRequest("Subject is required");
            }

            if (string.IsNullOrWhiteSpace(createNotificationDto.Message))
            {
                return BadRequest("Message is required");
            }

            // Create notification object
            var notification = _mapper.Map<Notification>(createNotificationDto);
            notification.Status = NotificationStatus.Pending;
            notification.CreatedAt = DateTime.UtcNow;

            _logger.LogInformation("Creating notification for customer ID: {CustomerId}", notification.CustomerId);
            var createdNotification = await _notificationRepository.CreateNotificationAsync(notification);

            // Send the email
            bool emailSent = await _emailService.SendEmailAsync(
                notification.CustomerEmail,
                notification.Subject,
                notification.Message);

            if (emailSent)
            {
                createdNotification.Status = NotificationStatus.Sent;
                createdNotification.SentAt = DateTime.UtcNow;
            }
            else
            {
                createdNotification.Status = NotificationStatus.Failed;
            }

            await _notificationRepository.UpdateNotificationAsync(createdNotification);

            return CreatedAtAction(
                nameof(GetNotificationById),
                new { id = createdNotification.Id },
                _mapper.Map<NotificationDto>(createdNotification));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating notification");
            return StatusCode(500, "An error occurred while creating the notification");
        }
    }

    [HttpPost("resend/{id}")]
    public async Task<IActionResult> ResendNotification(Guid id)
    {
        try
        {
            var notification = await _notificationRepository.GetNotificationByIdAsync(id);
            if (notification == null)
            {
                return NotFound();
            }

            // Only failed notifications can be resent
            if (notification.Status != NotificationStatus.Failed)
            {
                return BadRequest("Only failed notifications can be resent");
            }

            // Attempt to send the email again
            bool emailSent = await _emailService.SendEmailAsync(
                notification.CustomerEmail,
                notification.Subject,
                notification.Message);

            if (emailSent)
            {
                notification.Status = NotificationStatus.Sent;
                notification.SentAt = DateTime.UtcNow;
                await _notificationRepository.UpdateNotificationAsync(notification);
                return Ok("Notification resent successfully");
            }
            else
            {
                return StatusCode(500, "Failed to resend notification");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resending notification with ID: {NotificationId}", id);
            return StatusCode(500, "An error occurred while resending the notification");
        }
    }
}