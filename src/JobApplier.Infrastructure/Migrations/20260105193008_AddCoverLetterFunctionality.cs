using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobApplier.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCoverLetterFunctionality : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CoverLetters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    UserId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    CVId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    JobDescriptionId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    GeneratedContent = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    WordCount = table.Column<int>(type: "int", nullable: false),
                    TokensUsed = table.Column<int>(type: "int", nullable: false),
                    Model = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Notes = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoverLetters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CoverLetters_CVs_CVId",
                        column: x => x.CVId,
                        principalTable: "CVs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CoverLetters_JobDescriptions_JobDescriptionId",
                        column: x => x.JobDescriptionId,
                        principalTable: "JobDescriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CoverLetters_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_CoverLetters_CreatedAt",
                table: "CoverLetters",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_CoverLetters_CVId",
                table: "CoverLetters",
                column: "CVId");

            migrationBuilder.CreateIndex(
                name: "IX_CoverLetters_CVId_JobDescriptionId",
                table: "CoverLetters",
                columns: new[] { "CVId", "JobDescriptionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CoverLetters_JobDescriptionId",
                table: "CoverLetters",
                column: "JobDescriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_CoverLetters_UserId",
                table: "CoverLetters",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CoverLetters");
        }
    }
}
