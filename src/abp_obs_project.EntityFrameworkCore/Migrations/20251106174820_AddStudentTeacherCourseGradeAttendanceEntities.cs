using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace abp_obs_project.Migrations
{
    /// <inheritdoc />
    public partial class AddStudentTeacherCourseGradeAttendanceEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppStudents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FirstName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    LastName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    StudentNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    BirthDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Gender = table.Column<int>(type: "integer", nullable: false),
                    PhoneNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
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
                    table.PrimaryKey("PK_AppStudents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppTeachers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FirstName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    LastName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Department = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    PhoneNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
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
                    table.PrimaryKey("PK_AppTeachers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppCourses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Credits = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    TeacherId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_AppCourses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppCourses_AppTeachers_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "AppTeachers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AppAttendances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentId = table.Column<Guid>(type: "uuid", nullable: false),
                    CourseId = table.Column<Guid>(type: "uuid", nullable: false),
                    AttendanceDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    IsPresent = table.Column<bool>(type: "boolean", nullable: false),
                    Remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_AppAttendances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppAttendances_AppCourses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "AppCourses",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AppAttendances_AppStudents_StudentId",
                        column: x => x.StudentId,
                        principalTable: "AppStudents",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AppGrades",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentId = table.Column<Guid>(type: "uuid", nullable: false),
                    CourseId = table.Column<Guid>(type: "uuid", nullable: false),
                    GradeValue = table.Column<double>(type: "numeric(5,2)", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Comments = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    GradedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
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
                    table.PrimaryKey("PK_AppGrades", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppGrades_AppCourses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "AppCourses",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AppGrades_AppStudents_StudentId",
                        column: x => x.StudentId,
                        principalTable: "AppStudents",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppAttendances_CourseId",
                table: "AppAttendances",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_AppAttendances_StudentId",
                table: "AppAttendances",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_AppAttendances_StudentId_CourseId_AttendanceDate",
                table: "AppAttendances",
                columns: new[] { "StudentId", "CourseId", "AttendanceDate" });

            migrationBuilder.CreateIndex(
                name: "IX_AppCourses_Code",
                table: "AppCourses",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_AppCourses_TeacherId",
                table: "AppCourses",
                column: "TeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_AppGrades_CourseId",
                table: "AppGrades",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_AppGrades_StudentId",
                table: "AppGrades",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_AppGrades_StudentId_CourseId",
                table: "AppGrades",
                columns: new[] { "StudentId", "CourseId" });

            migrationBuilder.CreateIndex(
                name: "IX_AppStudents_Email",
                table: "AppStudents",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_AppStudents_StudentNumber",
                table: "AppStudents",
                column: "StudentNumber");

            migrationBuilder.CreateIndex(
                name: "IX_AppTeachers_Email",
                table: "AppTeachers",
                column: "Email");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppAttendances");

            migrationBuilder.DropTable(
                name: "AppGrades");

            migrationBuilder.DropTable(
                name: "AppCourses");

            migrationBuilder.DropTable(
                name: "AppStudents");

            migrationBuilder.DropTable(
                name: "AppTeachers");
        }
    }
}
