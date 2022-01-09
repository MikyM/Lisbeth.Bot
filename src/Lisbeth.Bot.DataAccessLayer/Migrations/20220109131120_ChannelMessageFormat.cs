using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Lisbeth.Bot.DataAccessLayer.Migrations
{
    public partial class ChannelMessageFormat : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "user_id",
                table: "audit_log",
                type: "varchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(100)",
                oldMaxLength: 100);

            migrationBuilder.CreateTable(
                name: "channel_message_format",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    channel_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    creator_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    lat_edit_by_id = table.Column<long>(type: "bigint", nullable: false),
                    message_format = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    is_disabled = table.Column<bool>(type: "boolean", nullable: false),
                    guild_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_channel_message_format", x => x.id);
                    table.ForeignKey(
                        name: "FK_channel_message_format_guild_guild_id",
                        column: x => x.guild_id,
                        principalTable: "guild",
                        principalColumn: "guild_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_channel_message_format_channel_id",
                table: "channel_message_format",
                column: "channel_id");

            migrationBuilder.CreateIndex(
                name: "IX_channel_message_format_guild_id",
                table: "channel_message_format",
                column: "guild_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "channel_message_format");

            migrationBuilder.AlterColumn<string>(
                name: "user_id",
                table: "audit_log",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "varchar(200)",
                oldMaxLength: 200,
                oldNullable: true);
        }
    }
}
