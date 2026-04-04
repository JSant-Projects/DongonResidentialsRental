using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DongonResidentialsRental.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddedForeignKeysToTablesAccordingly : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddForeignKey(
                name: "FK_credit_allocations_invoices_invoice_id",
                table: "credit_allocations",
                column: "invoice_id",
                principalTable: "invoices",
                principalColumn: "invoice_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_credit_notes_leases_lease_id",
                table: "credit_notes",
                column: "lease_id",
                principalTable: "leases",
                principalColumn: "lease_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_invoices_leases_lease_id",
                table: "invoices",
                column: "lease_id",
                principalTable: "leases",
                principalColumn: "lease_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_payment_allocations_invoices_invoice_id",
                table: "payment_allocations",
                column: "invoice_id",
                principalTable: "invoices",
                principalColumn: "invoice_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_payments_tenants_tenant_id",
                table: "payments",
                column: "tenant_id",
                principalTable: "tenants",
                principalColumn: "tenant_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_units_buildings_building_id",
                table: "units",
                column: "building_id",
                principalTable: "buildings",
                principalColumn: "building_id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_credit_allocations_invoices_invoice_id",
                table: "credit_allocations");

            migrationBuilder.DropForeignKey(
                name: "FK_credit_notes_leases_lease_id",
                table: "credit_notes");

            migrationBuilder.DropForeignKey(
                name: "FK_invoices_leases_lease_id",
                table: "invoices");

            migrationBuilder.DropForeignKey(
                name: "FK_payment_allocations_invoices_invoice_id",
                table: "payment_allocations");

            migrationBuilder.DropForeignKey(
                name: "FK_payments_tenants_tenant_id",
                table: "payments");

            migrationBuilder.DropForeignKey(
                name: "FK_units_buildings_building_id",
                table: "units");
        }
    }
}
