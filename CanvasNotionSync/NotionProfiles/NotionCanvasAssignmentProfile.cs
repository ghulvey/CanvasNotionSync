using AutoMapper;
using CanvasNotionSync.CanvasModels;
using CanvasNotionSync.NotionModels;

namespace CanvasNotionSync.NotionProfiles;

public class NotionCanvasAssignmentProfile : Profile
{
    public NotionCanvasAssignmentProfile()
    {
        CreateMap<NotionAssignment, CanvasAssignment>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.CanvasId))
            .ForMember(dest => dest.DueAt, opt => opt.MapFrom(src => src.Due))
            .ForMember(dest => dest.UnlockAt, opt => opt.MapFrom(src => src.Unlocked))
            .ForMember(dest => dest.HtmlUrl, opt => opt.MapFrom(src => src.Link))
            .ForMember(dest => dest.HasSubmittedSubmissions, opt => opt.MapFrom(src => src.Status == "Done"));
    }
}