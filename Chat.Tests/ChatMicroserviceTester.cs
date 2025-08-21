using System.Text;

using ChatApp.Client;

using LiteDB;
namespace Chat.Tests;

public class ChatMicroserviceTester : ITester {
string runIndexIdentifier;
    LiteDatabase db = new LiteDatabase("benchmark.db");
    ILiteCollection<Data> dataCollection;

    List<string> usernames = new List<string>();

    // Benchmark: Simuliere parallele Nachrichtenübertragung
    int threadCount = 100;
    Random rand = new();
    List<Thread> threads = new();

    public ChatMicroserviceTester() {
        for (int i = 0; i < 100; i++) {
            usernames.Add($"user{i + 1}");
        }
        dataCollection = db.GetCollection<Data>("data");
        db = new LiteDatabase("benchmark.db");
        runIndexIdentifier = Guid.NewGuid().ToString();
    }
    public void Run() {
        for (int i = 0; i < threadCount; i++) {
            var thread = new Thread(() => {
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
                Console.WriteLine("Getting room for: " + sender + " -> " + receiver);
                var getRoomStart = DateTime.UtcNow;
                var roomId = messagingClient.GetRoomAsync(sender, [receiver]);
                roomId.Wait(TimeSpan.FromSeconds(200));
                var roomDuration = (float)(DateTime.UtcNow - getRoomStart).TotalMilliseconds;
                dataCollection.Insert(new Data(runIndexIdentifier, "/room", getRoomStart, roomDuration, sender, receiver));

                Console.WriteLine("Getting chat history for: " + sender + " -> " + receiver);
                var getHistoryStart = DateTime.UtcNow;
                var historyTask = historyClient.GetChatHistory(roomId.Result);
                historyTask.Wait(TimeSpan.FromSeconds(200));
                var histroyDuration = (float)(DateTime.UtcNow - getHistoryStart).TotalMilliseconds;
                dataCollection.Insert(new Data(runIndexIdentifier, "/history", getHistoryStart, histroyDuration, sender, receiver));

                // Sende mehrere Nachrichten und messe die Zeit pro Nachricht
                int msgCount = rand.Next(5, 20);
                Console.WriteLine("Sending messages for: " + sender + " -> " + receiver);
                for (int msgIdx = 0; msgIdx < msgCount; msgIdx++) {
                    var msgStart = DateTime.UtcNow;
                    var sendTask = messagingClient.SendMessageAsync(new(sender, roomId.Result, $"{sender}:Message{msgIdx} -> {receiver}", DateTime.UtcNow));
                    try {
                        sendTask.Wait(TimeSpan.FromSeconds(200));
                    } catch (AggregateException ex) {
                        // Fehlerbehandlung, z.B. Timeout
                        continue;
                    }
                    var duration = (float)(DateTime.UtcNow - msgStart).TotalMilliseconds;
                    dataCollection.Insert(new Data(runIndexIdentifier, "/send", msgStart, duration, sender, receiver));
                    Thread.Sleep(rand.Next(100, 2000));
                }
            });
            threads.Add(thread);
            thread.Start();
        }
        // Warten bis alle Threads fertig sind
        foreach (var t in threads) t.Join();
    }
    public StringBuilder ReportBuilder { get; set; } = new StringBuilder();
    public string Report() {
        // Ergebnisse ausgeben
        ReportBuilder.AppendLine($"Benchmark abgeschlossen: {dataCollection.Count()} Nachrichten gesendet.");
        //Console.WriteLine("Beispielmessungen:");
        //foreach (var d in dataCollection.FindAll().Take(10)) {
        //    Console.WriteLine($"{d.timestamp:HH:mm:ss.fff} | {d.durationMs} ms | {d.sender} -> {d.receiver}");
        //}

        // #report
        var reportData = dataCollection.Find(x => x.runIndexIdentifier == runIndexIdentifier).ToList();

        var groupedData = reportData
            .GroupBy(d => d.endpoint)
            .Select(g => new {
                Endpoint = g.Key,
                Count = g.Count(),
                AvgDuration = g.Average(d => d.durationMs),
                MinDuration = g.Min(d => d.durationMs),
                MaxDuration = g.Max(d => d.durationMs)
            })
            .ToList();

        Console.WriteLine("Benchmark Report:");
        foreach (var endpointData in groupedData) {
            ReportBuilder.AppendLine($"Endpoint: {endpointData.Endpoint}");
            ReportBuilder.AppendLine($"  Requests: {endpointData.Count}");
            ReportBuilder.AppendLine($"  Avg Duration: {endpointData.AvgDuration:F2} ms");
            ReportBuilder.AppendLine($"  Min Duration: {endpointData.MinDuration:F2} ms");
            ReportBuilder.AppendLine($"  Max Duration: {endpointData.MaxDuration:F2} ms");
        }


        // Ergebnisse in CSV-Datei schreiben
        //using (var writer = new StreamWriter("benchmark_results.csv"))
        //using (var csv = new CsvWriter(writer, System.Globalization.CultureInfo.InvariantCulture)) {
        //    csv.WriteRecords(dataCollection.FindAll().ToList());
        //}
        return ReportBuilder.ToString();
    }
}