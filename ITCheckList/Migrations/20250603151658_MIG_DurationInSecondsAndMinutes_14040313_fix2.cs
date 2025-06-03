using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ITCheckList.Migrations
{
    /// <inheritdoc />
    public partial class MIG_DurationInSecondsAndMinutes_14040313_fix2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DurationInSeconds",
                table: "TBLCheckItems");

            migrationBuilder.DropColumn(
                name: "DurationInSeconds",
                table: "TBLCheckItemArchives");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DurationInSeconds",
                table: "TBLCheckItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DurationInSeconds",
                table: "TBLCheckItemArchives",
                type: "int",
                nullable: true);
        }
    }
}
