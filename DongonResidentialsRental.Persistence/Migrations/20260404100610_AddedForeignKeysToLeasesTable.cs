using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DongonResidentialsRental.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddedForeignKeysToLeasesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddForeignKey(
                name: "FK_leases_tenants_tenant_id",
                table: "leases",
                column: "tenant_id",
                principalTable: "tenants",
                principalColumn: "tenant_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_leases_units_unit_id",
                table: "leases",
                column: "unit_id",
                principalTable: "units",
                principalColumn: "unit_id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_leases_tenants_tenant_id",
                table: "leases");

            migrationBuilder.DropForeignKey(
                name: "FK_leases_units_unit_id",
                table: "leases");
        }
    }
}
