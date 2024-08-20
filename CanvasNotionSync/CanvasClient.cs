using System.Net;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using CanvasNotionSync.CanvasModels;
using Newtonsoft.Json;
using Serilog;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace CanvasNotionSync;

public class CanvasClient
{
    private readonly string _token;
    private readonly ILogger _logger;
    private readonly HttpClient _client;

    private List<CanvasAssignment> _result = new List<CanvasAssignment>();

    public CanvasClient(string token, ILogger logger)
    {
        _token = token;
        _logger = logger;
        _client = new HttpClient();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
    }

    private async Task MakeRequest(string url)
    {
        var res =  await _client.SendAsync(new HttpRequestMessage(HttpMethod.Get, url));
        if(res.StatusCode != HttpStatusCode.OK)
            return;
        _result.AddRange(JsonConvert.DeserializeObject<List<CanvasAssignment>>(await res.Content.ReadAsStringAsync()) ?? new ());
        var next = GetNextLinkFromHeaders(res.Headers);
        if (next is not null)
            await MakeRequest(next);

    }
    
    private static string? GetNextLinkFromHeaders(HttpHeaders headers)
    {
        if (headers.TryGetValues("Link", out var linkHeaderValues))
        {
            var updated = string.Join(',', linkHeaderValues);
            foreach (var link in updated.Split(','))
            {
                var relMatch = Regex.Match(link, "(?<=rel=\").+?(?=\")", RegexOptions.IgnoreCase);
                var linkMatch = Regex.Match(link, "(?<=<).+?(?=>)", RegexOptions.IgnoreCase);

                if (relMatch.Success && linkMatch.Success)
                    if(relMatch.Value == "next")
                        return linkMatch.Value;
            }
        }
        return null;
    }

    public async Task<List<CanvasAssignment>?> GetCourseAssignments(string courseUrl)
    {
        string url = $"{courseUrl.Split("/courses/")[0]}/api/v1/courses/{courseUrl.Split("/courses/")[1]}/assignments";
        
        await MakeRequest(url);

        return _result;
    }
    
    
}