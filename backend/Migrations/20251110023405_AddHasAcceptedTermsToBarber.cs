using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace barbearia.api.Migrations
{
    /// <inheritdoc />
    public partial class AddHasAcceptedTermsToBarber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasAcceptedTerms",
                table: "Barbers",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasAcceptedTerms",
                table: "Barbers");
        }
    }
}
