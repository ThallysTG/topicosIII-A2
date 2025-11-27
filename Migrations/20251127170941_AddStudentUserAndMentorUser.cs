using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class AddStudentUserAndMentorUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MentorUserId1",
                table: "MentoringSessions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StudentUserId1",
                table: "MentoringSessions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MentoringSessions_MentorUserId1",
                table: "MentoringSessions",
                column: "MentorUserId1");

            migrationBuilder.CreateIndex(
                name: "IX_MentoringSessions_StudentUserId1",
                table: "MentoringSessions",
                column: "StudentUserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_MentoringSessions_Users_MentorUserId1",
                table: "MentoringSessions",
                column: "MentorUserId1",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MentoringSessions_Users_StudentUserId1",
                table: "MentoringSessions",
                column: "StudentUserId1",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MentoringSessions_Users_MentorUserId1",
                table: "MentoringSessions");

            migrationBuilder.DropForeignKey(
                name: "FK_MentoringSessions_Users_StudentUserId1",
                table: "MentoringSessions");

            migrationBuilder.DropIndex(
                name: "IX_MentoringSessions_MentorUserId1",
                table: "MentoringSessions");

            migrationBuilder.DropIndex(
                name: "IX_MentoringSessions_StudentUserId1",
                table: "MentoringSessions");

            migrationBuilder.DropColumn(
                name: "MentorUserId1",
                table: "MentoringSessions");

            migrationBuilder.DropColumn(
                name: "StudentUserId1",
                table: "MentoringSessions");
        }
    }
}
