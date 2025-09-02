using System.Diagnostics;

using Chat.Common;
using Chat.Common.Contracts;
using Chat.Common.Models;

using LiteDB;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
//const string DATABASE_PATH = "./chat.db";
//
//var db = new LiteDB.LiteDatabase(DATABASE_PATH);
//
//var roomCollection = db.GetCollection<ChatRoom>(CollectionName.ROOMS);
//var usersCollection = db.GetCollection<User>(CollectionName.USERS);
//var messagesCollection = db.GetCollection<Message>(CollectionName.MESSAGES);
//var tokensCollection = db.GetCollection<Token>(CollectionName.TOKENS);

var builder = WebApplication.CreateBuilder(args);

// Services registrieren
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// Swagger aktivieren
if (app.Environment.IsDevelopment()) { }
ILogger<Program> appLogger = app.Services.GetRequiredService<ILogger<Program>>();
Logger logger = new("ChatDatabaseService");
FlatMockDatabase database = new("../../../../../chat-microservice.db");
// Beispiel-Endpunkt
app.MapGet("/", () => "Type=ChatDatabaseService");

app.MapPost("/insertMessage", async ([FromBody] MessageSendContract messageSendContract) => {
    var start = Stopwatch.StartNew();
    var response = database.InsertMessage(messageSendContract);
    start.Stop();

    logger.Log("/insertMessage", $"Took {start.ElapsedMilliseconds} ms|{start.Elapsed.Microseconds} ns");
    //appLogger.LogInformation(new EventId(1, "MessageInserted"), $"/insertMessage took {start.ElapsedMilliseconds} ms");

    var tag = new BenchmarkTag([
        new(
            "Microservice/ChatDatabaseService/insertMessage",
            start.ElapsedMilliseconds,
            GC.GetAllocatedBytesForCurrentThread(),
            GC.GetTotalAllocatedBytes())
    ]);
    return Results.Json(
        response with {
            Tag = tag
        }
    );
});
app.MapPost("/getMessages", async ([FromBody] HistoryRetrieveContract historyRetrieveContract) => {
    var start = Stopwatch.StartNew();
    var response = database.GetMessages(historyRetrieveContract);
    start.Stop();

    logger.Log("/getMessages", $"Took {start.ElapsedMilliseconds} ms|{start.Elapsed.Microseconds} ns");
    //appLogger.LogInformation(new EventId(2, "MessagesRetrieved"), $"/getMessages took {start.ElapsedMilliseconds} ms");

    var tag = new BenchmarkTag([
        new(
            "Microservice/ChatDatabaseService/getMessages",
            start.ElapsedMilliseconds,
            GC.GetAllocatedBytesForCurrentThread(),
            GC.GetTotalAllocatedBytes())
    ]);

    return Results.Json(response with {
        Tag = tag
    });
});

app.MapPost("/getroom", async ([FromBody] RoomRetrieveContract roomRetrieveContract) => {
    var start = Stopwatch.StartNew();
    var response = database.GetRoom(roomRetrieveContract);
    start.Stop();

    logger.Log("/getroom", $"Took {start.ElapsedMilliseconds} ms|{start.Elapsed.Microseconds} ns");
    //appLogger.LogInformation(new EventId(3, "RoomRetrieved"), $"/getroom took {start.ElapsedMilliseconds} ms");

    var tag = new BenchmarkTag([
        new("Microservice/ChatDatabaseService/getroom", start.ElapsedMilliseconds, GC.GetAllocatedBytesForCurrentThread(), GC.GetTotalAllocatedBytes())
    ]);

    return Results.Json(
        response with {
            Tag = tag
        }
    );
});

app.Run(Addresses.CHAT_DB_SERVICE);