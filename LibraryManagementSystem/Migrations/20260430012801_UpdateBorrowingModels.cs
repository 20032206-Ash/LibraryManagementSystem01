using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LibraryManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class UpdateBorrowingModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Reservations",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Fines",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "BorrowingTransactions",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_BookId",
                table: "Reservations",
                column: "BookId");

            migrationBuilder.CreateIndex(
                name: "IX_Fines_TransactionId",
                table: "Fines",
                column: "TransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_BorrowingTransactions_BookId",
                table: "BorrowingTransactions",
                column: "BookId");

            migrationBuilder.AddForeignKey(
                name: "FK_BorrowingTransactions_Books_BookId",
                table: "BorrowingTransactions",
                column: "BookId",
                principalTable: "Books",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Fines_BorrowingTransactions_TransactionId",
                table: "Fines",
                column: "TransactionId",
                principalTable: "BorrowingTransactions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_Books_BookId",
                table: "Reservations",
                column: "BookId",
                principalTable: "Books",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BorrowingTransactions_Books_BookId",
                table: "BorrowingTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Fines_BorrowingTransactions_TransactionId",
                table: "Fines");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_Books_BookId",
                table: "Reservations");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_BookId",
                table: "Reservations");

            migrationBuilder.DropIndex(
                name: "IX_Fines_TransactionId",
                table: "Fines");

            migrationBuilder.DropIndex(
                name: "IX_BorrowingTransactions_BookId",
                table: "BorrowingTransactions");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Fines");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Reservations",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "BorrowingTransactions",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");
        }
    }
}
