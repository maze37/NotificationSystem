using NotificationSystem.Domain.Enums;

namespace NotificationSystem.Application.DTOs;

public sealed record DeliveryDispatchRequest(
    NotificationChannel Channel,
    string Recipient,
    string Subject,
    string Body,
    string CorrelationId);
