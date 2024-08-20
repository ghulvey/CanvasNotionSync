using CanvasNotionSync.NotionModels;
using Newtonsoft.Json;

namespace CanvasNotionSync.CanvasModels;

public class CanvasAssignment
{
    public int Id { get; set; }
    public string Name { get; set; }
    [JsonProperty("due_at")] public DateTime? DueAt { get; set; }
    [JsonProperty("html_url")] public string HtmlUrl { get; set; }
    [JsonProperty("unlock_at")] public DateTime? UnlockAt { get; set; }
    [JsonProperty("points_possible")] public decimal? Points { get; set; }
    public bool HasSubmittedSubmissions { get; set; }

    public bool Equals(NotionAssignment notionAssignment)
    {
        var dueAtUtc = notionAssignment.Due?.ToUniversalTime();
        
        if (Id != notionAssignment.CanvasId)
            return false;
        if(Name != notionAssignment.Name)
            return false;
        if(dueAtUtc?.ToString("yyyy-MM-dd HH:mm") != DueAt?.ToString("yyyy-MM-dd HH:mm"))
            return false;
        if(HtmlUrl != notionAssignment.Link)
            return false;
        if(UnlockAt != notionAssignment.Unlocked?.ToUniversalTime())
            return false;
        if(Points != notionAssignment.Points)
            return false;

        
        return true;
    }
}