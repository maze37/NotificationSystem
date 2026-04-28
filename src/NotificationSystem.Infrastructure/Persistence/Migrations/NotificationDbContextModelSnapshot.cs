using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using NotificationSystem.Infrastructure.Persistence;

namespace NotificationSystem.Infrastructure.Persistence.Migrations;

[DbContext(typeof(NotificationDbContext))]
partial class NotificationDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasAnnotation("ProductVersion", "10.0.0")
            .HasAnnotation("Relational:MaxIdentifierLength", 63);

        NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

        modelBuilder.Entity("NotificationSystem.Domain.Entities.DeliveryAttempt", b =>
        {
            b.Property<Guid>("Id")
                .ValueGeneratedNever()
                .HasColumnType("uuid");

            b.Property<int>("AttemptNumber")
                .HasColumnType("integer");

            b.Property<DateTimeOffset>("CreatedWhen")
                .HasColumnType("timestamp with time zone");

            b.Property<string>("ErrorMessage")
                .HasMaxLength(2048)
                .HasColumnType("character varying(2048)");

            b.Property<Guid>("NotificationId")
                .HasColumnType("uuid");

            b.Property<string>("Status")
                .IsRequired()
                .HasMaxLength(32)
                .HasColumnType("character varying(32)");

            b.HasKey("Id");

            b.HasIndex("NotificationId");

            b.HasIndex("NotificationId", "AttemptNumber")
                .IsUnique()
                .HasDatabaseName("ux_delivery_attempts_notification_attempt");

            b.ToTable("delivery_attempts", (string?)null);
        });

        modelBuilder.Entity("NotificationSystem.Domain.Entities.NotificationJob", b =>
        {
            b.Property<Guid>("Id")
                .ValueGeneratedNever()
                .HasColumnType("uuid");

            b.Property<int>("Attempts")
                .HasColumnType("integer");

            b.Property<string>("Channel")
                .IsRequired()
                .HasMaxLength(32)
                .HasColumnType("character varying(32)");

            b.Property<string>("CorrelationId")
                .IsRequired()
                .HasMaxLength(128)
                .HasColumnType("character varying(128)");

            b.Property<DateTimeOffset>("CreatedWhen")
                .HasColumnType("timestamp with time zone");

            b.Property<string>("ErrorMessage")
                .HasMaxLength(2048)
                .HasColumnType("character varying(2048)");

            b.Property<string>("PayloadJson")
                .IsRequired()
                .HasColumnType("jsonb");

            b.Property<string>("Recipient")
                .IsRequired()
                .HasMaxLength(512)
                .HasColumnType("character varying(512)");

            b.Property<string>("Status")
                .IsRequired()
                .HasMaxLength(32)
                .HasColumnType("character varying(32)");

            b.Property<string>("TemplateCode")
                .IsRequired()
                .HasMaxLength(128)
                .HasColumnType("character varying(128)");

            b.Property<DateTimeOffset>("UpdatedWhen")
                .HasColumnType("timestamp with time zone");

            b.HasKey("Id");

            b.HasIndex("CorrelationId")
                .IsUnique()
                .HasDatabaseName("ux_notification_jobs_correlation_id");

            b.HasIndex("Status", "CreatedWhen")
                .HasDatabaseName("ix_notification_jobs_status_created_at");

            b.ToTable("notification_jobs", (string?)null);
        });

        modelBuilder.Entity("NotificationSystem.Domain.Entities.DeliveryAttempt", b =>
        {
            b.HasOne("NotificationSystem.Domain.Entities.NotificationJob", "Notification")
                .WithMany("DeliveryAttempts")
                .HasForeignKey("NotificationId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            b.Navigation("Notification");
        });

        modelBuilder.Entity("NotificationSystem.Domain.Entities.NotificationJob", b =>
        {
            b.Navigation("DeliveryAttempts");
        });
    }
}
