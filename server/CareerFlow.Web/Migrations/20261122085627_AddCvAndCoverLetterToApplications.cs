using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CareerFlow.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddCvAndCoverLetterToApplications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_JobApplications_OwnerId",
                table: "JobApplications");

            migrationBuilder.AddColumn<string>(
                name: "CoverLetter",
                table: "JobApplications",
                type: "TEXT",
                maxLength: 3000,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ResumeDocumentId",
                table: "JobApplications",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_JobApplications_OwnerId_JobOpportunityId",
                table: "JobApplications",
                columns: new[] { "OwnerId", "JobOpportunityId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_JobApplications_ResumeDocumentId",
                table: "JobApplications",
                column: "ResumeDocumentId");

            migrationBuilder.AddForeignKey(
                name: "FK_JobApplications_ResumeDocuments_ResumeDocumentId",
                table: "JobApplications",
                column: "ResumeDocumentId",
                principalTable: "ResumeDocuments",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JobApplications_ResumeDocuments_ResumeDocumentId",
                table: "JobApplications");

            migrationBuilder.DropIndex(
                name: "IX_JobApplications_OwnerId_JobOpportunityId",
                table: "JobApplications");

            migrationBuilder.DropIndex(
                name: "IX_JobApplications_ResumeDocumentId",
                table: "JobApplications");

            migrationBuilder.DropColumn(
                name: "CoverLetter",
                table: "JobApplications");

            migrationBuilder.DropColumn(
                name: "ResumeDocumentId",
                table: "JobApplications");

            migrationBuilder.CreateIndex(
                name: "IX_JobApplications_OwnerId",
                table: "JobApplications",
                column: "OwnerId");
        }
    }
}
