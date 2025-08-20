using Chat.Common;
using Chat.Common.Contracts;
using Chat.Common.Models;

using LiteDB;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
const string DATABASE_PATH = "./chat.db";

var db = new LiteDB.LiteDatabase(DATABASE_PATH);

var roomCollection = db.GetCollection<ChatRoom>(CollectionName.ROOMS);
var usersCollection = db.GetCollection<User>(CollectionName.USERS);
var messagesCollection = db.GetCollection<Message>(CollectionName.MESSAGES);
//var tokensCollection = db.GetCollection<Token>(CollectionName.TOKENS);

var builder = WebApplication.CreateBuilder(args);

// Services registrieren
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// Swagger aktivieren
if (app.Environment.IsDevelopment()) {
}

// Beispiel-Endpunkt
app.MapGet("/", () => "Type=ChatDatabaseService");

app.MapPost("/insertMessage", async ([FromBody] MessageSendContract messageSendContract) => {
    // get user reference
    var user = usersCollection.FindOne(u => u.Username.Equals(messageSendContract.Sender));
    // get room reference
    var room = roomCollection.FindOne(r => r.Id.Equals(messageSendContract.RoomId));
    // add Message to the room
    var message = new Message {
        Id = Guid.NewGuid().ToString(),
        SendingUser = user,
        ChatRoom = room,
        Content = messageSendContract.Content,
        Timestamp = DateTime.UtcNow
    };
    messagesCollection.Insert(message);
    room.Messages.Add(message);
    roomCollection.Update(room);
    // update collections

    return Results.Json(new MessageSendResponseContract(messageSendContract.Content, true));
});
app.MapPost("/getMessages", async ([FromBody] HistoryRetrieveContract historyRetrieveContract) => {
    var room = roomCollection.FindOne(r => r.Id.Equals(historyRetrieveContract.RoomId));

    var messages = messagesCollection
        .Include(m=>m.ChatRoom)
        .Include(m => m.ChatRoom.Users)
        .Include(m => m.SendingUser)
        .Find(x=> x.ChatRoom.Id.Equals(historyRetrieveContract.RoomId))
        .Where(x=>x.Timestamp > historyRetrieveContract.StartDate)
        .ToList();

    return Results.Json(new HistoryResponseContract(messages, true));
});

app.MapPost("/getroom", async ([FromBody] RoomRetrieveContract roomRetrieveContract) => {
    List<string> usernames = [roomRetrieveContract.Sender];
    usernames.AddRange(roomRetrieveContract.Receivers);

    var users = GetOrCreateUsers(usernames);

    var expectedChatRoomID = GetComparableRoomId(users);

    // ERROR: creates allways a new room
    var room = GetOrCreateRoom(expectedChatRoomID);

    UpdateRoomWithUsers(room, users);
    return Results.Json(new RoomRetrieveResponseContract(true, RoomId:room.Id));
});

app.Run(Addresses.CHAT_DB_SERVICE);

List<User> GetOrCreateUsers(List<string> usernames) {
    var users = new List<User>();
    foreach (var username in usernames) {
        var refUser = usersCollection.FindOne(u => u.Username.Equals(username));
        // if the user does not exist, create a new user
        if (refUser is null) {
            var newUser = new User {
                Id = Guid.NewGuid().ToString(),
                Username = username,
                ChatRooms = []
            };
            usersCollection.Insert(newUser);
            users.Add(newUser);
        } else {
            // add the existing user to the userList
            users.Add(refUser);
        }
    }
    return users;
}

ChatRoom GetOrCreateRoom(string expectedChatRoomID) {
    // Check if the room already exists
    var room = roomCollection.FindOne(r => r.Id.Equals(expectedChatRoomID));
    // If the room does not exist, create a new one
    if (room is null) {
        room = new ChatRoom {
            Id = expectedChatRoomID,
            ComparableUserBasedId = expectedChatRoomID,
            Users = new List<User>()
        };
        roomCollection.Insert(room);
    }
    return room;
}

void UpdateRoomWithUsers(ChatRoom room, List<User> users) {
    // Füge nur Benutzer hinzu, die noch nicht im Raum sind
    room.Users = users.Where(u => room.Users.All(existing => existing.Id != u.Id)).Concat(room.Users).ToList();
    // Aktualisiere den Raum in der Datenbank
    roomCollection.Update(room);
    // update users with the room
    foreach (var user in users.Where(user => !user.ChatRooms.Contains(room))) {
        user.ChatRooms.Add(room);
    }
    // Update the users in the database
    usersCollection.Update(users);
}

string GetComparableRoomId(List<User> userList) => string.Join("-", userList.Select(r => r.Id).OrderBy(r => r));