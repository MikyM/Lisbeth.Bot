using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lisbeth.Bot.DataAccessLayer.Migrations
{
    public partial class hangfirestringid : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "hangfire_id",
                table: "reminder",
                type: "varchar(300)",
                maxLength: 300,
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<string>(
                name: "hangfire_id",
                table: "recurring_reminder",
                type: "varchar(300)",
                maxLength: 300,
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "hangfire_id",
                table: "reminder",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(300)",
                oldMaxLength: 300);

            migrationBuilder.AlterColumn<long>(
                name: "hangfire_id",
                table: "recurring_reminder",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(300)",
                oldMaxLength: 300);
        }
    }
}
