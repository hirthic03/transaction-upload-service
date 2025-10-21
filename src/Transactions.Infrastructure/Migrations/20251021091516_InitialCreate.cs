using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Transactions.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "imports",
                columns: table => new
                {
                    import_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    received_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    source_format = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    sha256 = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    record_count = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_imports", x => x.import_id);
                });

            migrationBuilder.CreateTable(
                name: "import_errors",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    import_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    row_number = table.Column<int>(type: "int", nullable: false),
                    field = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    message = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_import_errors", x => x.id);
                    table.ForeignKey(
                        name: "FK_import_errors_imports_import_id",
                        column: x => x.import_id,
                        principalTable: "imports",
                        principalColumn: "import_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "transactions",
                columns: table => new
                {
                    id = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    currency_code = table.Column<string>(type: "char(3)", nullable: false),
                    transaction_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    status_code = table.Column<string>(type: "char(1)", nullable: false),
                    source_format = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    import_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transactions", x => x.id);
                    table.ForeignKey(
                        name: "FK_transactions_imports_import_id",
                        column: x => x.import_id,
                        principalTable: "imports",
                        principalColumn: "import_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_import_errors_import_id",
                table: "import_errors",
                column: "import_id");

            migrationBuilder.CreateIndex(
                name: "IX_Imports_Sha256Hash",
                table: "imports",
                column: "sha256",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_CurrencyCode",
                table: "transactions",
                column: "currency_code");

            migrationBuilder.CreateIndex(
                name: "IX_transactions_import_id",
                table: "transactions",
                column: "import_id");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_StatusCode",
                table: "transactions",
                column: "status_code");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_TransactionDate",
                table: "transactions",
                column: "transaction_date");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "import_errors");

            migrationBuilder.DropTable(
                name: "transactions");

            migrationBuilder.DropTable(
                name: "imports");
        }
    }
}
