using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobApplier.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EnhancePersistenceLayer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "IX_JobDescriptions_UserId",
                table: "JobDescriptions",
                newName: "IX_JobDescription_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_JobDescriptions_IsOCRExtracted",
                table: "JobDescriptions",
                newName: "IX_JobDescription_IsOCRExtracted");

            migrationBuilder.RenameIndex(
                name: "IX_JobDescriptions_CreatedAt",
                table: "JobDescriptions",
                newName: "IX_JobDescription_CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_CVs_UserId_FileChecksum",
                table: "CVs",
                newName: "IX_CV_UserId_FileChecksum");

            migrationBuilder.RenameIndex(
                name: "IX_CVs_UserId",
                table: "CVs",
                newName: "IX_CV_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_CVs_IsParsed",
                table: "CVs",
                newName: "IX_CV_IsParsed");

            migrationBuilder.RenameIndex(
                name: "IX_CoverLetters_UserId",
                table: "CoverLetters",
                newName: "IX_CoverLetter_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_CoverLetters_JobDescriptionId",
                table: "CoverLetters",
                newName: "IX_CoverLetter_JobDescriptionId");

            migrationBuilder.RenameIndex(
                name: "IX_CoverLetters_CVId_JobDescriptionId",
                table: "CoverLetters",
                newName: "IX_CoverLetter_CVId_JobDescriptionId_Unique");

            migrationBuilder.RenameIndex(
                name: "IX_CoverLetters_CVId",
                table: "CoverLetters",
                newName: "IX_CoverLetter_CVId");

            migrationBuilder.RenameIndex(
                name: "IX_CoverLetters_CreatedAt",
                table: "CoverLetters",
                newName: "IX_CoverLetter_CreatedAt");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Users",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "RefreshTokens",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "JobDescriptions",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "CVs",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "CoverLetters",
                type: "datetime(6)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "JobDescriptions");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "CVs");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "CoverLetters");

            migrationBuilder.RenameIndex(
                name: "IX_JobDescription_UserId",
                table: "JobDescriptions",
                newName: "IX_JobDescriptions_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_JobDescription_IsOCRExtracted",
                table: "JobDescriptions",
                newName: "IX_JobDescriptions_IsOCRExtracted");

            migrationBuilder.RenameIndex(
                name: "IX_JobDescription_CreatedAt",
                table: "JobDescriptions",
                newName: "IX_JobDescriptions_CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_CV_UserId_FileChecksum",
                table: "CVs",
                newName: "IX_CVs_UserId_FileChecksum");

            migrationBuilder.RenameIndex(
                name: "IX_CV_UserId",
                table: "CVs",
                newName: "IX_CVs_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_CV_IsParsed",
                table: "CVs",
                newName: "IX_CVs_IsParsed");

            migrationBuilder.RenameIndex(
                name: "IX_CoverLetter_UserId",
                table: "CoverLetters",
                newName: "IX_CoverLetters_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_CoverLetter_JobDescriptionId",
                table: "CoverLetters",
                newName: "IX_CoverLetters_JobDescriptionId");

            migrationBuilder.RenameIndex(
                name: "IX_CoverLetter_CVId_JobDescriptionId_Unique",
                table: "CoverLetters",
                newName: "IX_CoverLetters_CVId_JobDescriptionId");

            migrationBuilder.RenameIndex(
                name: "IX_CoverLetter_CVId",
                table: "CoverLetters",
                newName: "IX_CoverLetters_CVId");

            migrationBuilder.RenameIndex(
                name: "IX_CoverLetter_CreatedAt",
                table: "CoverLetters",
                newName: "IX_CoverLetters_CreatedAt");
        }
    }
}
