using AutoMapper;
using NotificationService.DTOs;
using NotificationService.Models;

namespace NotificationService.Profiles;

public class NotificationProfile : Profile
{
    public NotificationProfile()
    {
        // Map from Entity to DTO
        CreateMap<Notification, NotificationDto>();
        
        // Map from DTO to Entity
        CreateMap<CreateNotificationDto, Notification>();
    }
}