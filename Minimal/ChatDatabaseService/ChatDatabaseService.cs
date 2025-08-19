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

var roomCollection = db.GetCollection<ChatRoom>(CollectionName.ROOMS);
var usersCollection = db.GetCollection<User>(CollectionName.USERS);
var messagesCollection = db.GetCollection<Message>(CollectionName.MESSAGES);
var tokensCollection = db.GetCollection<Token>(CollectionName.TOKENS);

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
	return Results.Json(new MessageSendResponseContract(messageSendContract.Content, true));
});
app.MapPost("/getMessages", async ([FromBody] HistoryRetrieveContract historyRetrieveContract) => {
	return Results.Json(new HistoryResponseContract( [
		Message.GenerateMessage("user1", "user1-user2", "Hello World!"),
		Message.GenerateMessage("user2", "user1-user2", "Hello User1!"),
		Message.GenerateMessage("user1", "user1-user2", "How are you?"),
		Message.GenerateMessage("user2", "user1-user2", "I'm fine, thanks!"),
		Message.GenerateMessage("user1", "user1-user2", "What about you?"),
		Message.GenerateMessage("user2", "user1-user2", "I'm also fine, thanks!"),
		Message.GenerateMessage("user1", "user1-user2", "Great to hear that!"),
		Message.GenerateMessage("user2", "user1-user2", "Yes, it is!"),
		Message.GenerateMessage("user1", "user1-user2", "Let's meet up soon!"),
		Message.GenerateMessage("user2", "user1-user2", "Sure, that would be great!"),
		Message.GenerateMessage("user1", "user1-user2", "Looking forward to it!"),
		Message.GenerateMessage("user2", "user1-user2", "Me too!"),
		Message.GenerateMessage("user1", "user1-user2", "See you soon!"),
		Message.GenerateMessage("user2", "user1-user2", "Bye!"),
		Message.GenerateMessage("user1", "user1-user2", "Take care!"),
		Message.GenerateMessage("user2", "user1-user2", "You too!"),
		Message.GenerateMessage("user1", "user1-user2", "Have a great day!"),
		Message.GenerateMessage("user2", "user1-user2", "You too!"),
		Message.GenerateMessage("user1", "user1-user2", "Talk to you later!"),
		Message.GenerateMessage("user2", "user1-user2", "Sure, talk to you later!"),
		Message.GenerateMessage("user1", "user1-user2", "Goodbye!")
	], true));
});

app.Run(Addresses.CHAT_DB_SERVICE);