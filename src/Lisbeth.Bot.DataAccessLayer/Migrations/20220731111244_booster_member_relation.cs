using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Lisbeth.Bot.DataAccessLayer.Migrations
{
    public partial class booster_member_relation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_member_history_entry_guild_guild_id",
                table: "member_history_entry");

            migrationBuilder.DropForeignKey(
                name: "FK_reminder_guild_guild_id",
                table: "reminder");

            migrationBuilder.DropForeignKey(
                name: "FK_server_booster_history_entry_guild_guild_id",
                table: "server_booster_history_entry");

            migrationBuilder.DropForeignKey(
                name: "FK_server_booster_history_entry_member_history_entry_guild_id_~",
                table: "server_booster_history_entry");

            migrationBuilder.DropForeignKey(
                name: "FK_tag_guild_guild_id",
                table: "tag");

            migrationBuilder.DropIndex(
                name: "IX_server_booster_history_entry_guild_id_user_id",
                table: "server_booster_history_entry");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_member_history_entry_guild_id_user_id",
                table: "member_history_entry");

            migrationBuilder.AlterColumn<long>(
                name: "user_id",
                table: "server_booster_history_entry",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<long>(
                name: "member_history_entry_id",
                table: "server_booster_history_entry",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_server_booster_history_entry_guild_id",
                table: "server_booster_history_entry",
                column: "guild_id");

            migrationBuilder.CreateIndex(
                name: "IX_server_booster_history_entry_member_history_entry_id",
                table: "server_booster_history_entry",
                column: "member_history_entry_id");

            migrationBuilder.CreateIndex(
                name: "IX_member_history_entry_guild_id",
                table: "member_history_entry",
                column: "guild_id");

            migrationBuilder.AddForeignKey(
                name: "FK_member_history_entry_guild_guild_id",
                table: "member_history_entry",
                column: "guild_id",
                principalTable: "guild",
                principalColumn: "guild_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_reminder_guild_guild_id",
                table: "reminder",
                column: "guild_id",
                principalTable: "guild",
                principalColumn: "guild_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_server_booster_history_entry_guild_guild_id",
                table: "server_booster_history_entry",
                column: "guild_id",
                principalTable: "guild",
                principalColumn: "guild_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_server_booster_history_entry_member_history_entry_member_hi~",
                table: "server_booster_history_entry",
                column: "member_history_entry_id",
                principalTable: "member_history_entry",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_tag_guild_guild_id",
                table: "tag",
                column: "guild_id",
                principalTable: "guild",
                principalColumn: "guild_id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_member_history_entry_guild_guild_id",
                table: "member_history_entry");

            migrationBuilder.DropForeignKey(
                name: "FK_reminder_guild_guild_id",
                table: "reminder");

            migrationBuilder.DropForeignKey(
                name: "FK_server_booster_history_entry_guild_guild_id",
                table: "server_booster_history_entry");

            migrationBuilder.DropForeignKey(
                name: "FK_server_booster_history_entry_member_history_entry_member_hi~",
                table: "server_booster_history_entry");

            migrationBuilder.DropForeignKey(
                name: "FK_tag_guild_guild_id",
                table: "tag");

            migrationBuilder.DropIndex(
                name: "IX_server_booster_history_entry_guild_id",
                table: "server_booster_history_entry");

            migrationBuilder.DropIndex(
                name: "IX_server_booster_history_entry_member_history_entry_id",
                table: "server_booster_history_entry");

            migrationBuilder.DropIndex(
                name: "IX_member_history_entry_guild_id",
                table: "member_history_entry");

            migrationBuilder.DropColumn(
                name: "member_history_entry_id",
                table: "server_booster_history_entry");

            migrationBuilder.AlterColumn<long>(
                name: "user_id",
                table: "server_booster_history_entry",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddUniqueConstraint(
                name: "AK_member_history_entry_guild_id_user_id",
                table: "member_history_entry",
                columns: new[] { "guild_id", "user_id" });

            migrationBuilder.CreateIndex(
                name: "IX_server_booster_history_entry_guild_id_user_id",
                table: "server_booster_history_entry",
                columns: new[] { "guild_id", "user_id" });

            migrationBuilder.AddForeignKey(
                name: "FK_member_history_entry_guild_guild_id",
                table: "member_history_entry",
                column: "guild_id",
                principalTable: "guild",
                principalColumn: "guild_id");

            migrationBuilder.AddForeignKey(
                name: "FK_reminder_guild_guild_id",
                table: "reminder",
                column: "guild_id",
                principalTable: "guild",
                principalColumn: "guild_id");

            migrationBuilder.AddForeignKey(
                name: "FK_server_booster_history_entry_guild_guild_id",
                table: "server_booster_history_entry",
                column: "guild_id",
                principalTable: "guild",
                principalColumn: "guild_id");

            migrationBuilder.AddForeignKey(
                name: "FK_server_booster_history_entry_member_history_entry_guild_id_~",
                table: "server_booster_history_entry",
                columns: new[] { "guild_id", "user_id" },
                principalTable: "member_history_entry",
                principalColumns: new[] { "guild_id", "user_id" });

            migrationBuilder.AddForeignKey(
                name: "FK_tag_guild_guild_id",
                table: "tag",
                column: "guild_id",
                principalTable: "guild",
                principalColumn: "guild_id");
        }
    }
}
