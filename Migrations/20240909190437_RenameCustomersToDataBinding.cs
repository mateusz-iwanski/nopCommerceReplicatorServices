using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace nopCommerceReplicatorServices.Migrations
{
    /// <inheritdoc />
    public partial class RenameCustomersToDataBinding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
            name: "Customers",
            newName: "DataBinding");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
            name: "DataBinding",
            newName: "Customers");
        }
    }
}
