using Chat.Common.Contracts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Services registrieren
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// Swagger aktivieren
if (app.Environment.IsDevelopment())
{
}

// Beispiel-Endpunkt
app.MapGet("/", () => "Type=ChatHistoryService");
app.MapPost("/history", ([FromBody] HistoryRetrieveContract historyContract) => {
    // Hier könnte Logik zum Abrufen der ChatRoom-Historie stehen
    // TODO: call DB Service to get messages for room and time range

    var DBResponse = new List<Chat.Common.Models.Message> {};

    var response = new HistoryResponseContract(new(), true);
    return Results.Json(response);
});

app.Run(Chat.Common.Addresses.CHAT_HISTORY_SERVICE);