using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyStream.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLibraryCreatedBy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "Libraries",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Libraries_CreatedBy",
                table: "Libraries",
                column: "CreatedBy");

            migrationBuilder.AddForeignKey(
                name: "FK_Libraries_Users_CreatedBy",
                table: "Libraries",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Libraries_Users_CreatedBy",
                table: "Libraries");

            migrationBuilder.DropIndex(
                name: "IX_Libraries_CreatedBy",
                table: "Libraries");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Libraries");
        }
    }
}
