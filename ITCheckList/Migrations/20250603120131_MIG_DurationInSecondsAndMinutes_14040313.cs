using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ITCheckList.Migrations
{
    /// <inheritdoc />
    public partial class MIG_DurationInSecondsAndMinutes_14040313 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Duration",
                table: "TBLCheckItems",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Duration",
                table: "TBLCheckItemArchives",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Duration",
                table: "TBLCheckItems");

            migrationBuilder.DropColumn(
                name: "Duration",
                table: "TBLCheckItemArchives");
        }
    }
}
