using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace FishBook.DAL.Migrations
{
    public partial class AddLonLatDateTimeInPhoto : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DateTime",
                table: "Photo",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Latitude",
                table: "Photo",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Longitude",
                table: "Photo",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateTime",
                table: "Photo");

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "Photo");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "Photo");
        }
    }
}
