using System.Text;
using LiteDB;
using System.Threading;
using System.Collections.Generic;
using System;

namespace Chat.Tests {
    public abstract class BenchmarkTesterBase : ATester {
        protected string runIndexIdentifier;
        protected List<string> usernames = new();
        protected Random rand = new();
        protected List<Thread> threads = new();
        protected string connectionString { get; set; }
        protected int threadCount;
        protected int msgCount;
        protected int threadThrottle;
        protected DateTime startTime = DateTime.MinValue;
        protected DateTime endTime = DateTime.MinValue;
        public StringBuilder ReportBuilder { get; set; } = new StringBuilder();

        public BenchmarkTesterBase(string connectionString, int maxThreads, int maxMessages, int threadThrottle) {
            this.connectionString = connectionString;
            this.threadThrottle = threadThrottle;
            threadCount = maxThreads;
            msgCount = maxMessages;
            for (int i = 0; i < 100; i++) {
                usernames.Add($"user{i + 1}");
            }
            runIndexIdentifier = Guid.NewGuid().ToString();
        }

        public override void Run() {
            startTime = DateTime.UtcNow;
            using (LiteDatabase db = new(connectionString)) {
                var dataCollection = db.GetCollection<Data>("data");
                for (int i = 0; i < threadCount; i++) {
                    var thread = new Thread(() => ExecuteBenchmarkThread(dataCollection));
                    threads.Add(thread);
                    thread.Start();
                    Thread.Sleep(threadThrottle);
                }
                foreach (var t in threads) t.Join();
            }
            endTime = DateTime.UtcNow;
        }

        protected abstract void ExecuteBenchmarkThread(ILiteCollection<Data> dataCollection);

        public override string Report() {
            using (LiteDatabase db = new(connectionString)) {
                var dataCollection = db.GetCollection<Data>("data");
                ReportBuilder.AppendLine($"{GetType().Name} Benchmark Report\n");
                ReportBuilder.AppendLine($"Run Index Identifier: {runIndexIdentifier}");
                ReportBuilder.AppendLine($"Anzahl Threads: {threadCount}");
                ReportBuilder.AppendLine($"Anzahl Nachrichten pro Thread: {msgCount}");
                ReportBuilder.AppendLine($"Thread Throttle: {threadThrottle} ms");
                ReportBuilder.AppendLine($"Startzeit: {startTime:HH:mm:ss.fff}");
                ReportBuilder.AppendLine($"Endzeit: {endTime:HH:mm:ss.fff}");
                ReportBuilder.AppendLine($"Dauer: {(endTime - startTime).TotalSeconds:F3} Sekunden\n");
                ReportBuilder.AppendLine($"Benchmark abgeschlossen: {dataCollection.Count()} Requests verarbeitet.");
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
                foreach (var endpointData in groupedData) {
                    ReportBuilder.AppendLine($"Endpoint: {endpointData.Endpoint}");
                    ReportBuilder.AppendLine($"  Requests: {endpointData.Count}");
                    ReportBuilder.AppendLine($"  Avg Duration: {endpointData.AvgDuration:F2} ms");
                    ReportBuilder.AppendLine($"  Min Duration: {endpointData.MinDuration:F2} ms");
                    ReportBuilder.AppendLine($"  Max Duration: {endpointData.MaxDuration:F2} ms");
                }
                return ReportBuilder.ToString();
            }
        }
    }
}

