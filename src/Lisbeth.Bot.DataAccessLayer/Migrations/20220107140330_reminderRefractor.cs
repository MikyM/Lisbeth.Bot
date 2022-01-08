using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lisbeth.Bot.DataAccessLayer.Migrations
{
    public partial class reminderRefractor : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "set_for",
                table: "reminder",
                type: "timestamptz",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamptz");

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "reminder",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<string>(
                name: "cron_expression",
                table: "reminder",
                type: "varchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.DropTable(name: "recurring_reminder");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_recurring_reminder_guild_GuildId1",
                table: "recurring_reminder");

            migrationBuilder.DropIndex(
                name: "IX_recurring_reminder_embed_config_id",
                table: "recurring_reminder");

            migrationBuilder.DropIndex(
                name: "IX_recurring_reminder_GuildId1",
                table: "recurring_reminder");

            migrationBuilder.DropColumn(
                name: "cron_expression",
                table: "reminder");

            migrationBuilder.DropColumn(
                name: "GuildId1",
                table: "recurring_reminder");

            migrationBuilder.AlterColumn<DateTime>(
                name: "set_for",
                table: "reminder",
                type: "timestamptz",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamptz",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "reminder",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "varchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_recurring_reminder_embed_config_id",
                table: "recurring_reminder",
                column: "embed_config_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_recurring_reminder_guild_id",
                table: "recurring_reminder",
                column: "guild_id");

            migrationBuilder.AddForeignKey(
                name: "FK_recurring_reminder_guild_guild_id",
                table: "recurring_reminder",
                column: "guild_id",
                principalTable: "guild",
                principalColumn: "guild_id");
        }
    }
}
