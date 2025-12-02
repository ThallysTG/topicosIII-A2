using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class AddTrackInstitutions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.CreateTable(
                name: "TrackInstitution",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudyTrackId = table.Column<int>(type: "int", nullable: false),
                    InstitutionName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CourseName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    State = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackInstitution", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrackInstitution_StudyTracks_StudyTrackId",
                        column: x => x.StudyTrackId,
                        principalTable: "StudyTracks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TrackInstitution_StudyTrackId",
                table: "TrackInstitution",
                column: "StudyTrackId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TrackInstitution");

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
    }
}
