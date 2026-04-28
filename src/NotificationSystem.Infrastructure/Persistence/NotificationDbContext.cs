using Microsoft.EntityFrameworkCore;
using NotificationSystem.Application.Abstractions;
using NotificationSystem.Domain.Entities;

namespace NotificationSystem.Infrastructure.Persistence;

public sealed class NotificationDbContext(DbContextOptions<NotificationDbContext> options)
    : DbContext(options), IUnitOfWork
{
    public DbSet<NotificationJob> NotificationJobs => Set<NotificationJob>();

    public DbSet<DeliveryAttempt> DeliveryAttempts => Set<DeliveryAttempt>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NotificationDbContext).Assembly);
    }
}
