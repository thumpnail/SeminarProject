using Chat.Common;
using Chat.Common.Contracts;
using Chat.Common.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
const string DATABASE_PATH = "./chat.db";

var db = new LiteDB.LiteDatabase(DATABASE_PATH);

var roomCollection = db.GetCollection<ChatRoom>("chats");
var usersCollection = db.GetCollection<User>("users");
var messagesCollection = db.GetCollection<Message>("messages");
var tokensCollection = db.GetCollection<Token>("tokens");

var builder = WebApplication.CreateBuilder(args);

// Services registrieren
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// Swagger aktivieren
if (app.Environment.IsDevelopment()) {
}

// Beispiel-Endpunkt
app.MapGet("/", () => "Type=ChatDatabaseService");
app.MapPost("/login", ([FromBody] LoginContract loginContract) => {
	// Hier könnte Logik zum Erstellen eines neuen Benutzers oder einer neuen Nachricht stehen
	var user = usersCollection.FindOne(u => u.Username == loginContract.Username);
	var newToken = Guid.NewGuid().ToString();

	if (user.Password != loginContract.Password) {
		return Results.Ok(new LoginResponseContract(
		"User or message created successfully.",
		true,
		new LoginToken(newToken,user.Username)
		));
	}
	return Results.Unauthorized();
});
app.MapPost("/register", async ([FromBody] RegisterUserContract registerUserContract) => {
	//throw new NotImplementedException("Register endpoint not implemented yet");
	var user = new User {
		Id = null,
		Username = null,
		Password = null,
		ChatRooms = null
	};
	usersCollection.Insert(user);
	return Results.Ok(new RegisterUserResponseContract(
		"User registered successfully.",
		true,
		null
	));
});

app.MapPost("/createroom", async ([FromBody] CreateRoomContract createRoomContract) => {
	roomCollection.Insert(createRoomContract.ChatRoom);
	return Results.Ok(new CreateRoomResponseContract());
});

app.MapPost("/addusertoroom", async ([FromBody] AddUserToRoomContract addUserToRoomContract) => {
	// Extract userId and roomId from payload
	var roomId = addUserToRoomContract.RoomId;
	var userId = addUserToRoomContract.UserId;
	// Find user and room by their IDs
	var user = usersCollection.FindById(userId);
	var room = roomCollection.FindById(roomId);
	// If user or room is not found, return NotFound
	if (room == null)
		return Results.NotFound("Room not found");
	// If user is not found, return NotFound
	if (!room.Users.Contains(user))
		room.Users.Add(user);
	roomCollection.Update(room);
	return Results.Ok(new AddUserToRoomResponseContract() {
		Message = "User added to room successfully.",
		Success = true,
		RoomId = room.Id,
		UserId = user.Id
	});
});

app.MapPost("/insertMessage", async ([FromBody] MessageSendContract messageSendContract) => {
	User senderUser = usersCollection.FindById(messageSendContract.Sender);
	User receivingUser = usersCollection.FindById(messageSendContract.Receiver);
	ChatRoom chatRoom = roomCollection.FindById(messageSendContract.roomId);
	if (chatRoom != null) {
		roomCollection.Update(chatRoom);
	}
	messagesCollection.Insert(new Message {
		Id = Guid.NewGuid().ToString(),
		User = senderUser,
		ReceivingUser = receivingUser,
		ChatRoom = chatRoom,
		Content = messageSendContract.Content,
		Timestamp = DateTime.Now
	});

	return Results.Ok(messageSendContract);
});

app.MapGet("/messages/{roomId}", (string roomId) => {
	var room = roomCollection.FindById(roomId);
	if (room == null) return Results.NotFound("Room not found");
	var msgs = messagesCollection.Find(m => room.Messages.Contains(m.Id)).ToList();
	return Results.Ok(msgs);
});

app.MapGet("/rooms/{userId}", (string userId) => {
	var rooms = roomCollection.Find(r => r.Users.Contains(userId)).ToList();
	return Results.Ok(rooms);
});

app.Run(Chat.Common.Addresses.CHAT_DB_SERVICE);