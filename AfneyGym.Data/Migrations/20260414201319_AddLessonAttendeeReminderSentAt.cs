using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AfneyGym.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLessonAttendeeReminderSentAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ReminderSentAt",
                table: "LessonAttendees",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LessonAttendees_ReminderSentAt",
                table: "LessonAttendees",
                column: "ReminderSentAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LessonAttendees_ReminderSentAt",
                table: "LessonAttendees");

            migrationBuilder.DropColumn(
                name: "ReminderSentAt",
                table: "LessonAttendees");
        }
    }
}
