using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace nopCommerceReplicatorServices.Migrations
{
    /// <inheritdoc />
    public partial class AddServiceValueToCustomer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ServiceValue",
                table: "DataBinding",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ServiceValue",
                table: "DataBinding");
        }
    }
}
