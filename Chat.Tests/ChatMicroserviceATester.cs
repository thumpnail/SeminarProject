using System.Text;

using ChatApp.Client;
using LiteDB;
namespace Chat.Tests {
    public class ChatMicroserviceATester : BenchmarkTesterBase {
        public ChatMicroserviceATester(string connectionString, int maxThreads, int maxMessages, int threadThrottle)
            : base(connectionString, maxThreads, maxMessages, threadThrottle) {}

        protected override void ExecuteBenchmarkThread(ILiteCollection<Data> dataCollection) {
            var messagingClient = new HttpClient {
                BaseAddress = new(Chat.Common.Addresses.CHAT_MESSAGING_SERVICE),
                Timeout = TimeSpan.FromSeconds(200)
            };
            var historyClient = new HttpClient {
                BaseAddress = new(Chat.Common.Addresses.CHAT_HISTORY_SERVICE),
                Timeout = TimeSpan.FromSeconds(200)
            };
            var sender = usernames[rand.Next(usernames.Count)];
            var receiver = usernames[rand.Next(usernames.Count)];
            if (sender == receiver) return;
            // Get room information
            var getRoomStart = DateTime.UtcNow;
            var roomId = messagingClient.GetRoomAsync(sender, [receiver]);
            roomId.Wait(TimeSpan.FromSeconds(200));
            var roomDuration = (float)(DateTime.UtcNow - getRoomStart).TotalMilliseconds;
            dataCollection.Insert(new Data(runIndexIdentifier, "microservice", "/room", getRoomStart, roomDuration, sender, receiver));

            // Get chat history
            var getHistoryStart = DateTime.UtcNow;
            var historyTask = historyClient.GetChatHistory(roomId.Result);
            historyTask.Wait(TimeSpan.FromSeconds(200));
            var histroyDuration = (float)(DateTime.UtcNow - getHistoryStart).TotalMilliseconds;
            dataCollection.Insert(new Data(runIndexIdentifier, "microservice", "/history", getHistoryStart, histroyDuration, sender, receiver));

            // Send messages
            for (int msgIdx = 0; msgIdx < msgCount; msgIdx++) {
                var msgStart = DateTime.UtcNow;
                var sendTask = messagingClient.SendMessageAsync(new(sender, roomId.Result, $"{sender}:Message{msgIdx} -> {receiver}", DateTime.UtcNow));
                try {
                    sendTask.Wait(TimeSpan.FromSeconds(200));
                } catch (AggregateException) {
                    continue;
                }
                var duration = (float)(DateTime.UtcNow - msgStart).TotalMilliseconds;
                dataCollection.Insert(new Data(runIndexIdentifier, "microservice", "/send", msgStart, duration, sender, receiver));
                Thread.Sleep(1000);
            }
        }
    }
}