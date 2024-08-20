using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using AutoMapper;
using CanvasNotionSync.CanvasModels;
using CanvasNotionSync.NotionModels;
using CanvasNotionSync.NotionModels.NotionApi;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;

namespace CanvasNotionSync;

public class NotionClient
{
    private readonly IMapper _mapper;
    private readonly ILogger _logger;
    private readonly string _token;
    private readonly string _courseDatabase;
    private readonly string _assignmentDatabase;
    private readonly HttpClient _client;

    public NotionClient(IMapper mapper, ILogger logger, string token, string courseDatabase, string assignmentDatabase)
    {
        _mapper = mapper;
        _token = token;
        _courseDatabase = courseDatabase;
        _assignmentDatabase = assignmentDatabase;
        _client = new HttpClient();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        _client.DefaultRequestHeaders.Add("Notion-Version", "2022-06-28");
    }

    public async Task<List<NotionCourse>> GetCourses()
    {
        string url = $"https://api.notion.com/v1/databases/{_courseDatabase}/query";

        var res = await _client.PostAsJsonAsync(url, new NotionCheckboxFilter()
        {
            Filter = new NotionCheckboxFilterOptions()
            {
                Property = "Canvas Sync",
                Checkbox = new NotionCheckBoxOptions()
                {
                    Equals = true
                }
            }
        });

        var json = JObject.Parse(await res.Content.ReadAsStringAsync());
        return _mapper.Map<List<NotionCourse>>((JArray)json["results"]);
    }

    public async Task<List<NotionAssignment>> GetAssignments(string courseId)
    {
        string url = $"https://api.notion.com/v1/databases/{_assignmentDatabase}/query";

        var res = await _client.PostAsJsonAsync(url, new NotionRelationFilter()
        {
            Filter = new NotionRelationFilterOptions()
            {
                Property = "Class",
                Relation = new NotionRelationOptions()
                {
                    Contains = courseId
                }
            }
        });

        var json = JObject.Parse(await res.Content.ReadAsStringAsync());
        return _mapper.Map<List<NotionAssignment>>((JArray)json["results"]);
    }

    public async Task UpdateAssignment(string course, NotionAssignment notionAssignment,
        CanvasAssignment canvasAssignment)
    {
        string url = $"https://api.notion.com/v1/pages/{notionAssignment.Id}";

        JObject json = CanvasAssignmentToJson(course, canvasAssignment);
        
        var res = await _client.PatchAsync(url, new StringContent(JsonConvert.SerializeObject(json), Encoding.UTF8, "application/json"));
        if(res.StatusCode != HttpStatusCode.OK)
            _logger.Error("Failed to send the following request: {Request}", JsonConvert.SerializeObject(json, Formatting.None));
    }
    
    public async Task CreateAssignment(string course, CanvasAssignment canvasAssignment)
    {
        string url = $"https://api.notion.com/v1/pages";

        JObject json = CanvasAssignmentToJson(course, canvasAssignment);
        
        var res = await _client.PostAsync(url, new StringContent(JsonConvert.SerializeObject(json), Encoding.UTF8, "application/json"));
        if(res.StatusCode != HttpStatusCode.OK)
            _logger.Error("Failed to send the following request: {Request}", JsonConvert.SerializeObject(json, Formatting.None));
    }

    private JObject CanvasAssignmentToJson(string course, CanvasAssignment canvasAssignment)
    {
        JObject json = new JObject()
        {
            ["parent"] = new JObject() { ["database_id"] = _assignmentDatabase },
            ["properties"] = new JObject()
            {
                ["Class"] = new JObject() { 
                    ["relation"] = new JArray()
                    {
                        new JObject()
                        {
                            ["id"] = course
                        }
                    }
                },
                ["Name"] = new JObject()
                { 
                    ["title"] = new JArray()
                    {
                        new JObject() { 
                            ["text"] = new JObject()
                            {
                                ["content"] = canvasAssignment.Name
                            }
                        } 
                    }
                },
                ["Canvas ID"] = new JObject()
                {
                    ["number"] = canvasAssignment.Id
                },
                ["URL"] = new JObject()
                {
                    ["url"] = canvasAssignment.HtmlUrl
                },
                ["Possible Points"] = new JObject()
                {
                    ["number"] = canvasAssignment.Points
                },
            }
        };
        
        if(canvasAssignment.DueAt is not null)
            json["properties"]["Date"] = new JObject()
            {
                ["date"] = new JObject()
                {
                    ["start"] = canvasAssignment.DueAt.Value.ToString("yyyy-MM-dd HH:mm:ss")
                }
            };
        
        if(canvasAssignment.UnlockAt is not null)
            json["properties"]["Unlock Date"] = new JObject()
            {
                ["date"] = new JObject()
                {
                    ["start"] = canvasAssignment.UnlockAt.Value.ToString("yyyy-MM-dd HH:mm:ss") 
                }
            };
        
        return json;
    }
}