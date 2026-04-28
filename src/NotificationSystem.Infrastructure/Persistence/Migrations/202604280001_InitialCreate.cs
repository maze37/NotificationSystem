using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace NotificationSystem.Infrastructure.Persistence.Migrations;

public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "notification_jobs",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Channel = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                Recipient = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                TemplateCode = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                PayloadJson = table.Column<string>(type: "jsonb", nullable: false),
                Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                Attempts = table.Column<int>(type: "integer", nullable: false),
                CreatedWhen = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                UpdatedWhen = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                ErrorMessage = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                CorrelationId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false)
            },
            constraints: table => { table.PrimaryKey("PK_notification_jobs", x => x.Id); });

        migrationBuilder.CreateTable(
            name: "delivery_attempts",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                NotificationId = table.Column<Guid>(type: "uuid", nullable: false),
                AttemptNumber = table.Column<int>(type: "integer", nullable: false),
                Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                ErrorMessage = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                CreatedWhen = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_delivery_attempts", x => x.Id);
                table.ForeignKey(
                    name: "FK_delivery_attempts_notification_jobs_NotificationId",
                    column: x => x.NotificationId,
                    principalTable: "notification_jobs",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "ix_notification_jobs_status_created_at",
            table: "notification_jobs",
            columns: new[] { "Status", "CreatedWhen" });

        migrationBuilder.CreateIndex(
            name: "ux_notification_jobs_correlation_id",
            table: "notification_jobs",
            column: "CorrelationId",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_delivery_attempts_NotificationId",
            table: "delivery_attempts",
            column: "NotificationId");

        migrationBuilder.CreateIndex(
            name: "ux_delivery_attempts_notification_attempt",
            table: "delivery_attempts",
            columns: new[] { "NotificationId", "AttemptNumber" },
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "delivery_attempts");
        migrationBuilder.DropTable(name: "notification_jobs");
    }
}
