using Chat.Common.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
const string DATABASE_PATH = "./chat.db";

var db = new LiteDB.LiteDatabase(DATABASE_PATH);

var chatsCollection = db.GetCollection<ChatRoom>("chats");
var usersCollection = db.GetCollection<User>("users");
var messagesCollection = db.GetCollection<Message>("messages");

var builder = WebApplication.CreateBuilder(args);

// Services registrieren
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// Swagger aktivieren
if (app.Environment.IsDevelopment())
{
}

// Beispiel-Endpunkt
app.MapGet("/", () => "Type=ChatDatabaseService");
app.MapPost("/login", () => {
    // Hier könnte Logik zum Erstellen eines neuen Benutzers oder einer neuen Nachricht stehen
    return Results.Ok("User or message created successfully.");
});
app.MapPost("/register", async ([FromBody] User user) => {
    usersCollection.Insert(user);
    return Results.Ok(user);
});

app.MapPost("/createroom", async ([FromBody] ChatRoom room) => {
    chatsCollection.Insert(room);
    return Results.Ok(room);
});

app.MapPost("/addusertoroom", async ([FromBody] dynamic payload) => {
    var roomId = (string)payload.roomId;
    var userId = (string)payload.userId;
    var room = chatsCollection.FindById(roomId);
    if (room == null) return Results.NotFound("Room not found");
    if (!room.UserIds.Contains(userId)) room.UserIds.Add(userId);
    chatsCollection.Update(room);
    return Results.Ok(room);
});

app.MapPost("/sendmessage", async ([FromBody] Message message) => {
    messagesCollection.Insert(message);
    var room = chatsCollection.FindById(message.ChatRoomId);
    if (room != null) {
        room.MessageIds.Add(message.ID);
        chatsCollection.Update(room);
    }
    return Results.Ok(message);
});

app.MapGet("/messages/{roomId}", (string roomId) => {
    var room = chatsCollection.FindById(roomId);
    if (room == null) return Results.NotFound("Room not found");
    var msgs = messagesCollection.Find(m => room.MessageIds.Contains(m.ID)).ToList();
    return Results.Ok(msgs);
});

app.MapGet("/rooms/{userId}", (string userId) => {
    var rooms = chatsCollection.Find(r => r.UserIds.Contains(userId)).ToList();
    return Results.Ok(rooms);
});

app.Run(Chat.Common.Addresses.CHAT_DB_SERVICE);