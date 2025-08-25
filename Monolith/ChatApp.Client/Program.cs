using System.Net.Http.Json;

using Chat.Common;

using ChatApp.Client;
var client = new HttpClient { BaseAddress = new Uri(Addresses.CHAT_MONOLITH_SERVICE) };


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
client.GetWelcomeMessage();

// Get room information
var room = await client.GetRoomAsync(currentUser, currentReceivers);

// Get chat history between current user and receiver
await client.GetChatHistory(room.RoomId);

Task.Run(() => {
    while (true) {
        Thread.Sleep(2000);
        // Fetch last messages from the chat history
        client.FetchLastMessages(room.RoomId);
    }
});


var input = string.Empty;
do {
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
            client.SendMessageAsync(new(currentUser, room.RoomId, input, DateTime.Now))
                .ContinueWith(task => {
                    if (!task.Result.Success) {
                        Console.WriteLine("Failed to send Message: " + task.Result.Message);
                    }
                });
            // clearing the field
            //messageField.Text = string.Empty;
        }
    }
} while (true);