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

Database Database = new("../../../../../chat-microservice.db");
// Beispiel-Endpunkt
app.MapGet("/", () => "Type=ChatDatabaseService");

app.MapPost("/insertMessage", async ([FromBody] MessageSendContract messageSendContract) => {
    var start = Stopwatch.StartNew();
    var response = Database.InsertMessage(messageSendContract);
    start.Stop();

    return Results.Json(
        response with {
            Tag = new BenchmarkTag([
                new BenchmarkSubTag(
                    "Microservice/Database/insertMessage",
                    start.ElapsedMilliseconds,
                    GC.GetAllocatedBytesForCurrentThread(),
                    GC.GetTotalAllocatedBytes())
            ])
        }
    );
});
app.MapPost("/getMessages", async ([FromBody] HistoryRetrieveContract historyRetrieveContract) => {
    var start = Stopwatch.StartNew();
    var response = Database.GetMessages(historyRetrieveContract);
    start.Stop();

    return Results.Json(response with {
        Tag = new BenchmarkTag([
            new BenchmarkSubTag(
                "Microservice/Database/getMessages",
                start.ElapsedMilliseconds,
                GC.GetAllocatedBytesForCurrentThread(),
                GC.GetTotalAllocatedBytes())
        ])
    });
});

app.MapPost("/getroom", async ([FromBody] RoomRetrieveContract roomRetrieveContract) => {
    var start = Stopwatch.StartNew();
    var response = Database.GetRoom(roomRetrieveContract);
    start.Stop();

    return Results.Json(
        response with {
            Tag = new BenchmarkTag([
                new BenchmarkSubTag(
                    "Microservice/Database/getroom",
                    start.ElapsedMilliseconds,
                    GC.GetAllocatedBytesForCurrentThread(),
                    GC.GetTotalAllocatedBytes())
            ])
        }
    );
});

app.Run(Addresses.CHAT_DB_SERVICE);