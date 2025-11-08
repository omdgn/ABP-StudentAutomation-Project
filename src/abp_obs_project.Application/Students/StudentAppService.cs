using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using abp_obs_project.Caching;
using abp_obs_project.Events;
using abp_obs_project.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.EventBus.Distributed;

namespace abp_obs_project.Students;

[Authorize(abp_obs_projectPermissions.Students.Default)]
public class StudentAppService : ApplicationService, IStudentAppService
{
    private readonly IStudentRepository _studentRepository;
    private readonly StudentManager _studentManager;
    private readonly IDistributedEventBus _distributedEventBus;
    private readonly IObsCacheService _cacheService;

    public StudentAppService(
        IStudentRepository studentRepository,
        StudentManager studentManager,
        IDistributedEventBus distributedEventBus,
        IObsCacheService cacheService)
    {
        _studentRepository = studentRepository;
        _studentManager = studentManager;
        _distributedEventBus = distributedEventBus;
        _cacheService = cacheService;
    }

    [Authorize(abp_obs_projectPermissions.Students.ViewAll)]
    public virtual async Task<PagedResultDto<StudentDto>> GetListAsync(GetStudentsInput input)
    {
        // Use cache only for simple list requests (no filters, no pagination)
        var isSimpleListRequest = string.IsNullOrWhiteSpace(input.FilterText) &&
                                  string.IsNullOrWhiteSpace(input.FirstName) &&
                                  string.IsNullOrWhiteSpace(input.LastName) &&
                                  string.IsNullOrWhiteSpace(input.Email) &&
                                  string.IsNullOrWhiteSpace(input.StudentNumber) &&
                                  input.Gender == null &&
                                  input.BirthDateMin == null &&
                                  input.BirthDateMax == null &&
                                  input.SkipCount == 0;

        if (isSimpleListRequest)
        {
            var cachedResult = await _cacheService.GetOrSetAsync(
                ObsCacheKeys.Students.List,
                async () =>
                {
                    var totalCount = await _studentRepository.GetCountAsync();
                    var items = await _studentRepository.GetListAsync(
                        sorting: input.Sorting,
                        maxResultCount: input.MaxResultCount
                    );

                    return new PagedResultDto<StudentDto>
                    {
                        TotalCount = totalCount,
                        Items = ObjectMapper.Map<List<Student>, List<StudentDto>>(items)
                    };
                }
            );

            return cachedResult!;
        }

        // For filtered/paginated requests, bypass cache
        var totalCount = await _studentRepository.GetCountAsync(
            input.FilterText,
            input.FirstName,
            input.LastName,
            input.Email,
            input.StudentNumber,
            input.Gender,
            input.BirthDateMin,
            input.BirthDateMax
        );

        var items = await _studentRepository.GetListAsync(
            input.FilterText,
            input.FirstName,
            input.LastName,
            input.Email,
            input.StudentNumber,
            input.Gender,
            input.BirthDateMin,
            input.BirthDateMax,
            input.Sorting,
            input.MaxResultCount,
            input.SkipCount
        );

        return new PagedResultDto<StudentDto>
        {
            TotalCount = totalCount,
            Items = ObjectMapper.Map<List<Student>, List<StudentDto>>(items)
        };
    }

    public virtual async Task<StudentDto> GetAsync(Guid id)
    {
        var student = await _studentRepository.GetAsync(id);
        return ObjectMapper.Map<Student, StudentDto>(student);
    }

    [Authorize(abp_obs_projectPermissions.Students.Create)]
    public virtual async Task<StudentDto> CreateAsync(CreateUpdateStudentDto input)
    {
        var student = await _studentManager.CreateAsync(
            input.FirstName,
            input.LastName,
            input.Email,
            input.StudentNumber,
            input.BirthDate,
            input.Gender,
            input.Phone
        );

        // Invalidate cache after creation
        await _cacheService.RemoveAsync(ObsCacheKeys.Students.List);

        // Publish distributed event
        await _distributedEventBus.PublishAsync(new StudentCreatedEto
        {
            Id = student.Id,
            FirstName = student.FirstName,
            LastName = student.LastName,
            Email = student.Email,
            StudentNumber = student.StudentNumber,
            CreationTime = Clock.Now
        });

        return ObjectMapper.Map<Student, StudentDto>(student);
    }

    [Authorize(abp_obs_projectPermissions.Students.Edit)]
    public virtual async Task<StudentDto> UpdateAsync(Guid id, CreateUpdateStudentDto input)
    {
        var student = await _studentRepository.GetAsync(id);

        await _studentManager.UpdateAsync(
            id,
            input.FirstName,
            input.LastName,
            input.Email,
            input.StudentNumber,
            input.BirthDate,
            input.Gender,
            input.Phone
        );

        // Invalidate cache after update
        await _cacheService.RemoveAsync(ObsCacheKeys.Students.List);

        // Publish distributed event
        await _distributedEventBus.PublishAsync(new StudentUpdatedEto
        {
            Id = student.Id,
            FirstName = student.FirstName,
            LastName = student.LastName,
            Email = student.Email,
            StudentNumber = student.StudentNumber,
            LastModificationTime = Clock.Now
        });

        return ObjectMapper.Map<Student, StudentDto>(student);
    }

    [Authorize(abp_obs_projectPermissions.Students.Delete)]
    public virtual async Task DeleteAsync(Guid id)
    {
        var student = await _studentRepository.GetAsync(id);

        await _studentRepository.DeleteAsync(id);

        // Invalidate cache after deletion
        await _cacheService.RemoveAsync(ObsCacheKeys.Students.List);

        // Publish distributed event
        await _distributedEventBus.PublishAsync(new StudentDeletedEto
        {
            Id = student.Id,
            StudentNumber = student.StudentNumber,
            DeletionTime = Clock.Now
        });
    }
}
