using Microsoft.EntityFrameworkCore.Migrations;

namespace Lisbeth.Bot.DataAccessLayer.Migrations
{
    public partial class cfggg : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "welcome_embed_config_id",
                table: "ticketing_config",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ticketing_config_welcome_embed_config_id",
                table: "ticketing_config",
                column: "welcome_embed_config_id",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ticketing_config_embed_config_welcome_embed_config_id",
                table: "ticketing_config",
                column: "welcome_embed_config_id",
                principalTable: "embed_config",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ticketing_config_embed_config_welcome_embed_config_id",
                table: "ticketing_config");

            migrationBuilder.DropIndex(
                name: "IX_ticketing_config_welcome_embed_config_id",
                table: "ticketing_config");

            migrationBuilder.DropColumn(
                name: "welcome_embed_config_id",
                table: "ticketing_config");
        }
    }
}
