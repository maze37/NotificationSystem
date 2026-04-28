namespace NotificationSystem.Application.DTOs;

public sealed record DeliveryDispatchResult(bool IsSuccess, bool IsTransientFailure, string? ErrorMessage);
