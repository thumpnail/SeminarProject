using Chat.Common;
var client = new HttpClient { BaseAddress = new Uri(Addresses.CHAT_MONOLITH_SERVICE) };


Console.WriteLine(client.GetAsync("/").Result);
