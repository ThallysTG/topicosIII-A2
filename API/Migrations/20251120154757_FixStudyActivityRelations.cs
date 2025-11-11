using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class FixStudyActivityRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StudyActivities_StudyTracks_StudyTrackId1",
                table: "StudyActivities");

            migrationBuilder.DropForeignKey(
                name: "FK_StudyTracks_Users_UserId",
                table: "StudyTracks");

            migrationBuilder.DropIndex(
                name: "IX_StudyTracks_UserId",
                table: "StudyTracks");

            migrationBuilder.DropIndex(
                name: "IX_StudyActivities_StudyTrackId1",
                table: "StudyActivities");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "StudyTracks");

            migrationBuilder.DropColumn(
                name: "StudyTrackId1",
                table: "StudyActivities");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "StudyTracks",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StudyTrackId1",
                table: "StudyActivities",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudyTracks_UserId",
                table: "StudyTracks",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_StudyActivities_StudyTrackId1",
                table: "StudyActivities",
                column: "StudyTrackId1");

            migrationBuilder.AddForeignKey(
                name: "FK_StudyActivities_StudyTracks_StudyTrackId1",
                table: "StudyActivities",
                column: "StudyTrackId1",
                principalTable: "StudyTracks",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_StudyTracks_Users_UserId",
                table: "StudyTracks",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
