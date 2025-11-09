using AutoMapper;
using abp_obs_project.Students;
using abp_obs_project.Teachers;
using abp_obs_project.Courses;
using abp_obs_project.Grades;
using abp_obs_project.Attendances;
using abp_obs_project.Enrollments;

namespace abp_obs_project;

public class abp_obs_projectApplicationAutoMapperProfile : Profile
{
    public abp_obs_projectApplicationAutoMapperProfile()
    {
        /* You can configure your AutoMapper mapping configuration here.
         * Alternatively, you can split your mapping configurations
         * into multiple profile classes for a better organization. */

        // Student mappings
        CreateMap<Student, StudentDto>()
            .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.PhoneNumber));
        CreateMap<CreateUpdateStudentDto, Student>()
            .ForMember(dest => dest.PhoneNumber, opt => opt.Ignore()); // Phone is set via SetPhoneNumber method

        // Teacher mappings
        CreateMap<Teacher, TeacherDto>();
        CreateMap<CreateUpdateTeacherDto, Teacher>();
        CreateMap<Teacher, TeacherLookupDto>();

        // Course mappings
        // Note: TeacherName is manually mapped in CourseAppService
        CreateMap<Course, CourseDto>()
            .ForMember(dest => dest.TeacherName, opt => opt.Ignore());
        CreateMap<CreateUpdateCourseDto, Course>();

        // Grade mappings
        // Note: StudentName and CourseName are manually mapped in GradeAppService
        CreateMap<Grade, GradeDto>()
            .ForMember(dest => dest.StudentName, opt => opt.Ignore())
            .ForMember(dest => dest.CourseName, opt => opt.Ignore());
        CreateMap<CreateUpdateGradeDto, Grade>();

        // Attendance mappings
        // Note: StudentName and CourseName are manually mapped in AttendanceAppService
        CreateMap<Attendance, AttendanceDto>()
            .ForMember(dest => dest.StudentName, opt => opt.Ignore())
            .ForMember(dest => dest.CourseName, opt => opt.Ignore());
        CreateMap<CreateUpdateAttendanceDto, Attendance>();

        // Enrollment mappings
        // Note: StudentName, StudentNumber, CourseName and CourseCode are manually mapped in EnrollmentAppService
        CreateMap<Enrollment, EnrollmentDto>()
            .ForMember(dest => dest.StudentName, opt => opt.Ignore())
            .ForMember(dest => dest.StudentNumber, opt => opt.Ignore())
            .ForMember(dest => dest.CourseName, opt => opt.Ignore())
            .ForMember(dest => dest.CourseCode, opt => opt.Ignore());
        CreateMap<CreateEnrollmentDto, Enrollment>();
    }
}
