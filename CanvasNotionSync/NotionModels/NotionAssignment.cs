namespace CanvasNotionSync.NotionModels;

public class NotionAssignment
{
    public string Id { get; set; }
    public string Name { get; set; }
    public int CanvasId { get; set; }
    public DateTime? Due { get; set; }
    public string Link { get; set; }
    public DateTime? Unlocked { get; set; }
    public decimal? Points { get; set; }
    public string Status { get; set; }
}