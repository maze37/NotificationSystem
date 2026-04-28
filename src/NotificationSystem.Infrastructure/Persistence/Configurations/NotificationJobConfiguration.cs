using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NotificationSystem.Domain.Entities;
using NotificationSystem.Contracts.Constants;

namespace NotificationSystem.Infrastructure.Persistence.Configurations;

public sealed class NotificationJobConfiguration : IEntityTypeConfiguration<NotificationJob>
{
    public void Configure(EntityTypeBuilder<NotificationJob> builder)
    {
        builder.ToTable("notification_jobs");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Channel)
            .HasConversion<string>()
            .HasMaxLength(LengthConstants.EnumName)
            .IsRequired();

        builder.Property(x => x.Recipient)
            .HasMaxLength(LengthConstants.Recipient)
            .IsRequired();

        builder.Property(x => x.TemplateCode)
            .HasMaxLength(LengthConstants.TemplateCode)
            .IsRequired();

        builder.Property(x => x.PayloadJson)
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(LengthConstants.EnumName)
            .IsRequired();

        builder.Property(x => x.ErrorMessage)
            .HasMaxLength(LengthConstants.ErrorMessage);

        builder.Property(x => x.CorrelationId)
            .HasMaxLength(LengthConstants.CorrelationId)
            .IsRequired();

        builder.HasIndex(x => new { x.Status, x.CreatedWhen })
            .HasDatabaseName("ix_notification_jobs_status_created_at");

        builder.HasIndex(x => x.CorrelationId)
            .IsUnique()
            .HasDatabaseName("ux_notification_jobs_correlation_id");

        builder.HasMany(x => x.DeliveryAttempts)
            .WithOne(x => x.Notification)
            .HasForeignKey(x => x.NotificationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
