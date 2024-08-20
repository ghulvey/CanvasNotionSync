using AutoMapper;
using CanvasNotionSync.NotionModels;
using Newtonsoft.Json.Linq;

namespace CanvasNotionSync.NotionProfiles;

public class DateTimeTypeConverter : ITypeConverter<JToken, DateTime?>
{
    public DateTime? Convert(JToken source, DateTime? destination, ResolutionContext context)
    {
        if(source["start"] is not null)
            return System.Convert.ToDateTime(source["start"].ToString());
        return null;
    }
}

public class NotionAssignmentProfile : Profile
{
    
    public NotionAssignmentProfile()
    {
        
        CreateMap<JToken, NotionAssignment>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src["id"]))
            .ForMember(dest => dest.Name,
                opt => opt.MapFrom(src => src["properties"]["Name"]["title"][0]["text"]["content"]))
            .ForMember(dest => dest.CanvasId, opt => opt.MapFrom(src => src["properties"]["Canvas ID"]["number"]))
            .ForMember(dest => dest.Link, opt => opt.MapFrom(src => src["properties"]["URL"]["url"]))
            .AfterMap((src, dest) =>
            {
                if (src["properties"]["Date"]["date"].HasValues)
                    dest.Due = DateTime.Parse(src["properties"]["Date"]["date"]["start"].ToString());
                if (src["properties"]["Unlock Date"]["date"].HasValues)
                    dest.Unlocked = DateTime.Parse(src["properties"]["Unlock Date"]["date"]["start"].ToString());
                if (src["properties"]["Possible Points"]["number"] is not null)
                {
                    decimal? parse = decimal.TryParse(src["properties"]["Possible Points"]["number"].ToString(), out decimal result) ? result : null;
                    dest.Points = parse;
                }
            });
    }
}