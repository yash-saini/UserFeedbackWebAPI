using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserFeedbackWebAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddedTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "EmailConfirmationTokenExpires",
                table: "Users",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailConfirmationTokenExpires",
                table: "Users");
        }
    }
}
