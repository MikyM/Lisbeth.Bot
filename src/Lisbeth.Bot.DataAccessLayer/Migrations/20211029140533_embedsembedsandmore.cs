using Microsoft.EntityFrameworkCore.Migrations;

namespace Lisbeth.Bot.DataAccessLayer.Migrations
{
    public partial class embedsembedsandmore : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_guild_moderation_config_guild_guild_id",
                table: "guild_moderation_config");

            migrationBuilder.DropForeignKey(
                name: "FK_guild_ticketing_config_guild_guild_id",
                table: "guild_ticketing_config");

            migrationBuilder.DropPrimaryKey(
                name: "PK_guild_ticketing_config",
                table: "guild_ticketing_config");

            migrationBuilder.DropPrimaryKey(
                name: "PK_guild_moderation_config",
                table: "guild_moderation_config");

            migrationBuilder.DropColumn(
                name: "ticket_center_message_description",
                table: "guild_ticketing_config");

            migrationBuilder.DropColumn(
                name: "ticket_center_message_fields",
                table: "guild_ticketing_config");

            migrationBuilder.DropColumn(
                name: "member_welcome_message_title",
                table: "guild_moderation_config");

            migrationBuilder.RenameTable(
                name: "guild_ticketing_config",
                newName: "ticketing_config");

            migrationBuilder.RenameTable(
                name: "guild_moderation_config",
                newName: "moderation_config");

            migrationBuilder.RenameColumn(
                name: "ticket_welcome_message_fields",
                table: "ticketing_config",
                newName: "welcome_message_description");

            migrationBuilder.RenameColumn(
                name: "ticket_welcome_message_description",
                table: "ticketing_config",
                newName: "center_message_description");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "reminder",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "center_embed_config_id",
                table: "ticketing_config",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "member_welcome_embed_config_id",
                table: "moderation_config",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ticketing_config",
                table: "ticketing_config",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_moderation_config",
                table: "moderation_config",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "IX_guild_guild_id",
                table: "guild",
                column: "guild_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ticketing_config_center_embed_config_id",
                table: "ticketing_config",
                column: "center_embed_config_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ticketing_config_guild_id",
                table: "ticketing_config",
                column: "guild_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_moderation_config_guild_id",
                table: "moderation_config",
                column: "guild_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_moderation_config_member_welcome_embed_config_id",
                table: "moderation_config",
                column: "member_welcome_embed_config_id",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_moderation_config_embed_config_member_welcome_embed_config_~",
                table: "moderation_config",
                column: "member_welcome_embed_config_id",
                principalTable: "embed_config",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_moderation_config_guild_guild_id",
                table: "moderation_config",
                column: "guild_id",
                principalTable: "guild",
                principalColumn: "guild_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ticketing_config_embed_config_center_embed_config_id",
                table: "ticketing_config",
                column: "center_embed_config_id",
                principalTable: "embed_config",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ticketing_config_guild_guild_id",
                table: "ticketing_config",
                column: "guild_id",
                principalTable: "guild",
                principalColumn: "guild_id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_moderation_config_embed_config_member_welcome_embed_config_~",
                table: "moderation_config");

            migrationBuilder.DropForeignKey(
                name: "FK_moderation_config_guild_guild_id",
                table: "moderation_config");

            migrationBuilder.DropForeignKey(
                name: "FK_ticketing_config_embed_config_center_embed_config_id",
                table: "ticketing_config");

            migrationBuilder.DropForeignKey(
                name: "FK_ticketing_config_guild_guild_id",
                table: "ticketing_config");

            migrationBuilder.DropIndex(
                name: "IX_guild_guild_id",
                table: "guild");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ticketing_config",
                table: "ticketing_config");

            migrationBuilder.DropIndex(
                name: "IX_ticketing_config_center_embed_config_id",
                table: "ticketing_config");

            migrationBuilder.DropIndex(
                name: "IX_ticketing_config_guild_id",
                table: "ticketing_config");

            migrationBuilder.DropPrimaryKey(
                name: "PK_moderation_config",
                table: "moderation_config");

            migrationBuilder.DropIndex(
                name: "IX_moderation_config_guild_id",
                table: "moderation_config");

            migrationBuilder.DropIndex(
                name: "IX_moderation_config_member_welcome_embed_config_id",
                table: "moderation_config");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "reminder");

            migrationBuilder.DropColumn(
                name: "center_embed_config_id",
                table: "ticketing_config");

            migrationBuilder.DropColumn(
                name: "member_welcome_embed_config_id",
                table: "moderation_config");

            migrationBuilder.RenameTable(
                name: "ticketing_config",
                newName: "guild_ticketing_config");

            migrationBuilder.RenameTable(
                name: "moderation_config",
                newName: "guild_moderation_config");

            migrationBuilder.RenameColumn(
                name: "welcome_message_description",
                table: "guild_ticketing_config",
                newName: "ticket_welcome_message_fields");

            migrationBuilder.RenameColumn(
                name: "center_message_description",
                table: "guild_ticketing_config",
                newName: "ticket_welcome_message_description");

            migrationBuilder.AddColumn<string>(
                name: "ticket_center_message_description",
                table: "guild_ticketing_config",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ticket_center_message_fields",
                table: "guild_ticketing_config",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "member_welcome_message_title",
                table: "guild_moderation_config",
                type: "varchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_guild_ticketing_config",
                table: "guild_ticketing_config",
                column: "guild_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_guild_moderation_config",
                table: "guild_moderation_config",
                column: "guild_id");

            migrationBuilder.AddForeignKey(
                name: "FK_guild_moderation_config_guild_guild_id",
                table: "guild_moderation_config",
                column: "guild_id",
                principalTable: "guild",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_guild_ticketing_config_guild_guild_id",
                table: "guild_ticketing_config",
                column: "guild_id",
                principalTable: "guild",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
