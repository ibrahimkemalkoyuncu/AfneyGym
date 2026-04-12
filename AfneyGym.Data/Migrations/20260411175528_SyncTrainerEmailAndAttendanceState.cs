using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AfneyGym.Data.Migrations
{
    /// <inheritdoc />
    public partial class SyncTrainerEmailAndAttendanceState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Lessons_LessonId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_LessonId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LessonId",
                table: "Users");

            migrationBuilder.Sql(@"UPDATE LessonAttendees
SET CreatedAt = EnrollmentDate");

            migrationBuilder.Sql(@"UPDATE Subscriptions
SET Status = CASE Status
    WHEN 1 THEN 2
    WHEN 2 THEN 4
    WHEN 3 THEN 5
    WHEN 4 THEN 1
    ELSE Status
END");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Trainers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql(@"UPDATE Trainers
SET Email = CONCAT('trainer-', CAST(Id AS nvarchar(36)), '@afneygym.local')
WHERE Email = ''");

            migrationBuilder.AddColumn<bool>(
                name: "IsAttended",
                table: "LessonAttendees",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.DropColumn(
                name: "EnrollmentDate",
                table: "LessonAttendees");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"UPDATE Subscriptions
SET Status = CASE Status
    WHEN 1 THEN 4
    WHEN 2 THEN 1
    WHEN 4 THEN 2
    WHEN 5 THEN 3
    ELSE Status
END");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Trainers");

            migrationBuilder.AddColumn<Guid>(
                name: "LessonId",
                table: "Users",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EnrollmentDate",
                table: "LessonAttendees",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.Sql(@"UPDATE LessonAttendees
SET EnrollmentDate = CreatedAt");

            migrationBuilder.DropColumn(
                name: "IsAttended",
                table: "LessonAttendees");

            migrationBuilder.CreateIndex(
                name: "IX_Users_LessonId",
                table: "Users",
                column: "LessonId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Lessons_LessonId",
                table: "Users",
                column: "LessonId",
                principalTable: "Lessons",
                principalColumn: "Id");
        }
    }
}
