namespace CanvasNotionSync.NotionModels.NotionApi;

public class NotionCheckboxFilter
{
    public NotionCheckboxFilterOptions Filter { get; set; }
}

public class NotionCheckboxFilterOptions
{
    public string Property { get; set; }
    public NotionCheckBoxOptions Checkbox { get; set; }
}

public class NotionCheckBoxOptions
{
    public bool Equals { get; set; }
}