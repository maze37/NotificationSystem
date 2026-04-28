namespace NotificationSystem.Contracts.Constants;

/// <summary>
/// Централизованные ограничения длины строк.
/// </summary>
public static class LengthConstants
{
    public const int EnumName = 32;
    public const int Recipient = 512;
    public const int TemplateCode = 128;
    public const int CorrelationId = 128;
    public const int ErrorMessage = 2048;
}
