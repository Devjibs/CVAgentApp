using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CVAgentApp.API.Migrations
{
    /// <inheritdoc />
    public partial class FixSessionRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GeneratedDocuments_DocumentSession_SessionId",
                table: "GeneratedDocuments");

            migrationBuilder.DropForeignKey(
                name: "FK_GeneratedDocuments_Sessions_SessionId1",
                table: "GeneratedDocuments");

            migrationBuilder.DropTable(
                name: "DocumentSession");

            migrationBuilder.DropIndex(
                name: "IX_GeneratedDocuments_SessionId1",
                table: "GeneratedDocuments");

            migrationBuilder.DropColumn(
                name: "SessionId1",
                table: "GeneratedDocuments");

            migrationBuilder.AddForeignKey(
                name: "FK_GeneratedDocuments_Sessions_SessionId",
                table: "GeneratedDocuments",
                column: "SessionId",
                principalTable: "Sessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GeneratedDocuments_Sessions_SessionId",
                table: "GeneratedDocuments");

            migrationBuilder.AddColumn<Guid>(
                name: "SessionId1",
                table: "GeneratedDocuments",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "DocumentSession",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CandidateId = table.Column<Guid>(type: "TEXT", nullable: false),
                    JobPostingId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ProcessingLog = table.Column<string>(type: "TEXT", nullable: true),
                    SessionToken = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentSession", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentSession_Candidates_CandidateId",
                        column: x => x.CandidateId,
                        principalTable: "Candidates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DocumentSession_JobPostings_JobPostingId",
                        column: x => x.JobPostingId,
                        principalTable: "JobPostings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GeneratedDocuments_SessionId1",
                table: "GeneratedDocuments",
                column: "SessionId1");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentSession_CandidateId",
                table: "DocumentSession",
                column: "CandidateId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentSession_JobPostingId",
                table: "DocumentSession",
                column: "JobPostingId");

            migrationBuilder.AddForeignKey(
                name: "FK_GeneratedDocuments_DocumentSession_SessionId",
                table: "GeneratedDocuments",
                column: "SessionId",
                principalTable: "DocumentSession",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GeneratedDocuments_Sessions_SessionId1",
                table: "GeneratedDocuments",
                column: "SessionId1",
                principalTable: "Sessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
