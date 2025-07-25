// See https://aka.ms/new-console-template for more information
HttpClient client = new HttpClient();
Console.WriteLine("Welcome to the Chat Application!");

client.BaseAddress = new Uri("http://localhost:5000/");

var input = "";
var welcomeMsg = await client.GetAsync("/");
Console.WriteLine(await welcomeMsg.Content.ReadAsStringAsync());
do {
    input = Console.ReadLine();
    // send message to api endpoint
     if (input.Contains("!login")) {
          Console.WriteLine("Logging in...");
     } else if (input.Contains("!logout")) {
          Console.WriteLine("Logging out...");
     } else {
          Console.WriteLine("Sending message...");
          // call api endpoint to send message
          Console.WriteLine($"Message sent: {input}");
          client.PostAsync("/Api/send", new StringContent(input));
     }
} while(input.Contains("!logout") == false);