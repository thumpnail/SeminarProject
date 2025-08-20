using System.Net.Http.Json;

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
// receive a Message
app.MapPost("/send", async ([FromBody] MessageSendContract msgSend) => {
    var response = await dbClient.PostAsJsonAsync("/insertMessage", msgSend);

    var msgSendResponse = await response.Content.ReadFromJsonAsync<MessageSendResponseContract>();

    if (msgSendResponse is null) {
        return Results.Json(new MessageSendResponseContract("Failed to send Message", false));
    }

    return Results.Json(msgSendResponse);
});
// create a room
app.MapPost("/room", async ([FromBody] RoomRetrieveContract room) => {
    var roomResponse = await dbClient.PostAsJsonAsync("/getroom",
    new RoomRetrieveContract(room.Sender, room.Receivers));

    var parsedRoom = await roomResponse.Content.ReadFromJsonAsync<RoomRetrieveResponseContract>();

    if (parsedRoom == null) {
        return Results.Json(new RoomRetrieveResponseContract(false, Message:"Failed to retrieve room"));
    }
    return Results.Json(new RoomRetrieveResponseContract(parsedRoom.Success, parsedRoom.Message, parsedRoom.RoomId));
});

// Run the web server
app.Run(Chat.Common.Addresses.CHAT_MESSAGING_SERVICE);