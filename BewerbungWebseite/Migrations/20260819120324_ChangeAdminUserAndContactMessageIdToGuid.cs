using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BewerbungsSeite.Migrations
{
    /// <inheritdoc />
    public partial class ChangeAdminUserAndContactMessageIdToGuid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ContactMessage",
                table: "ContactMessage");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "ContactMessage");

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "ContactMessage",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ContactMessage",
                table: "ContactMessage",
                column: "Id");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AdminUser",
                table: "AdminUser");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "AdminUser");

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "AdminUser",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AdminUser",
                table: "AdminUser",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ContactMessage",
                table: "ContactMessage");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "ContactMessage");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "ContactMessage",
                type: "int",
                nullable: false)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ContactMessage",
                table: "ContactMessage",
                column: "Id");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AdminUser",
                table: "AdminUser");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "AdminUser");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "AdminUser",
                type: "int",
                nullable: false)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AdminUser",
                table: "AdminUser",
                column: "Id");
        }
    }
}
