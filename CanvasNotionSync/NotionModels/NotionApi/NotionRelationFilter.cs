namespace CanvasNotionSync.NotionModels.NotionApi;

public class NotionRelationFilter
{
    public NotionRelationFilterOptions Filter { get; set; }
}

public class NotionRelationFilterOptions
{
    public string Property { get; set; }
    public NotionRelationOptions Relation { get; set; }
}

public class NotionRelationOptions
{
    public string Contains { get; set; }
}