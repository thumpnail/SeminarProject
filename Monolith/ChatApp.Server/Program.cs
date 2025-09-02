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
FlatMockDatabase database = new("../../../../../chat-monolith.db");

// Define a simple endpoint
app.MapGet("/", () => "Type=ChatMessagingService");

app.MapPost("/send", ([FromBody] MessageSendContract messageSendContract) => {
    var start = Stopwatch.StartNew();
    database.InsertMessage(messageSendContract);
    start.Stop();

    logger.Log("/send", $"Insert took {start.ElapsedMilliseconds} ms|{start.Elapsed.Nanoseconds} ns");
    //appLogger.LogInformation(new EventId(1, "MessageSent"), $"/send Insert took {start.ElapsedMilliseconds} ms");

    // benchmarking
    var subTag = new BenchmarkSubTag(
        "Monolith/send",
        start.ElapsedMilliseconds,
        GC.GetAllocatedBytesForCurrentThread(),
        GC.GetTotalAllocatedBytes()
    );
    var newTag = new BenchmarkTag();
    newTag.SubTags.Add(subTag);

    return Results.Json(new MessageSendResponseContract(
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