using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lisbeth.Bot.DataAccessLayer.Migrations
{
    public partial class tagrestdd : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Name",
                table: "reminder",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "bigint",
                table: "reminder",
                newName: "channel_id");

            migrationBuilder.RenameColumn(
                name: "bigint",
                table: "recurring_reminder",
                newName: "channel_id");

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "reminder",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "messages",
                table: "prune",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "name",
                table: "reminder",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "channel_id",
                table: "reminder",
                newName: "bigint");

            migrationBuilder.RenameColumn(
                name: "channel_id",
                table: "recurring_reminder",
                newName: "bigint");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "reminder",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "messages",
                table: "prune",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");
        }
    }
}
