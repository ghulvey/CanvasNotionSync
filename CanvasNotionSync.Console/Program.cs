using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace CanvasNotionSync.Console;

class Program
{
    static async Task Main(string[] args)
    {
        // Logging
        // create a logger
        using var logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.Debug()
            .MinimumLevel.Debug()
            .CreateLogger();
        
        
        // Configuaration
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", true, true)
            .AddJsonFile("appsettings.Development.json", true, true)
            .AddEnvironmentVariables()
            .AddUserSecrets(Assembly.GetExecutingAssembly(), true)
            .Build();
        
        // Dependency Injection
        var serviceProvider = new ServiceCollection()
            .AddSingleton<IConfiguration>(configuration)
            .AddTransient<SyncService>()
            .AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies())
            .AddSingleton<ILogger>(logger)
            .BuildServiceProvider();

        var syncService = serviceProvider.GetService<SyncService>();
        await syncService?.Sync();

    }
}