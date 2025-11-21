using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    Role = table.Column<string>(type: "text", nullable: false),
                    AreaInteresse = table.Column<string>(type: "text", nullable: true),
                    InepCode = table.Column<string>(type: "text", nullable: true),
                    Bio = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MentoringSessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StudentUserId = table.Column<int>(type: "integer", nullable: false),
                    MentorUserId = table.Column<int>(type: "integer", nullable: false),
                    ScheduledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SessionStatus = table.Column<string>(type: "text", nullable: false),
                    NotesMentor = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MentoringSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MentoringSession_Mentor",
                        column: x => x.MentorUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MentoringSession_Student",
                        column: x => x.StudentUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RecommendationLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StudentUserId = table.Column<int>(type: "integer", nullable: false),
                    PromptSent = table.Column<string>(type: "text", nullable: false),
                    ResponseSummary = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecommendationLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecommendationLogs_Users_StudentUserId",
                        column: x => x.StudentUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StudyTracks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StudentUserId = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Source = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudyTracks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudyTracks_Users_StudentUserId",
                        column: x => x.StudentUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudyTracks_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "StudyActivities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StudyTrackId = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Link = table.Column<string>(type: "text", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    ActivityStatus = table.Column<string>(type: "text", nullable: false),
                    StudyTrackId1 = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudyActivities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudyActivities_StudyTracks_StudyTrackId",
                        column: x => x.StudyTrackId,
                        principalTable: "StudyTracks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudyActivities_StudyTracks_StudyTrackId1",
                        column: x => x.StudyTrackId1,
                        principalTable: "StudyTracks",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_MentoringSessions_MentorUserId",
                table: "MentoringSessions",
                column: "MentorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_MentoringSessions_StudentUserId",
                table: "MentoringSessions",
                column: "StudentUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RecommendationLogs_StudentUserId",
                table: "RecommendationLogs",
                column: "StudentUserId");

            migrationBuilder.CreateIndex(
                name: "IX_StudyActivities_StudyTrackId",
                table: "StudyActivities",
                column: "StudyTrackId");

            migrationBuilder.CreateIndex(
                name: "IX_StudyActivities_StudyTrackId1",
                table: "StudyActivities",
                column: "StudyTrackId1");

            migrationBuilder.CreateIndex(
                name: "IX_StudyTracks_StudentUserId",
                table: "StudyTracks",
                column: "StudentUserId");

            migrationBuilder.CreateIndex(
                name: "IX_StudyTracks_UserId",
                table: "StudyTracks",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MentoringSessions");

            migrationBuilder.DropTable(
                name: "RecommendationLogs");

            migrationBuilder.DropTable(
                name: "StudyActivities");

            migrationBuilder.DropTable(
                name: "StudyTracks");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
