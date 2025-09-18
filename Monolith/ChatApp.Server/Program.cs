using System.Diagnostics;

using Chat.Common;
using Chat.Common.Contracts;
using Chat.Common.Models;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;


var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Add services to the container (optional)
builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

var app = builder.Build();

// Optional: Swagger UI
if (app.Environment.IsDevelopment()) {
    //app.UseSwagger();
    //app.UseSwaggerUI();
}
ILogger<Program> appLogger = app.Services.GetRequiredService<ILogger<Program>>();
Logger logger = new("ChatMonolith");
IDatabase database;
#if DEBUG
database = new FlatMockDatabase("../../../../../chat-monolith.db");
#elif RELEASE
if (args.Length > 0) {
    switch (args[0]) {
        case "lite":
            database = new LiteBasedDatabase("./chat-monolith.db");
            break;
        case "memory":
            database = new LocalDatabase();
            break;
        case "mock":
            database = new FlatMockDatabase();
            break;
        case "help":
        case "--help":
            Console.WriteLine("Usage: ChatApp.Server [lite|memory|mock]");
            return;
        default:
            appLogger.LogWarning("No database type specified, defaulting to 'mock'. Use 'help' for options.\n");
            database = new FlatMockDatabase();
            break;
    }
} else {
    appLogger.LogWarning("No database type specified, defaulting to 'mock'. Use 'help' for options.\n");
    database = new FlatMockDatabase();
}
#endif
// Define a simple endpoint
app.MapGet("/", () => $"Type=ChatMonolithServer;DBType={database.GetType().Name}");

app.MapPost("/send", ([FromBody] MessageSendContract messageSendContract) => {
    var start = Stopwatch.StartNew();
    database.InsertMessage(messageSendContract);
    start.Stop();

    logger.Log("/send", $"Insert took {start.ElapsedMilliseconds} ms|{start.Elapsed.Nanoseconds} ns");
    //appLogger.LogInformation(new EventId(1, "MessageSent"), $"/send Insert took {start.ElapsedMilliseconds} ms");

    // benchmarking
    var subTag = new BenchmarkSubTag(
        "ChatMonolithServer",
        "Monolith/send",
        start.ElapsedMilliseconds,
        GC.GetAllocatedBytesForCurrentThread(),
        GC.GetTotalAllocatedBytes()
    );
    var newTag = new BenchmarkTag();
    newTag.SubTags.Add(subTag);

    return Results.Json(new MessageSendResponseContract(
        messageSendContract.runIndexIdentifier,
        messageSendContract.Content,
        true,
        newTag));
});

app.MapPost("/history", ([FromBody] HistoryRetrieveContract historyRetrieveContract) => {
    var start = Stopwatch.StartNew();
    var response = database.GetMessages(historyRetrieveContract);
    start.Stop();

    logger.Log("/history", $"GetMessages took {start.ElapsedMilliseconds} ms|{start.Elapsed.Microseconds} ns");
    //appLogger.LogInformation(new EventId(2, "HistoryRetrieved"), $"/history GetMessages took {start.ElapsedMilliseconds} ms");

    // benchmarking
    var subTag = new BenchmarkSubTag(
        "ChatMonolithServer",
        "Monolith/history",
        start.ElapsedMilliseconds,
        GC.GetAllocatedBytesForCurrentThread(),
        GC.GetTotalAllocatedBytes()
    );
    var newTag = new BenchmarkTag();
    newTag.SubTags.Add(subTag);

    return Results.Json(response with {
        Tag = newTag,
    });
});

app.MapPost("/room", ([FromBody] RoomRetrieveContract roomRetrieveContract) => {
    var start = Stopwatch.StartNew();
    var response = database.GetRoom(roomRetrieveContract);
    start.Stop();

    logger.Log("/room", $"GetRoom took {start.ElapsedMilliseconds} ms|{start.Elapsed.Microseconds} ns");
    //appLogger.LogInformation(new EventId(3, "RoomRetrieved"), $"/room GetRoom took {start.ElapsedMilliseconds} ms");

    var subTag = new BenchmarkSubTag(
        "ChatMonolithServer",
        "Monolith/room",
        start.ElapsedMilliseconds,
        GC.GetAllocatedBytesForCurrentThread(),
        GC.GetTotalAllocatedBytes()
    );
    var newTag = new BenchmarkTag();
    newTag.SubTags.Add(subTag);

    return Results.Json(response with {
        Tag = newTag
    });
});

// Run the web server
app.Run(Addresses.CHAT_MONOLITH_SERVICE);