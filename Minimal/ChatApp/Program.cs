// See https://aka.ms/new-console-template for more information
using System.Net.Http.Json;

using Chat.Common;
using Chat.Common.Contracts;
using Chat.Common.Models;

HttpClient messagingClient = new() { BaseAddress = new Uri(Addresses.CHAT_MESSAGING_SERVICE) };

HttpClient historyClient = new() { BaseAddress = new Uri(Addresses.CHAT_HISTORY_SERVICE) };

Console.WriteLine("Welcome to the ChatRoom Application!");
Console.Write("Please provide your username(use ',' to separate):");
var currentUser = Console.ReadLine();
Console.Write("Receivers:");
var currentReceivers = Console.ReadLine().Split(',').Select(r => r.Trim()).ToList();

// Welcome message
GetWelcomeMessage(messagingClient);

// Get room information
var roomId = await GetRoomAsync(currentUser, currentReceivers.ToArray());

// Get chat history between current user and receiver
await GetChatHistory(roomId);

var input = string.Empty;
do {
    Console.Write("You: ");
    input = Console.ReadLine();
    // send message to api endpoint
    if (!string.IsNullOrEmpty(input) && input.StartsWith('/')) {
        // todo: ExecuteCommand(message);
    } else {
        // Process the message and send it to the server
        if (!string.IsNullOrWhiteSpace(input)) {
            // Add message to chat view or send to server
            AddMessages(("You", input));
            SendMessageAsync(new("You", "User", input))
                .ContinueWith((task) => {
                    if (!task.Result.Success) {
                        Console.WriteLine("Failed to send message: " + task.Result.Message);
                    }
                });
            // clearing the field
            //messageField.Text = string.Empty;
        }
    }
} while (input.Contains("!logout") == false);

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

void AddMessages(params (string username, string message)[] newMessages) {
    foreach (var message in newMessages) {
        //messages.Add(message.username + ": " + message.message);
    }
}

async Task<MessageSendResponseContract> SendMessageAsync(MessageSendContract messageContract) {
    var response = await messagingClient.PostAsJsonAsync("/send", messageContract);
    if (response.IsSuccessStatusCode) {
        return await response.Content.ReadFromJsonAsync<MessageSendResponseContract>();
    }

    AddMessages(("System", "Failed to send message."));
    return new MessageSendResponseContract("Failed to send message.", false);
}
// This method retrieves the welcome message from the chat service
void GetWelcomeMessage(HttpClient httpClient) {
    try {
        var welcomeMsg = httpClient.GetAsync("/").Result;
        Console.WriteLine(welcomeMsg.Content.ReadAsStringAsync().Result);
    } catch (Exception e) {
        Console.WriteLine("Error connecting to the chat service");
    }
}
async Task<string> GetRoomAsync(string sender, string[] receivers) {
    var roomRetrieveContract = new RoomRetrieveContract(sender, receivers);
    var roomResponse = await messagingClient.PostAsJsonAsync("/room", roomRetrieveContract);
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
async Task GetChatHistory(string roomId) {
    currentReceivers.Sort();
    var historyRetrieveContract = new HistoryRetrieveContract(roomId, DateTime.Now.AddDays(-1), 50);
    var historyResponse = await historyClient.PostAsJsonAsync("/history", historyRetrieveContract);
    if (historyResponse.IsSuccessStatusCode) {
        var history = await historyResponse.Content.ReadFromJsonAsync<HistoryResponseContract>();
        if (history != null && history.Messages.Count > 0) {
            Console.WriteLine("Chat History:");
            foreach (var message in history.Messages) {
                Console.WriteLine($"{message.Timestamp}: {message.SendingUser.Username}: {message.Content}");
            }
        } else {
            Console.WriteLine("No chat history found.");
        }
    } else {
        Console.WriteLine("Failed to retrieve chat history.");
    }
}