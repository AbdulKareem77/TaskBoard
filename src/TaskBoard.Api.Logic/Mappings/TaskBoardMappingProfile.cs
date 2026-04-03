using AutoMapper;
using TaskBoard.Api.Logic.Models;
using TaskBoard.Domain.Entities;

namespace TaskBoard.Api.Logic.Mappings;

public class TaskBoardMappingProfile : Profile
{
    public TaskBoardMappingProfile()
    {
        // User -> UserDto
        CreateMap<User, UserDto>()
            .ForCtorParam("Roles", opt => opt.MapFrom(src => src.Roles.Select(r => r.Name)));

        // Project -> ProjectDto
        CreateMap<Project, ProjectDto>()
            .ForMember(dest => dest.OwnerName, opt => opt.MapFrom(src => src.OwnerName ?? string.Empty))
            .ForMember(dest => dest.MemberCount, opt => opt.MapFrom(src => src.MemberCount))
            .ForMember(dest => dest.TaskCount, opt => opt.MapFrom(src => src.TaskCount));

        // ProjectMember -> ProjectMemberDto
        CreateMap<ProjectMember, ProjectMemberDto>()
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role));

        // TaskAssignment -> AssigneeDto
        CreateMap<TaskAssignment, AssigneeDto>()
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName));

        // TaskHistory -> TaskHistoryDto
        CreateMap<TaskHistory, TaskHistoryDto>()
            .ForMember(dest => dest.Date, opt => opt.MapFrom(src => src.DateCreated))
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName));

        // TaskItem -> TaskItemDto
        CreateMap<TaskItem, TaskItemDto>()
            .ForMember(dest => dest.Assignees, opt => opt.MapFrom(src => src.Assignees));

        // TaskItem -> TaskItemDetailDto
        CreateMap<TaskItem, TaskItemDetailDto>()
            .ForMember(dest => dest.Assignees, opt => opt.MapFrom(src => src.Assignees))
            .ForMember(dest => dest.History, opt => opt.MapFrom(src => src.History))
            .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => new CreatedByDto(
                src.CreatedByUserId,
                src.CreatedByFullName ?? string.Empty)));
    }
}
