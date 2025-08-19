using Chat.Common;
using Chat.Common.Contracts;
using MessagePack;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

HttpClient dbClient = new HttpClient {
    BaseAddress = new(Addresses.CHAT_DB_SERVICE)
};
HttpClient historyClient = new HttpClient {
    BaseAddress = new(Addresses.CHAT_HISTORY_SERVICE)
};

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

// Define a simple endpoint
app.MapGet("/", () => "Type=ChatMessagingService");
app.MapPost("/send", async ([FromBody] MessageSendContract messageSend) => {
    Console.WriteLine($"({messageSend.Sender} -> {messageSend.Content} -> {messageSend.Receiver})");
    return Results.Json(new{});
});

// Run the web server
app.Run(Chat.Common.Addresses.CHAT_MESSAGING_SERVICE);