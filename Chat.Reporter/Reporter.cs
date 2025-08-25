using LiteDB;

namespace Chat.Reporter;

/// <summary>
/// Analysiert Benchmark-Daten und erstellt Berichte zur Performance der Chat-Systeme.
/// </summary>
public class Reporter {
    /// <summary>
    /// Typ des zu analysierenden Systems (z.B. ChatMicroservice).
    /// </summary>
    public string type { get; set; }
    private LiteDB.LiteDatabase db { get; set; }
    private ILiteCollection<Data> dataCollection { get; set; }

    /// <summary>
    /// Initialisiert die Reporter-Klasse und öffnet die Benchmark-Datenbank.
    /// </summary>
    public Reporter() {
        db = new LiteDB.LiteDatabase("../../../../Chat.Tests/benchmark-1.db");
        dataCollection = db.GetCollection<Data>("data");
        type = "ChatMicroservice";
    }

    /// <summary>
    /// Führt die Analyse aus und gibt die Ergebnisse als Bericht aus.
    /// </summary>
    public void Run() {
        var unIndexes = dataCollection
            .Query()
            .Select(x => x.RunIndexIdentifier)
            .ToEnumerable()
            .Distinct()
            .ToList();
        var nidx = 1;
        Console.WriteLine("index;endpoint;duration;count;avgDuration;minDuration;maxDuration");
        foreach (var idx in unIndexes) {
            var reportData = dataCollection.Find(x => x.RunIndexIdentifier == idx).ToList();
            var groupedData = reportData
                .GroupBy(d => d.Endpoint)
                .Select(g => new {
                    Endpoint = g.Key,
                    Count = g.Count(),
                    AvgDuration = g.Average(d => d.DurationMs),
                    MinDuration = g.Min(d => d.DurationMs),
                    MaxDuration = g.Max(d => d.DurationMs)
                }).ToList();

            // Ausgabe der gruppierten Daten im CSV-Format
            foreach (var endpointData in groupedData) {
                Console.WriteLine($"{nidx};{type};'{endpointData.Endpoint}';{endpointData.AvgDuration:F2};{endpointData.Count};{endpointData.AvgDuration:F2};{endpointData.MinDuration:F2};{endpointData.MaxDuration:F2}");
            }
            nidx++;
        }
    }
}