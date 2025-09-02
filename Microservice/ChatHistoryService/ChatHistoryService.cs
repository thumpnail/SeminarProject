using System.Diagnostics;
using System.Net.Http.Json;

using Chat.Common;
using Chat.Common.Contracts;
using Chat.Common.Models;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var dbClient = new HttpClient {
    BaseAddress = new Uri(Chat.Common.Addresses.CHAT_DB_SERVICE)
};

var builder = WebApplication.CreateBuilder(args);

// Services registrieren
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// Swagger aktivieren
if (app.Environment.IsDevelopment()) { }

ILogger<Program> appLogger = app.Services.GetRequiredService<ILogger<Program>>();
Logger logger = new Logger("ChatHistoryService");

// Beispiel-Endpunkt
app.MapGet("/", () => "Type=ChatHistoryService");
app.MapPost("/history", async ([FromBody] HistoryRetrieveContract historyContract) => {
    var start = Stopwatch.StartNew();

    var response = await dbClient.PostAsJsonAsync("/getMessages",
        new HistoryRetrieveContract(historyContract.RoomId, historyContract.StartDate, historyContract.Limit));

    var historyResponse = await response.Content.ReadFromJsonAsync<HistoryResponseContract>();

    start.Stop();

    logger.Log("/history", $"Post '/getMessages' {start.ElapsedMilliseconds} ms|{start.Elapsed.Microseconds} ns");


    var subTag = new BenchmarkSubTag(
        "Microservice/History/history",
        start.ElapsedMilliseconds,
        GC.GetAllocatedBytesForCurrentThread(),
        GC.GetTotalAllocatedBytes()
    );
    var newTag = historyResponse.Tag;
    newTag.SubTags.Add(subTag);

    return Results.Json(historyResponse with {
        Tag = newTag
    });
});

app.Run(Chat.Common.Addresses.CHAT_HISTORY_SERVICE);