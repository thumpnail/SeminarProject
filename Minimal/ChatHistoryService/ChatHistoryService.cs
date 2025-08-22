using System.Diagnostics;
using System.Net.Http.Json;

using Chat.Common.Contracts;
using Chat.Common.Models;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var dbClient = new HttpClient {
    BaseAddress = new Uri(Chat.Common.Addresses.CHAT_DB_SERVICE)
};

var builder = WebApplication.CreateBuilder(args);

// Services registrieren
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// Swagger aktivieren
if (app.Environment.IsDevelopment()) { }

// Beispiel-Endpunkt
app.MapGet("/", () => "Type=ChatHistoryService");
app.MapPost("/history", async ([FromBody] HistoryRetrieveContract historyContract) => {
    var start = Stopwatch.StartNew();
    // Hier könnte Logik zum Abrufen der ChatRoom-Historie stehen
    // TODO: call DB Service to get messages for room and time range

    var DBResponse = await dbClient.PostAsJsonAsync("/getMessages", historyContract);

    var response = await DBResponse.Content.ReadFromJsonAsync<HistoryResponseContract>();

    start.Stop();

    var subTag = new BenchmarkSubTag(
        "Microservice/History/history",
        start.ElapsedMilliseconds,
        GC.GetAllocatedBytesForCurrentThread(),
        GC.GetTotalAllocatedBytes()
    );
    var newTag = response.Tag;
    newTag.SubTags.Add(subTag);

    return Results.Json(response with {
        Tag = newTag
    });
});

app.Run(Chat.Common.Addresses.CHAT_HISTORY_SERVICE);