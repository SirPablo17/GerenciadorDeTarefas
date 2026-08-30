using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GerenciadorDeTarefas.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTaskNumberAndUserCounter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NextTaskNumber",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "Number",
                table: "Tasks",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            // Backfill: assign each existing task a per-user sequential number
            // ordered by CreatedAt (ties broken by Id for determinism).
            migrationBuilder.Sql("""
                UPDATE Tasks
                SET Number = (
                    SELECT COUNT(*)
                    FROM Tasks AS t2
                    WHERE t2.UserId = Tasks.UserId
                      AND (t2.CreatedAt < Tasks.CreatedAt
                           OR (t2.CreatedAt = Tasks.CreatedAt AND t2.Id <= Tasks.Id))
                );
                """);

            // Backfill: point each user's counter past their highest existing task number.
            migrationBuilder.Sql("""
                UPDATE Users
                SET NextTaskNumber = (
                    SELECT COALESCE(MAX(Number), 0) + 1
                    FROM Tasks
                    WHERE Tasks.UserId = Users.Id
                );
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NextTaskNumber",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Number",
                table: "Tasks");
        }
    }
}
