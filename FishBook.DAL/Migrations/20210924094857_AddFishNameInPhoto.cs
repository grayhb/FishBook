using Microsoft.EntityFrameworkCore.Migrations;

namespace FishBook.DAL.Migrations
{
    public partial class AddFishNameInPhoto : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Tags",
                table: "Photo",
                newName: "FishName");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FishName",
                table: "Photo",
                newName: "Tags");
        }
    }
}
