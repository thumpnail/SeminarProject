using Chat.Common.Contracts;
using MessagePack;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
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

// Define a simple endpoint
app.MapGet("/", () => "Type=ChatMessagingService");
app.MapGet("/welcome", (string ab) => "Welcome to the ChatRoom Messaging Service!");
app.MapPost("/send", async ([FromBody] MessageSendContract messageSend) => {
    // TODO: call DB Service to persist message
    // TODO: notify user (simulate)
    var responseMessage = new MessageSendResponseContract {
        Message = messageSend.Content ?? "",
        Success = true
    };
    return Results.Json(responseMessage);
});
app.MapPost("/getmessages", () => {
    Console.WriteLine("Heartbeat received");
    
    return Results.Ok();
});
// Define a login endpoint
app.MapPost("/login", ([FromBody] LoginContract loginContract) => {
    Console.WriteLine($"Login attempt for user: {loginContract.Username} with password: {loginContract.Password}");
    return Results.Json(new LoginResponseContract {
        Message = "Login successful",
        Success = true
    });
});
app.MapGet("/logout", () => {
    return Results.Ok();
});

// User und Raumverwaltung
app.MapPost("/createroom", ([FromBody] dynamic payload) => {
    // payload: { name, isPrivate, userIds }
    var room = new Chat.Common.Models.ChatRoom {
        Id = Guid.NewGuid().ToString(),
        Name = payload.name,
        IsPrivate = payload.isPrivate,
        UserIds = ((IEnumerable<string>)payload.userIds).ToList(),
        MessageIds = new List<string>()
    };
    // TODO: call DB Service to persist room
    return Results.Ok(room);
});

app.MapPost("/notify", ([FromBody] dynamic payload) => {
    // Simuliere Benachrichtigung
    return Results.Ok($"User {payload.userId} notified for new messages in room {payload.roomId}");
});

// Run the web server
app.Run(Chat.Common.Addresses.CHAT_MESSAGING_SERVICE);