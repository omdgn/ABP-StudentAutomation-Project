using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace abp_obs_project.Migrations
{
    /// <inheritdoc />
    public partial class AddUnaccentExtension : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Enable unaccent extension for Turkish character support
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS unaccent;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop unaccent extension
            migrationBuilder.Sql("DROP EXTENSION IF EXISTS unaccent;");
        }
    }
}
