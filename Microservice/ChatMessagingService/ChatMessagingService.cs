using System.Diagnostics;
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
using Microsoft.Extensions.Logging;

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
//ILogger<Program> appLogger = app.Services.GetRequiredService<ILogger<Program>>();
Logger logger = new Logger("ChatMessagingService");
// Define a simple endpoint
app.MapGet("/", () => "Type=ChatMessagingService");
// receive a Message
app.MapPost("/send", async ([FromBody] MessageSendContract msgSend) => {
    var start = Stopwatch.StartNew();

    var response = await dbClient.PostAsJsonAsync("/insertMessage", msgSend);

    var msgSendResponse = await response.Content.ReadFromJsonAsync<MessageSendResponseContract>();

    start.Stop();

    if (msgSendResponse is null) {
        return Results.Json(new MessageSendResponseContract(msgSend.runIndexIdentifier, "Failed to send Message", false, msgSendResponse.Tag));
    }

    logger.Log("/send", $"Post '/insertMessage' took {start.ElapsedMilliseconds} ms|{start.Elapsed.Microseconds} ns");
    //appLogger.LogInformation(new EventId(1, "MessageSent"), $"/send Post '/insertMessage' took {start.ElapsedMilliseconds} ms");

    var subTag = new BenchmarkSubTag(
        "ChatMessagingService",
        "Microservice/ChatMessagingService/send",
        start.ElapsedMilliseconds,
        GC.GetAllocatedBytesForCurrentThread(),
        GC.GetTotalAllocatedBytes()
    );
    var newTag = msgSendResponse.Tag;
    newTag.SubTags.Add(subTag);

    return Results.Json(msgSendResponse with {
        Tag = newTag
    });
});
// create a room
app.MapPost("/room", async ([FromBody] RoomRetrieveContract room) => {
    var start = Stopwatch.StartNew();

    var roomResponse = await dbClient.PostAsJsonAsync("/getroom",
        new RoomRetrieveContract(room.runIndexIdentifier, room.Sender, room.Receivers));

    var parsedRoom = await roomResponse.Content.ReadFromJsonAsync<RoomRetrieveResponseContract>();

    start.Stop();

    //if (parsedRoom == null) {
    //    return Results.Json(new RoomRetrieveResponseContract(false, Message:"Failed to retrieve room", null, parsedRoom.Tag));
    //}

    logger.Log("/room", $"Post '/getroom' took {start.ElapsedMilliseconds} ms|{start.Elapsed.Microseconds} ns");
    //appLogger.LogInformation(new EventId(2, "RoomRetrieved"), $"/room Post '/getroom' took {start.ElapsedMilliseconds} ms");

    var subTag = new BenchmarkSubTag(
        "ChatMessagingService",
        "Microservice/ChatMessagingService/room",
        start.ElapsedMilliseconds,
        GC.GetAllocatedBytesForCurrentThread(),
        GC.GetTotalAllocatedBytes()
    );
    var newTag = parsedRoom.Tag;
    newTag.SubTags.Add(subTag);

    return Results.Json(parsedRoom with {
        Tag = newTag
    });
});

// Run the web server
app.Run(Chat.Common.Addresses.CHAT_MESSAGING_SERVICE);
