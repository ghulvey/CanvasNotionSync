using System.Diagnostics;
using AutoMapper;
using CanvasNotionSync.CanvasModels;
using CanvasNotionSync.NotionModels;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace CanvasNotionSync;

public class SyncService
{
    private readonly IConfiguration _configuration;
    private readonly IMapper _mapper;
    private readonly ILogger _logger;
    private readonly CanvasClient _canvasClient;
    private readonly NotionClient _notionClient;

    public SyncService(IConfiguration configuration, IMapper mapper, ILogger logger)
    {
        _configuration = configuration;
        _mapper = mapper;
        _logger = logger;
        
        string canvasToken = _configuration["CanvasApiToken"] ?? "";
        string notionToken = _configuration["NotionApiToken"] ?? "";
        string notionCourseDatabaseId = _configuration["NotionCourseDatabaseId"] ?? "";
        string notionAssignmentDatabaseId = _configuration["NotionAssignmentDatabaseId"] ?? "";
        
        if(canvasToken == "")
            _logger.Error("Canvas Token is missing");
        if(notionToken == "")
            _logger.Error("Notion Token is missing");
        if(notionCourseDatabaseId == "")
            _logger.Error("Notion Course Database Id is missing");
        if(notionAssignmentDatabaseId == "")
            _logger.Error("Notion Assignment Database Id is missing");
        
        if(canvasToken == "" || notionToken == "" || notionCourseDatabaseId == "" || notionAssignmentDatabaseId == "")
            throw new Exception("Missing configuration");
        
        _canvasClient = new CanvasClient(canvasToken, _logger);
        _notionClient = new NotionClient(_mapper, _logger, notionToken, notionCourseDatabaseId, notionAssignmentDatabaseId);
    }

    public async Task Sync()
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        
        _logger.Information("Syncing Canvas Assignments to Notion");
        var courses = await _notionClient.GetCourses();
        _logger.Information("Found {CourseCount} courses", courses.Count);
        foreach (var course in courses)
        {
            _logger.Information("Syncing {CourseName}", course.Name);
            
            var canvasAssignments = new List<CanvasAssignment>();
            canvasAssignments= await _canvasClient.GetCourseAssignments(course.Link);
            var notionAssignments = new List<NotionAssignment>();
            notionAssignments = await _notionClient.GetAssignments(course.Id);

            foreach (var canvasAssignment in canvasAssignments)
            {
                var notionAssignment = notionAssignments.FirstOrDefault(x => x.CanvasId == canvasAssignment.Id);
                if(notionAssignment is null)
                {
                    _logger.Information("Creating {CourseName}:{Assignment}", course.Name, canvasAssignment.Name);
                    await _notionClient.CreateAssignment(course.Id, canvasAssignment);
                }
                else
                {
                    if (!canvasAssignment.Equals(notionAssignment))
                    {
                        _logger.Information("Updating '{CourseName}' : '{Assignment}'", course.Name, canvasAssignment.Name);
                        await _notionClient.UpdateAssignment(course.Id, notionAssignment, canvasAssignment);
                    }
                        
                }
            }
            
            canvasAssignments.Clear();
            notionAssignments.Clear();
        }
        
        stopwatch.Stop();
        _logger.Information("Sync completed in {Time}", stopwatch.Elapsed);
    }
}