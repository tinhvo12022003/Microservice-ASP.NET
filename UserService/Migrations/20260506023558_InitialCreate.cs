using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserMicroservice.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "UNIQUEIDENTIFIER", nullable: false),
                    Fname = table.Column<string>(type: "NVARCHAR(100)", nullable: false),
                    Lname = table.Column<string>(type: "NVARCHAR(100)", nullable: false),
                    Email = table.Column<string>(type: "NVARCHAR(255)", nullable: false),
                    PasswordHash = table.Column<string>(type: "NVARCHAR(255)", nullable: false),
                    Gender = table.Column<bool>(type: "BIT", nullable: false),
                    BirthDate = table.Column<DateTime>(type: "DATE", nullable: false),
                    Phone = table.Column<string>(type: "NVARCHAR(20)", nullable: false),
                    Role = table.Column<int>(type: "INT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "DATETIME2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "NVARCHAR(100)", nullable: true, defaultValue: "SYSTEM"),
                    UpdatedAt = table.Column<DateTime>(type: "DATETIME2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedBy = table.Column<string>(type: "NVARCHAR(100)", nullable: true),
                    Status = table.Column<bool>(type: "BIT", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_Fname",
                table: "Users",
                column: "Fname",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
