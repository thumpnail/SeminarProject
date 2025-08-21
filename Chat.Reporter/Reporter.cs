using LiteDB;

namespace Chat.Reporter;

public class Reporter {
    public string type { get; set; }
    private LiteDB.LiteDatabase db { get; set; }
    private ILiteCollection<Data> dataCollection { get; set; }
    public Reporter() {
        db = new LiteDB.LiteDatabase("../../../../Chat.Tests/bin/Debug/net9.0/benchmark.db");
        dataCollection = db.GetCollection<Data>("data");
        type = "ChatMicroservice";
    }
    public void Run() {
        var unIndexes = dataCollection
            .Query()
            .Select(x => x.runIndexIdentifier)
            .ToEnumerable()
            .Distinct()
            .ToList();
        var nidx = 1;
        Console.WriteLine("index;endpoint;duration;count;avgDuration;minDuration;maxDuration");
        foreach (var idx in unIndexes) {
            var reportData = dataCollection.Find(x => x.runIndexIdentifier == idx).ToList();
            var groupedData = reportData
                .GroupBy(d => d.endpoint)
                .Select(g => new {
                    Endpoint = g.Key,
                    Count = g.Count(),
                    AvgDuration = g.Average(d => d.durationMs),
                    MinDuration = g.Min(d => d.durationMs),
                    MaxDuration = g.Max(d => d.durationMs)
                }).ToList();

            foreach (var endpointData in groupedData) {
                //Console.WriteLine($"Endpoint: {endpointData.Endpoint}");
                //Console.WriteLine($"  Requests: {endpointData.Count}");
                //Console.WriteLine($"  Avg Duration: {endpointData.AvgDuration:F2} ms");
                //Console.WriteLine($"  Min Duration: {endpointData.MinDuration:F2} ms");
                //Console.WriteLine($"  Max Duration: {endpointData.MaxDuration:F2} ms");
                //Console.WriteLine();
                Console.WriteLine($"{nidx};{type};'{endpointData.Endpoint}';{endpointData.AvgDuration:F2};{endpointData.Count};{endpointData.AvgDuration:F2};{endpointData.MinDuration:F2};{endpointData.MaxDuration:F2}");
            }
            nidx++;
        }
    }
}