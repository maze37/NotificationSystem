using Microsoft.Extensions.Options;

namespace NotificationSystem.Infrastructure.Options;

public sealed class RabbitMqOptionsValidator : IValidateOptions<RabbitMqOptions>
{
    public ValidateOptionsResult Validate(string? name, RabbitMqOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.Host))
        {
            return ValidateOptionsResult.Fail("RabbitMq:Host обязателен.");
        }

        if (options.Port is <= 0 or > 65535)
        {
            return ValidateOptionsResult.Fail("RabbitMq:Port должен быть в диапазоне 1..65535.");
        }

        if (string.IsNullOrWhiteSpace(options.UserName))
        {
            return ValidateOptionsResult.Fail("RabbitMq:UserName обязателен.");
        }

        if (string.IsNullOrWhiteSpace(options.VirtualHost))
        {
            return ValidateOptionsResult.Fail("RabbitMq:VirtualHost обязателен.");
        }

        if (options.RetryDelaysSeconds.Count == 0)
        {
            return ValidateOptionsResult.Fail("RabbitMq:RetryDelaysSeconds должен содержать минимум одно значение.");
        }

        if (options.RetryDelaysSeconds.Any(static x => x <= 0))
        {
            return ValidateOptionsResult.Fail("RabbitMq:RetryDelaysSeconds должен содержать только положительные значения.");
        }

        return ValidateOptionsResult.Success;
    }
}
