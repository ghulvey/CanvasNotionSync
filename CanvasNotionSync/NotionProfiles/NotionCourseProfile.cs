using AutoMapper;
using CanvasNotionSync.NotionModels;
using Newtonsoft.Json.Linq;

namespace CanvasNotionSync.NotionProfiles;

public class NotionCourseProfile : Profile
{
    public NotionCourseProfile()
    {
        CreateMap<JToken, NotionCourse>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src["id"]))
            .ForMember(dest => dest.Name,
                opt => opt.MapFrom(src => src["properties"]["Name"]["title"][0]["text"]["content"]))
            .ForMember(dest => dest.Link, opt => opt.MapFrom(src => src["properties"]["Canvas Course"]["url"]));
    }
}