using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ARAS.Auth.Api.Migrations
{
    /// <inheritdoc />
    public partial class renamedOid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OId",
                table: "AspNetUsers",
                newName: "OpenId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OpenId",
                table: "AspNetUsers",
                newName: "OId");
        }
    }
}
