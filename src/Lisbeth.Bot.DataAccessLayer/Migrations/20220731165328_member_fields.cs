using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Lisbeth.Bot.DataAccessLayer.Migrations
{
    public partial class member_fields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "punishment",
                table: "member_history_entry",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "punishment_by_id",
                table: "member_history_entry",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "punishment_by_username",
                table: "member_history_entry",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "punishment_reason",
                table: "member_history_entry",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "punishment",
                table: "member_history_entry");

            migrationBuilder.DropColumn(
                name: "punishment_by_id",
                table: "member_history_entry");

            migrationBuilder.DropColumn(
                name: "punishment_by_username",
                table: "member_history_entry");

            migrationBuilder.DropColumn(
                name: "punishment_reason",
                table: "member_history_entry");
        }
    }
}
