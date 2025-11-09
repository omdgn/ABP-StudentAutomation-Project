using abp_obs_project.Attendances;
using abp_obs_project.Courses;
using abp_obs_project.Enrollments;
using abp_obs_project.Grades;
using abp_obs_project.Students;
using abp_obs_project.Teachers;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.AuditLogging.EntityFrameworkCore;
using Volo.Abp.BackgroundJobs.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Modeling;
using Volo.Abp.FeatureManagement.EntityFrameworkCore;
using Volo.Abp.Identity;
using Volo.Abp.Identity.EntityFrameworkCore;
using Volo.Abp.OpenIddict.EntityFrameworkCore;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement.EntityFrameworkCore;
using Volo.Abp.TenantManagement;
using Volo.Abp.TenantManagement.EntityFrameworkCore;

namespace abp_obs_project.EntityFrameworkCore;

[ReplaceDbContext(typeof(IIdentityDbContext))]
[ReplaceDbContext(typeof(ITenantManagementDbContext))]
[ConnectionStringName("Default")]
public class abp_obs_projectDbContext :
    AbpDbContext<abp_obs_projectDbContext>,
    IIdentityDbContext,
    ITenantManagementDbContext
{
    /* Add DbSet properties for your Aggregate Roots / Entities here. */

    #region Student Automation Entities

    public DbSet<Student> Students { get; set; } = null!;
    public DbSet<Teacher> Teachers { get; set; } = null!;
    public DbSet<Course> Courses { get; set; } = null!;
    public DbSet<Grade> Grades { get; set; } = null!;
    public DbSet<Attendance> Attendances { get; set; } = null!;
    public DbSet<Enrollment> Enrollments { get; set; } = null!;

    #endregion

    #region Entities from the modules

    /* Notice: We only implemented IIdentityDbContext and ITenantManagementDbContext
     * and replaced them for this DbContext. This allows you to perform JOIN
     * queries for the entities of these modules over the repositories easily. You
     * typically don't need that for other modules. But, if you need, you can
     * implement the DbContext interface of the needed module and use ReplaceDbContext
     * attribute just like IIdentityDbContext and ITenantManagementDbContext.
     *
     * More info: Replacing a DbContext of a module ensures that the related module
     * uses this DbContext on runtime. Otherwise, it will use its own DbContext class.
     */

    //Identity
    public DbSet<IdentityUser> Users { get; set; }
    public DbSet<IdentityRole> Roles { get; set; }
    public DbSet<IdentityClaimType> ClaimTypes { get; set; }
    public DbSet<OrganizationUnit> OrganizationUnits { get; set; }
    public DbSet<IdentitySecurityLog> SecurityLogs { get; set; }
    public DbSet<IdentityLinkUser> LinkUsers { get; set; }
    public DbSet<IdentityUserDelegation> UserDelegations { get; set; }
    public DbSet<IdentitySession> Sessions { get; set; }
    // Tenant Management
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<TenantConnectionString> TenantConnectionStrings { get; set; }

    #endregion

    public abp_obs_projectDbContext(DbContextOptions<abp_obs_projectDbContext> options)
        : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        /* Include modules to your migration db context */

        builder.ConfigurePermissionManagement();
        builder.ConfigureSettingManagement();
        builder.ConfigureBackgroundJobs();
        builder.ConfigureAuditLogging();
        builder.ConfigureIdentity();
        builder.ConfigureOpenIddict();
        builder.ConfigureFeatureManagement();
        builder.ConfigureTenantManagement();

        /* Configure your own tables/entities inside here */

        // Student entity configuration
        builder.Entity<Student>(b =>
        {
            b.ToTable(abp_obs_projectConsts.DbTablePrefix + "Students", abp_obs_projectConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.FirstName)
                .HasColumnName(nameof(Student.FirstName))
                .IsRequired()
                .HasMaxLength(StudentConsts.MaxFirstNameLength);

            b.Property(x => x.LastName)
                .HasColumnName(nameof(Student.LastName))
                .IsRequired()
                .HasMaxLength(StudentConsts.MaxLastNameLength);

            b.Property(x => x.Email)
                .HasColumnName(nameof(Student.Email))
                .IsRequired()
                .HasMaxLength(StudentConsts.MaxEmailLength);

            b.Property(x => x.StudentNumber)
                .HasColumnName(nameof(Student.StudentNumber))
                .IsRequired()
                .HasMaxLength(StudentConsts.MaxStudentNumberLength);

            b.Property(x => x.PhoneNumber)
                .HasColumnName(nameof(Student.PhoneNumber))
                .IsRequired(false)
                .HasMaxLength(StudentConsts.MaxPhoneLength);

            b.Property(x => x.Gender)
                .HasColumnName(nameof(Student.Gender))
                .IsRequired();

            b.Property(x => x.BirthDate)
                .HasColumnName(nameof(Student.BirthDate))
                .IsRequired();

            // Indexes
            b.HasIndex(x => x.Email);
            b.HasIndex(x => x.StudentNumber);
        });

        // Teacher entity configuration
        builder.Entity<Teacher>(b =>
        {
            b.ToTable(abp_obs_projectConsts.DbTablePrefix + "Teachers", abp_obs_projectConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.FirstName)
                .HasColumnName(nameof(Teacher.FirstName))
                .IsRequired()
                .HasMaxLength(TeacherConsts.MaxFirstNameLength);

            b.Property(x => x.LastName)
                .HasColumnName(nameof(Teacher.LastName))
                .IsRequired()
                .HasMaxLength(TeacherConsts.MaxLastNameLength);

            b.Property(x => x.Email)
                .HasColumnName(nameof(Teacher.Email))
                .IsRequired()
                .HasMaxLength(TeacherConsts.MaxEmailLength);

            b.Property(x => x.Department)
                .HasColumnName(nameof(Teacher.Department))
                .IsRequired(false)
                .HasMaxLength(TeacherConsts.MaxDepartmentLength);

            b.Property(x => x.PhoneNumber)
                .HasColumnName(nameof(Teacher.PhoneNumber))
                .IsRequired(false)
                .HasMaxLength(TeacherConsts.MaxPhoneNumberLength);

            // Indexes
            b.HasIndex(x => x.Email);
        });

        // Course entity configuration
        builder.Entity<Course>(b =>
        {
            b.ToTable(abp_obs_projectConsts.DbTablePrefix + "Courses", abp_obs_projectConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.Name)
                .HasColumnName(nameof(Course.Name))
                .IsRequired()
                .HasMaxLength(CourseConsts.MaxNameLength);

            b.Property(x => x.Code)
                .HasColumnName(nameof(Course.Code))
                .IsRequired()
                .HasMaxLength(CourseConsts.MaxCodeLength);

            b.Property(x => x.Credits)
                .HasColumnName(nameof(Course.Credits))
                .IsRequired();

            b.Property(x => x.Description)
                .HasColumnName(nameof(Course.Description))
                .IsRequired(false)
                .HasMaxLength(CourseConsts.MaxDescriptionLength);

            b.Property(x => x.Status)
                .HasColumnName(nameof(Course.Status))
                .IsRequired();

            b.Property(x => x.TeacherId)
                .HasColumnName(nameof(Course.TeacherId))
                .IsRequired();

            // Foreign key with no cascade delete
            b.HasOne<Teacher>()
                .WithMany()
                .HasForeignKey(x => x.TeacherId)
                .OnDelete(DeleteBehavior.NoAction);

            // Indexes
            b.HasIndex(x => x.Code);
            b.HasIndex(x => x.TeacherId);
        });

        // Grade entity configuration
        builder.Entity<Grade>(b =>
        {
            b.ToTable(abp_obs_projectConsts.DbTablePrefix + "Grades", abp_obs_projectConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.StudentId)
                .HasColumnName(nameof(Grade.StudentId))
                .IsRequired();

            b.Property(x => x.CourseId)
                .HasColumnName(nameof(Grade.CourseId))
                .IsRequired();

            b.Property(x => x.GradeValue)
                .HasColumnName(nameof(Grade.GradeValue))
                .IsRequired()
                .HasColumnType("decimal(5,2)");

            b.Property(x => x.Status)
                .HasColumnName(nameof(Grade.Status))
                .IsRequired();

            b.Property(x => x.Comments)
                .HasColumnName(nameof(Grade.Comments))
                .IsRequired(false)
                .HasMaxLength(GradeConsts.MaxCommentsLength);

            b.Property(x => x.GradedAt)
                .HasColumnName(nameof(Grade.GradedAt))
                .IsRequired(false);

            // Foreign keys with no cascade delete
            b.HasOne<Student>()
                .WithMany()
                .HasForeignKey(x => x.StudentId)
                .OnDelete(DeleteBehavior.NoAction);

            b.HasOne<Course>()
                .WithMany()
                .HasForeignKey(x => x.CourseId)
                .OnDelete(DeleteBehavior.NoAction);

            // Indexes
            b.HasIndex(x => x.StudentId);
            b.HasIndex(x => x.CourseId);
            b.HasIndex(x => new { x.StudentId, x.CourseId });
        });

        // Attendance entity configuration
        builder.Entity<Attendance>(b =>
        {
            b.ToTable(abp_obs_projectConsts.DbTablePrefix + "Attendances", abp_obs_projectConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.StudentId)
                .HasColumnName(nameof(Attendance.StudentId))
                .IsRequired();

            b.Property(x => x.CourseId)
                .HasColumnName(nameof(Attendance.CourseId))
                .IsRequired();

            b.Property(x => x.AttendanceDate)
                .HasColumnName(nameof(Attendance.AttendanceDate))
                .IsRequired();

            b.Property(x => x.IsPresent)
                .HasColumnName(nameof(Attendance.IsPresent))
                .IsRequired();

            b.Property(x => x.Remarks)
                .HasColumnName(nameof(Attendance.Remarks))
                .IsRequired(false)
                .HasMaxLength(AttendanceConsts.MaxRemarksLength);

            // Foreign keys with no cascade delete
            b.HasOne<Student>()
                .WithMany()
                .HasForeignKey(x => x.StudentId)
                .OnDelete(DeleteBehavior.NoAction);

            b.HasOne<Course>()
                .WithMany()
                .HasForeignKey(x => x.CourseId)
                .OnDelete(DeleteBehavior.NoAction);

            // Indexes
            b.HasIndex(x => x.StudentId);
            b.HasIndex(x => x.CourseId);
            b.HasIndex(x => new { x.StudentId, x.CourseId, x.AttendanceDate });
        });

        // Enrollment entity configuration
        builder.Entity<Enrollment>(b =>
        {
            b.ToTable(abp_obs_projectConsts.DbTablePrefix + "Enrollments", abp_obs_projectConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.StudentId)
                .HasColumnName(nameof(Enrollment.StudentId))
                .IsRequired();

            b.Property(x => x.CourseId)
                .HasColumnName(nameof(Enrollment.CourseId))
                .IsRequired();

            b.Property(x => x.EnrolledAt)
                .HasColumnName(nameof(Enrollment.EnrolledAt))
                .IsRequired();

            b.Property(x => x.WithdrawnAt)
                .HasColumnName(nameof(Enrollment.WithdrawnAt))
                .IsRequired(false);

            b.Property(x => x.Status)
                .HasColumnName(nameof(Enrollment.Status))
                .IsRequired();

            // Foreign keys with no cascade delete
            b.HasOne<Student>()
                .WithMany()
                .HasForeignKey(x => x.StudentId)
                .OnDelete(DeleteBehavior.NoAction);

            b.HasOne<Course>()
                .WithMany()
                .HasForeignKey(x => x.CourseId)
                .OnDelete(DeleteBehavior.NoAction);

            // Indexes
            b.HasIndex(x => x.StudentId);
            b.HasIndex(x => x.CourseId);
            b.HasIndex(x => new { x.StudentId, x.CourseId });
            b.HasIndex(x => x.Status);
        });
    }
}
