using Microsoft.EntityFrameworkCore.Migrations;

namespace Lisbeth.Bot.DataAccessLayer.Migrations
{
    public partial class ids : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "tag",
                newName: "lasted_edit_by_id");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "role_menu",
                newName: "lasted_edit_by_id");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "reminder",
                newName: "lasted_edit_by_id");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "recurring_reminder",
                newName: "lasted_edit_by_id");

            migrationBuilder.AddColumn<long>(
                name: "creator_id",
                table: "tag",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "creator_id",
                table: "role_menu",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "creator_id",
                table: "reminder",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "creator_id",
                table: "recurring_reminder",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "creator_id",
                table: "embed_config",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "last_edit_by_id",
                table: "embed_config",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "creator_id",
                table: "tag");

            migrationBuilder.DropColumn(
                name: "creator_id",
                table: "role_menu");

            migrationBuilder.DropColumn(
                name: "creator_id",
                table: "reminder");

            migrationBuilder.DropColumn(
                name: "creator_id",
                table: "recurring_reminder");

            migrationBuilder.DropColumn(
                name: "creator_id",
                table: "embed_config");

            migrationBuilder.DropColumn(
                name: "last_edit_by_id",
                table: "embed_config");

            migrationBuilder.RenameColumn(
                name: "lasted_edit_by_id",
                table: "tag",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "lasted_edit_by_id",
                table: "role_menu",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "lasted_edit_by_id",
                table: "reminder",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "lasted_edit_by_id",
                table: "recurring_reminder",
                newName: "user_id");
        }
    }
}
