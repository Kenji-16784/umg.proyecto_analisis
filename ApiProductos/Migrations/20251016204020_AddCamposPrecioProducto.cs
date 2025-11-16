using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiProductos.Migrations
{
    /// <inheritdoc />
    public partial class AddCamposPrecioProducto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Compras_Proveedores_ProveedorId",
                table: "Compras");

            migrationBuilder.DropForeignKey(
                name: "FK_Compras_ReglasPrecio_ReglaPrecioId",
                table: "Compras");

            migrationBuilder.DropForeignKey(
                name: "FK_ComprasDetalles_Presentaciones_PresentacionId",
                table: "ComprasDetalles");

            migrationBuilder.DropForeignKey(
                name: "FK_ComprasDetalles_Productos_ProductoId",
                table: "ComprasDetalles");

            migrationBuilder.DropForeignKey(
                name: "FK_ComprasDetalles_Unidades_UnidadId",
                table: "ComprasDetalles");

            migrationBuilder.DropForeignKey(
                name: "FK_Ventas_Cliente_ClienteId",
                table: "Ventas");

            migrationBuilder.AddColumn<string>(
                name: "CodigoAlterno",
                table: "Productos",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PrecioCosto",
                table: "Productos",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PrecioVenta",
                table: "Productos",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddForeignKey(
                name: "FK_Compras_Proveedores_ProveedorId",
                table: "Compras",
                column: "ProveedorId",
                principalTable: "Proveedores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Compras_ReglasPrecio_ReglaPrecioId",
                table: "Compras",
                column: "ReglaPrecioId",
                principalTable: "ReglasPrecio",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ComprasDetalles_Presentaciones_PresentacionId",
                table: "ComprasDetalles",
                column: "PresentacionId",
                principalTable: "Presentaciones",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ComprasDetalles_Productos_ProductoId",
                table: "ComprasDetalles",
                column: "ProductoId",
                principalTable: "Productos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ComprasDetalles_Unidades_UnidadId",
                table: "ComprasDetalles",
                column: "UnidadId",
                principalTable: "Unidades",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Ventas_Cliente_ClienteId",
                table: "Ventas",
                column: "ClienteId",
                principalTable: "Cliente",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Compras_Proveedores_ProveedorId",
                table: "Compras");

            migrationBuilder.DropForeignKey(
                name: "FK_Compras_ReglasPrecio_ReglaPrecioId",
                table: "Compras");

            migrationBuilder.DropForeignKey(
                name: "FK_ComprasDetalles_Presentaciones_PresentacionId",
                table: "ComprasDetalles");

            migrationBuilder.DropForeignKey(
                name: "FK_ComprasDetalles_Productos_ProductoId",
                table: "ComprasDetalles");

            migrationBuilder.DropForeignKey(
                name: "FK_ComprasDetalles_Unidades_UnidadId",
                table: "ComprasDetalles");

            migrationBuilder.DropForeignKey(
                name: "FK_Ventas_Cliente_ClienteId",
                table: "Ventas");

            migrationBuilder.DropColumn(
                name: "CodigoAlterno",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "PrecioCosto",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "PrecioVenta",
                table: "Productos");

            migrationBuilder.AddForeignKey(
                name: "FK_Compras_Proveedores_ProveedorId",
                table: "Compras",
                column: "ProveedorId",
                principalTable: "Proveedores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Compras_ReglasPrecio_ReglaPrecioId",
                table: "Compras",
                column: "ReglaPrecioId",
                principalTable: "ReglasPrecio",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ComprasDetalles_Presentaciones_PresentacionId",
                table: "ComprasDetalles",
                column: "PresentacionId",
                principalTable: "Presentaciones",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ComprasDetalles_Productos_ProductoId",
                table: "ComprasDetalles",
                column: "ProductoId",
                principalTable: "Productos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ComprasDetalles_Unidades_UnidadId",
                table: "ComprasDetalles",
                column: "UnidadId",
                principalTable: "Unidades",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Ventas_Cliente_ClienteId",
                table: "Ventas",
                column: "ClienteId",
                principalTable: "Cliente",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
