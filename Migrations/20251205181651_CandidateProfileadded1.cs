using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace cv.deltareco.com.Migrations
{
    /// <inheritdoc />
    public partial class CandidateProfileadded1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CVId",
                table: "CandidateProfiles");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CVId",
                table: "CandidateProfiles",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
