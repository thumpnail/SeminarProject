using Chat.Common;
using Chat.Common.Contracts;
using Chat.Common.Models;

using ChatApp.Server.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container (optional)
builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<DatabaseService>();


var app = builder.Build();

// Optional: Swagger UI
if (app.Environment.IsDevelopment()) {
	//app.UseSwagger();
	//app.UseSwaggerUI();
}

// Define a simple endpoint
app.MapGet("/", () => "Type=ChatMessagingService");

app.MapPost("/send", ([FromBody] MessageSendContract messageSendContract, DatabaseService db) => {
    Thread.Sleep(10);
    // get user reference
    User user;
    user = db.usersCollection.FindOne(u => u.Username.Equals(messageSendContract.Sender));

    // get room reference
    ChatRoom room;
    room = db.roomCollection.FindOne(r => r.Id.Equals(messageSendContract.RoomId));
    // add Message to the room
    var message = new Message {
        Id = Guid.NewGuid().ToString(),
        SendingUser = user,
        ChatRoom = room,
        Content = messageSendContract.Content,
        Timestamp = DateTime.UtcNow
    };
    db.messagesCollection.Insert(message);
    room.Messages.Add(message);
    db.roomCollection.Update(room);

    // update collections

    return Results.Json(new MessageSendResponseContract(messageSendContract.Content, true));
});

app.MapPost("/history", ([FromBody] HistoryRetrieveContract historyRetrieveContract, DatabaseService db) => {
    Thread.Sleep(10);
    ChatRoom room;
    room = db.roomCollection.FindOne(r => r.Id.Equals(historyRetrieveContract.RoomId));

    List<Message> messages = new();
    messages = db.messagesCollection
            .Query()
            .Where(x=>x.ChatRoom.Id.Equals(room.Id) && x.Timestamp > historyRetrieveContract.StartDate)
            //.Include(m => m.ChatRoom)
            //.Include(m => m.ChatRoom.Users)
            //.Include(m => m.SendingUser)
            .Limit(historyRetrieveContract.Limit)
            .ToList();

    return Results.Json(new HistoryResponseContract(messages, true));
});

app.MapPost("/room", ([FromBody] RoomRetrieveContract roomRetrieveContract, DatabaseService db) => {
    Thread.Sleep(10);
    List<string> usernames = [roomRetrieveContract.Sender];
    usernames.AddRange(roomRetrieveContract.Receivers);

    var users = db.GetOrCreateUsers(usernames);

    var expectedChatRoomID = db.GetComparableRoomId(users);

    // ERROR: creates allways a new room
    var room = db.GetOrCreateRoom(expectedChatRoomID);

    db.UpdateRoomWithUsers(room, users);
    return Results.Json(new RoomRetrieveResponseContract(true, RoomId:room.Id));
});

// Run the web server
app.Run(Addresses.CHAT_MONOLITH_SERVICE);