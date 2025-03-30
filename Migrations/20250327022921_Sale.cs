using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SuperMarket.Migrations
{
    /// <inheritdoc />
    public partial class Sale : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SaleId",
                table: "Iitems",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "sales",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    total = table.Column<double>(type: "float", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sales", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Iitems_SaleId",
                table: "Iitems",
                column: "SaleId");

            migrationBuilder.AddForeignKey(
                name: "FK_Iitems_sales_SaleId",
                table: "Iitems",
                column: "SaleId",
                principalTable: "sales",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Iitems_sales_SaleId",
                table: "Iitems");

            migrationBuilder.DropTable(
                name: "sales");

            migrationBuilder.DropIndex(
                name: "IX_Iitems_SaleId",
                table: "Iitems");

            migrationBuilder.DropColumn(
                name: "SaleId",
                table: "Iitems");
        }
    }
}
