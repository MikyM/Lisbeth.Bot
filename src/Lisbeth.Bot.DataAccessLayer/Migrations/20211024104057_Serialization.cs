using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Lisbeth.Bot.DataAccessLayer.Migrations
{
    public partial class Serialization : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_recurring_reminder_name",
                table: "recurring_reminder");

            migrationBuilder.CreateTable(
                name: "role_menu",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    guild_id = table.Column<long>(type: "bigint", nullable: false),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    message_id = table.Column<long>(type: "bigint", nullable: false),
                    text = table.Column<string>(type: "text", nullable: true),
                    embed_config_id = table.Column<long>(type: "bigint", nullable: false),
                    role_emoji_mapping = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp", nullable: false),
                    is_disabled = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_role_menu", x => x.id);
                    table.ForeignKey(
                        name: "FK_role_menu_embed_config_embed_config_id",
                        column: x => x.embed_config_id,
                        principalTable: "embed_config",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_role_menu_guild_guild_id",
                        column: x => x.guild_id,
                        principalTable: "guild",
                        principalColumn: "guild_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_role_menu_embed_config_id",
                table: "role_menu",
                column: "embed_config_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_role_menu_guild_id",
                table: "role_menu",
                column: "guild_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "role_menu");

            migrationBuilder.CreateIndex(
                name: "IX_recurring_reminder_name",
                table: "recurring_reminder",
                column: "name",
                unique: true);
        }
    }
}
