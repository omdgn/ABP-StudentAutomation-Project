using System;
using System.Threading.Tasks;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Identity;
using Volo.Abp.PermissionManagement;
using Volo.Abp.Uow;

namespace abp_obs_project.Data;

public class abp_obs_projectDataSeederContributor : IDataSeedContributor, ITransientDependency
{
    private readonly IIdentityRoleRepository _roleRepository;
    private readonly IPermissionManager _permissionManager;
    private readonly IUnitOfWorkManager _unitOfWorkManager;

    public abp_obs_projectDataSeederContributor(
        IIdentityRoleRepository roleRepository,
        IPermissionManager permissionManager,
        IUnitOfWorkManager unitOfWorkManager)
    {
        _roleRepository = roleRepository;
        _permissionManager = permissionManager;
        _unitOfWorkManager = unitOfWorkManager;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        await SeedRolesAndPermissionsAsync();
    }

    private async Task SeedRolesAndPermissionsAsync()
    {
        using var uow = _unitOfWorkManager.Begin(requiresNew: true, isTransactional: true);

        // Admin Role (custom "Admin")
        await CreateRoleWithPermissionsAsync(
            "Admin",
            new[]
            {
                // Students
                "abp_obs_project.Students",
                "abp_obs_project.Students.ViewAll",
                "abp_obs_project.Students.Create",
                "abp_obs_project.Students.Edit",
                "abp_obs_project.Students.Delete",

                // Teachers
                "abp_obs_project.Teachers",
                "abp_obs_project.Teachers.ViewAll",
                "abp_obs_project.Teachers.Create",
                "abp_obs_project.Teachers.Edit",
                "abp_obs_project.Teachers.Delete",

                // Courses
                "abp_obs_project.Courses",
                "abp_obs_project.Courses.ViewAll",
                "abp_obs_project.Courses.Create",
                "abp_obs_project.Courses.Edit",
                "abp_obs_project.Courses.Delete",

                // Grades
                "abp_obs_project.Grades",
                "abp_obs_project.Grades.ViewAll",
                "abp_obs_project.Grades.Create",
                "abp_obs_project.Grades.Edit",
                "abp_obs_project.Grades.Delete",

                // Attendances
                "abp_obs_project.Attendances",
                "abp_obs_project.Attendances.ViewAll",
                "abp_obs_project.Attendances.Create",
                "abp_obs_project.Attendances.Edit",
                "abp_obs_project.Attendances.Delete",

                // Enrollments
                "abp_obs_project.Enrollments",
                "abp_obs_project.Enrollments.ViewAll",
                "abp_obs_project.Enrollments.Create",
                "abp_obs_project.Enrollments.Edit",
                "abp_obs_project.Enrollments.Delete"
            });

        // Also grant the same permissions to the built-in 'admin' role if it exists
        await GrantPermissionsToExistingRoleAsync(
            "admin",
            new[]
            {
                // Students
                "abp_obs_project.Students",
                "abp_obs_project.Students.ViewAll",
                "abp_obs_project.Students.Create",
                "abp_obs_project.Students.Edit",
                "abp_obs_project.Students.Delete",

                // Teachers
                "abp_obs_project.Teachers",
                "abp_obs_project.Teachers.ViewAll",
                "abp_obs_project.Teachers.Create",
                "abp_obs_project.Teachers.Edit",
                "abp_obs_project.Teachers.Delete",

                // Courses
                "abp_obs_project.Courses",
                "abp_obs_project.Courses.ViewAll",
                "abp_obs_project.Courses.Create",
                "abp_obs_project.Courses.Edit",
                "abp_obs_project.Courses.Delete",

                // Grades
                "abp_obs_project.Grades",
                "abp_obs_project.Grades.ViewAll",
                "abp_obs_project.Grades.Create",
                "abp_obs_project.Grades.Edit",
                "abp_obs_project.Grades.Delete",

                // Attendances
                "abp_obs_project.Attendances",
                "abp_obs_project.Attendances.ViewAll",
                "abp_obs_project.Attendances.Create",
                "abp_obs_project.Attendances.Edit",
                "abp_obs_project.Attendances.Delete",

                // Enrollments
                "abp_obs_project.Enrollments",
                "abp_obs_project.Enrollments.ViewAll",
                "abp_obs_project.Enrollments.Create",
                "abp_obs_project.Enrollments.Edit",
                "abp_obs_project.Enrollments.Delete"
            });

        // Teacher Role
        await CreateRoleWithPermissionsAsync(
            "Teacher",
            new[]
            {
                // Students - Teacher can create and edit students
                "abp_obs_project.Students",
                "abp_obs_project.Students.ViewAll",
                "abp_obs_project.Students.Create",
                "abp_obs_project.Students.Edit",

                // Courses - Görüntüleme ve düzenleme (ViewAll YOK - sadece kendi derslerini görsün)
                "abp_obs_project.Courses",
                "abp_obs_project.Courses.Edit",

                // Grades - Tam yetki
                "abp_obs_project.Grades",
                "abp_obs_project.Grades.ViewAll",
                "abp_obs_project.Grades.Create",
                "abp_obs_project.Grades.Edit",
                "abp_obs_project.Grades.Delete",

                // Attendances - Tam yetki
                "abp_obs_project.Attendances",
                "abp_obs_project.Attendances.ViewAll",
                "abp_obs_project.Attendances.Create",
                "abp_obs_project.Attendances.Edit",
                "abp_obs_project.Attendances.Delete",

                // Enrollments - Tam yetki (öğrenci kaydı yönetimi için)
                "abp_obs_project.Enrollments",
                "abp_obs_project.Enrollments.ViewAll",
                "abp_obs_project.Enrollments.Create",
                "abp_obs_project.Enrollments.Edit",
                "abp_obs_project.Enrollments.Delete"
            });

        // Student Role
        await CreateRoleWithPermissionsAsync(
            "Student",
            new[]
            {
                // Students - Sadece kendi bilgilerini görüntüleme
                "abp_obs_project.Students",

                // Courses - Sadece kayıtlı olduğu dersleri görüntüleme (ViewAll YOK)
                "abp_obs_project.Courses",

                // Grades - Sadece kendi notlarını görüntüleme
                "abp_obs_project.Grades",

                // Attendances - Sadece kendi devamsızlıklarını görüntüleme
                "abp_obs_project.Attendances"
            });

        await uow.CompleteAsync();
    }

    private async Task CreateRoleWithPermissionsAsync(string roleName, string[] permissions)
    {
        // Rolün zaten var olup olmadığını kontrol et
        var existingRole = await _roleRepository.FindByNormalizedNameAsync(roleName.ToUpperInvariant());

        if (existingRole == null)
        {
            // Rol yoksa oluştur
            var role = new IdentityRole(
                Guid.NewGuid(),
                roleName,
                tenantId: null
            )
            {
                IsDefault = false,
                IsPublic = true
            };

            await _roleRepository.InsertAsync(role, autoSave: true);
            existingRole = role;
        }

        // Permission'ları ata
        foreach (var permission in permissions)
        {
            await _permissionManager.SetAsync(
                permission,
                RolePermissionValueProvider.ProviderName,
                existingRole.Name,
                true
            );
        }
    }

    private async Task GrantPermissionsToExistingRoleAsync(string roleName, string[] permissions)
    {
        var existingRole = await _roleRepository.FindByNormalizedNameAsync(roleName.ToUpperInvariant());
        if (existingRole == null)
        {
            return;
        }

        foreach (var permission in permissions)
        {
            await _permissionManager.SetAsync(
                permission,
                RolePermissionValueProvider.ProviderName,
                existingRole.Name,
                true
            );
        }
    }
}
