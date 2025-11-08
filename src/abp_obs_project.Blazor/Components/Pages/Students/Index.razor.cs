using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using abp_obs_project.Blazor.Services.Abstractions;
using abp_obs_project.Permissions;
using abp_obs_project.Students;
using abp_obs_project.Teachers;
using Blazorise;
using Blazorise.DataGrid;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Volo.Abp.Application.Dtos;

namespace abp_obs_project.Blazor.Components.Pages.Students;

public partial class Index : abp_obs_projectComponentBase
{
    // ============================================
    // DEPENDENCY INJECTION
    // ============================================

    [Inject]
    private IStudentUIService StudentUIService { get; set; } = default!;

    [Inject]
    private ITeacherUIService TeacherUIService { get; set; } = default!;

    // ============================================
    // DATA PROPERTIES
    // ============================================

    private IReadOnlyList<StudentDto> StudentList { get; set; } = new List<StudentDto>();
    private int TotalCount { get; set; }
    private int PageSize { get; } = 10;
    private int CurrentPage { get; set; } = 1;

    // ============================================
    // MODAL & FORM
    // ============================================

    private Modal StudentModal { get; set; } = default!;
    private Guid? EditingStudentId { get; set; }
    private CreateUpdateStudentDto NewStudent { get; set; } = new();
    private Validations? StudentValidationsRef;

    // ============================================
    // TEACHER LOOKUP
    // ============================================

    private List<TeacherLookupDto> TeacherList { get; set; } = new();

    // ============================================
    // SEARCH & FILTER
    // ============================================

    private string SearchText { get; set; } = string.Empty;

    // ============================================
    // PERMISSIONS
    // ============================================

    private bool CanCreateStudent { get; set; }
    private bool CanEditStudent { get; set; }
    private bool CanDeleteStudent { get; set; }

    // ============================================
    // LIFECYCLE
    // ============================================

    protected override async Task OnInitializedAsync()
    {
        await SetPermissionsAsync();
        await GetStudentsAsync();
        await GetTeacherLookupAsync();
    }

    // ============================================
    // PERMISSION CHECKS
    // ============================================

    private async Task SetPermissionsAsync()
    {
        CanCreateStudent = await AuthorizationService
            .IsGrantedAsync(abp_obs_projectPermissions.Students.Create);

        CanEditStudent = await AuthorizationService
            .IsGrantedAsync(abp_obs_projectPermissions.Students.Edit);

        CanDeleteStudent = await AuthorizationService
            .IsGrantedAsync(abp_obs_projectPermissions.Students.Delete);
    }

    // ============================================
    // DATA LOADING
    // ============================================

    private async Task GetStudentsAsync()
    {
        var input = new GetStudentsInput
        {
            MaxResultCount = PageSize,
            SkipCount = (CurrentPage - 1) * PageSize,
            FilterText = SearchText
        };

        var result = await StudentUIService.GetListAsync(input);
        StudentList = result.Items;
        TotalCount = (int)result.TotalCount;
    }

    private async Task GetTeacherLookupAsync()
    {
        TeacherList = await TeacherUIService.GetTeacherLookupAsync();
    }

    // ============================================
    // SEARCH
    // ============================================

    private async Task OnSearchChanged(string searchText)
    {
        SearchText = searchText;
        CurrentPage = 1;
        await GetStudentsAsync();
    }

    // ============================================
    // MODAL OPERATIONS
    // ============================================

    private async Task OpenCreateModalAsync()
    {
        EditingStudentId = null;
        NewStudent = new CreateUpdateStudentDto
        {
            StudentNumber = $"STU{DateTime.Now:yyyyMMddHHmmss}",
            Gender = EnumGender.Male,
            BirthDate = DateTime.Now.AddYears(-18),
            EnrollmentDate = DateTime.Now
        };

        if (StudentValidationsRef != null)
        {
            await StudentValidationsRef.ClearAll();
        }

        await StudentModal.Show();
    }

    private async Task OpenEditModalAsync(StudentDto student)
    {
        EditingStudentId = student.Id;

        var studentDto = await StudentUIService.GetAsync(student.Id);

        NewStudent = new CreateUpdateStudentDto
        {
            FirstName = studentDto.FirstName,
            LastName = studentDto.LastName,
            Email = studentDto.Email,
            StudentNumber = studentDto.StudentNumber,
            Gender = studentDto.Gender,
            Phone = studentDto.Phone,
            BirthDate = studentDto.BirthDate,
            EnrollmentDate = studentDto.EnrollmentDate,
            TeacherId = studentDto.TeacherId,
            Address = studentDto.Address
        };

        if (StudentValidationsRef != null)
        {
            await StudentValidationsRef.ClearAll();
        }

        await StudentModal.Show();
    }

    private async Task CloseStudentModalAsync()
    {
        await StudentModal.Hide();
    }

    // ============================================
    // CREATE / UPDATE
    // ============================================

    private async Task CreateOrUpdateStudentAsync()
    {
        try
        {
            if (StudentValidationsRef != null && !await StudentValidationsRef.ValidateAll())
            {
                return;
            }

            if (EditingStudentId == null)
            {
                await StudentUIService.CreateAsync(NewStudent);
                await Message.Success(L["SuccessfullyCreated"]);
            }
            else
            {
                await StudentUIService.UpdateAsync(EditingStudentId.Value, NewStudent);
                await Message.Success(L["SuccessfullyUpdated"]);
            }

            await CloseStudentModalAsync();
            await GetStudentsAsync();
        }
        catch (Exception ex)
        {
            await HandleErrorAsync(ex);
        }
    }

    // ============================================
    // DELETE
    // ============================================

    private async Task DeleteStudentAsync(StudentDto student)
    {
        try
        {
            var confirmed = await Message.Confirm(
                L["StudentDeletionConfirmationMessage", student.FirstName, student.LastName],
                L["AreYouSure"]
            );

            if (confirmed)
            {
                await StudentUIService.DeleteAsync(student.Id);
                await Message.Success(L["SuccessfullyDeleted"]);
                await GetStudentsAsync();
            }
        }
        catch (Exception ex)
        {
            await HandleErrorAsync(ex);
        }
    }

    // ============================================
    // PAGINATION
    // ============================================

    private async Task OnPageChanged(int page)
    {
        CurrentPage = page;
        await GetStudentsAsync();
    }
}
