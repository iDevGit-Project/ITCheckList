using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ITCheckList.Migrations
{
    /// <inheritdoc />
    public partial class MIG_DurationInSeconds_14040313 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DurationInSeconds",
                table: "TBLCheckItems",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DurationInSeconds",
                table: "TBLCheckItems");
        }
    }
}
