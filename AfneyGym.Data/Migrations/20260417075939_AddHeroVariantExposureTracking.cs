using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AfneyGym.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddHeroVariantExposureTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HeroVariantExposures",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VisitorId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Variant = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HeroVariantExposures", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HeroVariantExposures_Variant",
                table: "HeroVariantExposures",
                column: "Variant");

            migrationBuilder.CreateIndex(
                name: "IX_HeroVariantExposures_VisitorId",
                table: "HeroVariantExposures",
                column: "VisitorId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HeroVariantExposures");
        }
    }
}
