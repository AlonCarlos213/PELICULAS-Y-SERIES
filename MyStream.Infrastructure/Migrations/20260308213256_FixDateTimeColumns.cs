using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyStream.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixDateTimeColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE \"Users\" ALTER COLUMN \"CreatedAt\" TYPE timestamp with time zone USING \"CreatedAt\"::timestamp with time zone;");
            migrationBuilder.Sql("ALTER TABLE \"Users\" ALTER COLUMN \"UpdatedAt\" TYPE timestamp with time zone USING \"UpdatedAt\"::timestamp with time zone;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
