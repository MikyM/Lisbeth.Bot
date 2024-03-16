using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Lisbeth.Bot.DataAccessLayer.Migrations
{
    public partial class booster_member_history : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "guild_server_booster");

            migrationBuilder.DropTable(
                name: "server_booster");

            migrationBuilder.DropColumn(
                name: "is_disabled",
                table: "audit_log");

            migrationBuilder.CreateTable(
                name: "member_history_entry",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    user_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    guild_id = table.Column<long>(type: "bigint", nullable: false),
                    is_disabled = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_member_history_entry", x => x.id);
                    table.UniqueConstraint("AK_member_history_entry_guild_id_user_id", x => new { x.guild_id, x.user_id });
                    table.ForeignKey(
                        name: "FK_member_history_entry_guild_guild_id",
                        column: x => x.guild_id,
                        principalTable: "guild",
                        principalColumn: "guild_id");
                });

            migrationBuilder.CreateTable(
                name: "server_booster_history_entry",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    guild_id = table.Column<long>(type: "bigint", nullable: false),
                    is_disabled = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_server_booster_history_entry", x => x.id);
                    table.ForeignKey(
                        name: "FK_server_booster_history_entry_guild_guild_id",
                        column: x => x.guild_id,
                        principalTable: "guild",
                        principalColumn: "guild_id");
                    table.ForeignKey(
                        name: "FK_server_booster_history_entry_member_history_entry_guild_id_~",
                        columns: x => new { x.guild_id, x.user_id },
                        principalTable: "member_history_entry",
                        principalColumns: new[] { "guild_id", "user_id" });
                });

            migrationBuilder.CreateIndex(
                name: "IX_server_booster_history_entry_guild_id_user_id",
                table: "server_booster_history_entry",
                columns: new[] { "guild_id", "user_id" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "server_booster_history_entry");

            migrationBuilder.DropTable(
                name: "member_history_entry");

            migrationBuilder.AddColumn<bool>(
                name: "is_disabled",
                table: "audit_log",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "server_booster",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    boost_count = table.Column<int>(type: "int", nullable: false),
                    boosting_since = table.Column<DateTime>(type: "timestamp", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: false),
                    guild_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    is_disabled = table.Column<bool>(type: "boolean", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp", nullable: false),
                    user_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_server_booster", x => x.id);
                    table.UniqueConstraint("AK_server_booster_user_id", x => x.user_id);
                });

            migrationBuilder.CreateTable(
                name: "guild_server_booster",
                columns: table => new
                {
                    guild_id = table.Column<long>(type: "bigint", nullable: false),
                    user_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_guild_server_booster", x => new { x.guild_id, x.user_id });
                    table.ForeignKey(
                        name: "FK_guild_server_booster_guild_guild_id",
                        column: x => x.guild_id,
                        principalTable: "guild",
                        principalColumn: "guild_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_guild_server_booster_server_booster_user_id",
                        column: x => x.user_id,
                        principalTable: "server_booster",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_guild_server_booster_user_id",
                table: "guild_server_booster",
                column: "user_id");
        }
    }
}
