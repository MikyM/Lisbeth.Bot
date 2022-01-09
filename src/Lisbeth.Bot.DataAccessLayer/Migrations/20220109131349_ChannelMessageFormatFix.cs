using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lisbeth.Bot.DataAccessLayer.Migrations
{
    public partial class ChannelMessageFormatFix : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "lat_edit_by_id",
                table: "channel_message_format",
                newName: "last_edit_by_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "last_edit_by_id",
                table: "channel_message_format",
                newName: "lat_edit_by_id");
        }
    }
}
