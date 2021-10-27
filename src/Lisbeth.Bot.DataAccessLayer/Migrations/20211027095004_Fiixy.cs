using Microsoft.EntityFrameworkCore.Migrations;

namespace Lisbeth.Bot.DataAccessLayer.Migrations
{
    public partial class Fiixy : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_tag_name",
                table: "tag");

            migrationBuilder.RenameColumn(
                name: "set_for_date",
                table: "reminder",
                newName: "set_for");

            migrationBuilder.AlterColumn<long>(
                name: "embed_config_id",
                table: "role_menu",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<long>(
                name: "guild_id",
                table: "reminder",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_guild_reminder",
                table: "reminder",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<long>(
                name: "guild_id",
                table: "recurring_reminder",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_guild_reminder",
                table: "recurring_reminder",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "author_url",
                table: "embed_config",
                type: "varchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "hex_color",
                table: "embed_config",
                type: "varchar(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_guild_reminder",
                table: "reminder");

            migrationBuilder.DropColumn(
                name: "is_guild_reminder",
                table: "recurring_reminder");

            migrationBuilder.DropColumn(
                name: "author_url",
                table: "embed_config");

            migrationBuilder.DropColumn(
                name: "hex_color",
                table: "embed_config");

            migrationBuilder.RenameColumn(
                name: "set_for",
                table: "reminder",
                newName: "set_for_date");

            migrationBuilder.AlterColumn<long>(
                name: "embed_config_id",
                table: "role_menu",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "guild_id",
                table: "reminder",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<long>(
                name: "guild_id",
                table: "recurring_reminder",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.CreateIndex(
                name: "IX_tag_name",
                table: "tag",
                column: "name",
                unique: true);
        }
    }
}
