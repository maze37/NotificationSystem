using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NotificationSystem.Domain.Entities;
using NotificationSystem.Contracts.Constants;

namespace NotificationSystem.Infrastructure.Persistence.Configurations;

public sealed class DeliveryAttemptConfiguration : IEntityTypeConfiguration<DeliveryAttempt>
{
    public void Configure(EntityTypeBuilder<DeliveryAttempt> builder)
    {
        builder.ToTable("delivery_attempts");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(LengthConstants.EnumName)
            .IsRequired();

        builder.Property(x => x.ErrorMessage)
            .HasMaxLength(LengthConstants.ErrorMessage);

        builder.HasIndex(x => new { x.NotificationId, x.AttemptNumber })
            .IsUnique()
            .HasDatabaseName("ux_delivery_attempts_notification_attempt");
    }
}
