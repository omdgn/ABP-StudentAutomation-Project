using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace abp_obs_project.Migrations
{
    /// <inheritdoc />
    public partial class AddedEnrollmentEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppEnrollments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentId = table.Column<Guid>(type: "uuid", nullable: false),
                    CourseId = table.Column<Guid>(type: "uuid", nullable: false),
                    EnrolledAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    WithdrawnAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppEnrollments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppEnrollments_AppCourses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "AppCourses",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AppEnrollments_AppStudents_StudentId",
                        column: x => x.StudentId,
                        principalTable: "AppStudents",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppEnrollments_CourseId",
                table: "AppEnrollments",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_AppEnrollments_Status",
                table: "AppEnrollments",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_AppEnrollments_StudentId",
                table: "AppEnrollments",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_AppEnrollments_StudentId_CourseId",
                table: "AppEnrollments",
                columns: new[] { "StudentId", "CourseId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppEnrollments");
        }
    }
}
