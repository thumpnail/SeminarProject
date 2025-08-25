using System.Diagnostics;

using Chat.Common;
using Chat.Common.Contracts;
using Chat.Common.Models;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container (optional)
builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

var app = builder.Build();

// Optional: Swagger UI
if (app.Environment.IsDevelopment()) {
    //app.UseSwagger();
    //app.UseSwaggerUI();
}

LocalDatabase database = new("../../../../../chat-monolith.db");

// Define a simple endpoint
app.MapGet("/", () => "Type=ChatMessagingService");

app.MapPost("/send", ([FromBody] MessageSendContract messageSendContract) => {
    Thread.Sleep(10);
    var start = Stopwatch.StartNew();
    database.InsertMessage(messageSendContract);
    start.Stop();

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
    Thread.Sleep(10);
    var start = Stopwatch.StartNew();
    var response = database.GetMessages(historyRetrieveContract);
    start.Stop();

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
    Thread.Sleep(10);
    var start = Stopwatch.StartNew();
    var response = database.GetRoom(roomRetrieveContract);
    start.Stop();

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