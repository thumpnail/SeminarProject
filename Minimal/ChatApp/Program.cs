// See https://aka.ms/new-console-template for more information
using System.Net.Http.Json;

using Chat.Common;
using Chat.Common.Contracts;
using Chat.Common.Models;

DateTime lastMessageTimestamp = DateTime.MinValue;

HttpClient messagingClient = new() { BaseAddress = new Uri(Addresses.CHAT_MESSAGING_SERVICE) };
HttpClient historyClient = new() { BaseAddress = new Uri(Addresses.CHAT_HISTORY_SERVICE) };

Console.WriteLine("Welcome to the ChatRoom Application!");
Console.Write("Please provide your username(use ',' to separate):");
var currentUser = Console.ReadLine();
// Validate user input
if (string.IsNullOrWhiteSpace(currentUser)) {
    Console.WriteLine("Invalid input. Please provide a valid username and at least one receiver.");
    return;
}
Console.Write("Receivers:");
var currentReceivers = Console.ReadLine()?.Split(',').Select(r => r.Trim()).ToArray();
if (currentReceivers is null || currentReceivers.Length == 0) {
    // Validate user input
    if (string.IsNullOrWhiteSpace(currentUser) || currentReceivers?.Length == 0) {
        Console.WriteLine("Invalid input. Please provide a valid username and at least one receiver.");
        return;
    }
}

// Welcome Message
GetWelcomeMessage(messagingClient);

// Get room information
var roomId = await GetRoomAsync(currentUser, currentReceivers);

// Get chat history between current user and receiver
await GetChatHistory(roomId);

var input = string.Empty;
do {
    Console.Write("You: ");
    input = Console.ReadLine();
    if (input.Equals("!logout")) {
        break;
    }
    // send Message to api endpoint
    if (!string.IsNullOrEmpty(input) && input.StartsWith('/')) {
        // todo: ExecuteCommand(Message);
    } else {
        // Process the Message and send it to the server
        if (!string.IsNullOrWhiteSpace(input)) {
            // Add Message to chat view or send to server
            SendMessageAsync(new(currentUser, roomId, input, DateTime.UtcNow))
                .ContinueWith((task) => {
                    if (!task.Result.Success) {
                        Console.WriteLine("Failed to send Message: " + task.Result.Message);
                    }
                });
            // clearing the field
            //messageField.Text = string.Empty;
        }
    }
} while (true);

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

async Task<MessageSendResponseContract> SendMessageAsync(MessageSendContract messageContract) {
    var response = await messagingClient.PostAsJsonAsync("/send", messageContract);
    if (response.IsSuccessStatusCode) {
        return await response.Content.ReadFromJsonAsync<MessageSendResponseContract>();
    }
    Console.WriteLine("System Failed to send Message.");
    return new MessageSendResponseContract("Failed to send Message.", false);
}
// This method retrieves the welcome Message from the chat service
void GetWelcomeMessage(HttpClient httpClient) {
    try {
        var welcomeMsg = httpClient.GetAsync("/").Result;
        Console.WriteLine(welcomeMsg.Content.ReadAsStringAsync().Result);
    } catch (Exception e) {
        Console.WriteLine("Error connecting to the chat service");
    }
}
async Task<string> GetRoomAsync(string sender, string[] receivers) {
    var roomResponse = await messagingClient.PostAsJsonAsync("/room",
        new RoomRetrieveContract(sender, receivers));

    if (roomResponse.IsSuccessStatusCode) {
        var room = await roomResponse.Content.ReadFromJsonAsync<RoomRetrieveResponseContract>();
        if (room is { RoomId: not null }) {
            Console.WriteLine($"Room ID: {room.RoomId}");
            return room.RoomId;
        }
    }
    Console.WriteLine("Failed to retrieve room information.");
    return null;
}
async Task FetchLastMessages(string roomId) {
    currentReceivers.Order();
    var historyRetrieveContract = new HistoryRetrieveContract(roomId, lastMessageTimestamp, -1);
    var historyResponse = await historyClient.PostAsJsonAsync("/history", historyRetrieveContract);
    if (historyResponse.IsSuccessStatusCode) {
        var history = await historyResponse.Content.ReadFromJsonAsync<HistoryResponseContract>();
        if (history != null && history.Messages.Count > 0) {
            Console.WriteLine("Last Messages:");
            foreach (var message in history.Messages) {
                Console.WriteLine($"{message.Timestamp}: {message.SendingUser.Username}: {message.Content}");
            }
        } else {
            Console.WriteLine("No messages found.");
        }
    } else {
        Console.WriteLine("Failed to retrieve messages.");
    }
}

async Task GetChatHistory(string roomId) {
    // Fetch the last messages from the chat history
    var last = DateTime.Now.AddDays(-1);
    lastMessageTimestamp = last;
    // Sort the Receivers to ensure consistent room ID generation
    currentReceivers.Order();
    // Retrieve the chat history for the room
    var historyResponse = await historyClient.PostAsJsonAsync("/history", new HistoryRetrieveContract(roomId, last, 50));
    if (historyResponse.IsSuccessStatusCode) {
        // Read the response content as HistoryResponseContract
        var history = await historyResponse.Content.ReadFromJsonAsync<HistoryResponseContract>();
        if (history != null && history.Messages.Count > 0) {
            Console.WriteLine("Chat History:");
            foreach (var message in history?.Messages) {
                Console.WriteLine($"{message?.SendingUser?.Username}: {message?.Content}");
            }
        } else {
            Console.WriteLine("No chat history found.");
        }
    } else {
        Console.WriteLine("Failed to retrieve chat history.");
    }
}