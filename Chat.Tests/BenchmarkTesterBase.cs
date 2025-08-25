using System.Text;

using LiteDB;

using System.Threading;
using System.Collections.Generic;
using System;
using System.Net;

using Chat.Common.Contracts;

using ChatApp.Client;

namespace Chat.Tests;

public abstract class BenchmarkTesterBase : ATester {
    protected string runIndexIdentifier;
    protected List<string> usernames = new();
    protected Random rand = new();
    protected List<Thread> threads = new();
    protected IBenchmarkDatabase benchmarkDatabase;
    protected int threadCount;
    protected int msgCount;
    protected int threadThrottle;
    protected DateTime startTime = DateTime.MinValue;
    protected DateTime endTime = DateTime.MinValue;
    public StringBuilder ReportBuilder { get; set; } = new StringBuilder();
    protected int invalidStatusCodeCount = 0;

    public BenchmarkTesterBase(IBenchmarkDatabase benchmarkDatabase, int maxThreads, int maxMessages, int threadThrottle) {
        this.benchmarkDatabase = benchmarkDatabase;
        this.threadThrottle = threadThrottle;
        threadCount = maxThreads;
        msgCount = maxMessages;
        for (int i = 0; i < maxThreads; i++) {
            usernames.Add($"user{i + 1}");
        }
        runIndexIdentifier = Guid.NewGuid().ToString();
    }

    public void TrackHttpStatusCode(int statusCode) {
        if (statusCode != 200) {
            invalidStatusCodeCount++;
        }
    }

    public override void Run() {
        Console.WriteLine("Starting benchmark...");
        startTime = DateTime.Now;
        for (int i = 0; i < threadCount; i++) {
            var thread = new Thread(() => {
                ExecuteBenchmarkThread(benchmarkDatabase);
                TrackHttpStatusCode(benchmarkDatabase.GetLastHttpStatusCode()); // Example usage
            });
            threads.Add(thread);
            thread.Start();
            Thread.Sleep(threadThrottle);
        }
        foreach (var t in threads) t.Join();
        endTime = DateTime.Now;
        Console.WriteLine($"Invalid HTTP Status Codes: {invalidStatusCodeCount}");
    }

    protected abstract void ExecuteBenchmarkThread(IBenchmarkDatabase benchmarkDatabase);
    public override BenchmarkReport Report(out string reportResult) {
        var dataCollection = benchmarkDatabase.GetDataCollection();
        var reportCollection = benchmarkDatabase.GetReportCollection();
        ReportBuilder.AppendLine($"{GetType().Name} Benchmark Report\n");
        ReportBuilder.AppendLine($"Run Index Identifier: {runIndexIdentifier}");
        ReportBuilder.AppendLine($"Anzahl Threads: {threadCount}");
        ReportBuilder.AppendLine($"Anzahl Nachrichten pro Thread: {msgCount}");
        ReportBuilder.AppendLine($"Thread Throttle: {threadThrottle} ms");
        ReportBuilder.AppendLine($"Startzeit: {startTime:HH:mm:ss.fff}");
        ReportBuilder.AppendLine($"Endzeit: {endTime:HH:mm:ss.fff}");
        ReportBuilder.AppendLine($"Dauer: {(endTime - startTime).TotalSeconds:F3} Sekunden\n");
        var count = -1;
        count = benchmarkDatabase.FindDataByRunIndex(runIndexIdentifier).Count;
        ReportBuilder.AppendLine($"Benchmark abgeschlossen: {count} Requests verarbeitet.");

        List<Data> reportData = benchmarkDatabase.FindDataByRunIndex(runIndexIdentifier);

        var groupedData = reportData
            .GroupBy(d => d.Endpoint)
            .Select(g => new {
                Endpoint = g.Key,
                Count = g.Count(),
                AvgDuration = g.Average(d => d.DurationMs),
                MinDuration = g.Min(d => d.DurationMs),
                MaxDuration = g.Max(d => d.DurationMs),
                MinAllocatedBytes = g.Min(d => d.Tag.SubTags
                    .Select(x => x.Memory)
                    .Sum()),
                MaxAllocatedBytes = g.Max(d => d.Tag.SubTags
                    .Select(x => x.Memory)
                    .Sum()),
                AvgAllocatedBytes = g.Average(d => d.Tag.SubTags
                    .Select(x => x.Memory)
                    .Sum())
            })
            .ToList();
        // create a BenchmarkReport
        var benchmarkReport = new BenchmarkReport {
            ServiceType = GetType().Name,
            RunIndexIdentifier = runIndexIdentifier,
            ThreadCount = threadCount,
            MsgCount = msgCount,
            ThreadThrottle = threadThrottle,
            StartTime = startTime,
            EndTime = endTime,
            Duration = (endTime - startTime).TotalSeconds,
            SubReports = new List<BenchmarkSubReport>()
        };
        foreach (var endpointData in groupedData) {
            ReportBuilder.AppendLine($"Endpoint: {endpointData.Endpoint}");
            ReportBuilder.AppendLine($"  Requests: {endpointData.Count}");
            ReportBuilder.AppendLine($"  Avg Duration: {endpointData.AvgDuration:F2} ms");
            ReportBuilder.AppendLine($"  Min Duration: {endpointData.MinDuration:F2} ms");
            ReportBuilder.AppendLine($"  Max Duration: {endpointData.MaxDuration:F2} ms");
            ReportBuilder.AppendLine($"  Avg Allocated Bytes: {(endpointData.AvgAllocatedBytes / 1024 / 1024):F2} MB");
            ReportBuilder.AppendLine($"  Min Allocated Bytes: {endpointData.MinAllocatedBytes / 1024 / 1024:F2} MB");
            ReportBuilder.AppendLine($"  Max Allocated Bytes: {endpointData.MaxAllocatedBytes / 1024 / 1024:F2} MB");
            benchmarkReport.SubReports.Add(new() {
                Endpoint = benchmarkReport.ServiceType + endpointData.Endpoint,
                Count = endpointData.Count,
                AvgDurationMs = endpointData.AvgDuration,
                MinDurationMs = endpointData.MinDuration,
                MaxDurationMs = endpointData.MaxDuration,
                AvgAllocatedBytes = endpointData.AvgAllocatedBytes,
                MinAllocatedBytes = endpointData.MinAllocatedBytes,
                MaxAllocatedBytes = endpointData.MaxAllocatedBytes
            });
        }
        benchmarkDatabase.InsertReport(benchmarkReport);
        reportResult = ReportBuilder.ToString();
        return benchmarkReport;
    }
    internal void GetChatHistory(IBenchmarkDatabase dataCollection, string serviceType, string endpoint, HttpClient historyClient, string roomId, string sender, string receiver, out DateTime getHistoryStart, out float historyDuration, out BenchmarkTag tag) {
        getHistoryStart = DateTime.Now;
        tag = null!;
        historyDuration = 0f;

        var historyTask = historyClient.GetChatHistory(roomId);
        if (historyTask == null)
            throw new InvalidOperationException("GetChatHistory returned null Task.");

        historyTask.Wait(TimeSpan.FromSeconds(200));
        historyDuration = (float)(DateTime.Now - getHistoryStart).TotalMilliseconds;

        if (historyTask.IsCanceled) {
            Console.WriteLine("GetChatHistory was canceled unexpected.");
            return;
        }

        var result = historyTask.Result;
        if (result == null)
            throw new InvalidOperationException("GetChatHistory result is null.");
        if (result.Tag == null)
            throw new InvalidOperationException("GetChatHistory result.Tag is null.");

        lock (dataCollection) {
            dataCollection.InsertData(new Data(
                    runIndexIdentifier,
                    serviceType,
                    endpoint,
                    getHistoryStart,
                    historyDuration,
                    sender,
                    receiver,
                    result.Tag,
                    200) // HttpStatusCode parameter
            );
        }
        tag = result.Tag;
    }
    internal string GetRoomInformationAsync(IBenchmarkDatabase dataCollection, string serviceType, string endpoint, HttpClient messagingClient, string sender, string receiver, out DateTime getRoomStart, out float roomDuration, out BenchmarkTag tag) {
        getRoomStart = DateTime.Now;
        var room = messagingClient.GetRoomAsync(sender, [receiver]);
        room.Wait();
        if (room.IsCanceled) {
            Console.WriteLine("GetRoomAsync was canceled unexpected.");
            tag = new();
            roomDuration = 0f;
            return null!;
        }
        if (room.IsFaulted) {
            Console.WriteLine("GetRoomAsync encountered an error: " + room.Exception?.Message);
            tag = new();
            roomDuration = 0f;
            return null!;
        }
        if (room.IsCompleted) {
            roomDuration = (float)(DateTime.Now - getRoomStart).TotalMilliseconds;
            lock (dataCollection) {
                dataCollection.InsertData(new Data(
                    runIndexIdentifier,
                    serviceType,
                    endpoint,
                    getRoomStart,
                    roomDuration,
                    sender,
                    receiver,
                    room.Result.Tag,
                    200)); // HttpStatusCode parameter
            }
            tag = room.Result.Tag;
            return room.Result.RoomId;
        }
        tag = new();
        roomDuration = -1f;
        return null!;
    }
    internal void SendMessage(IBenchmarkDatabase benchmarkDataCollection, string serviceType, string endpoint, HttpClient client, string sender, string room, int msgIdx, string receiver, out DateTime msgStart, out Task<MessageSendResponseContract> sendTask, out BenchmarkTag tags) {
        msgStart = DateTime.Now;
        sendTask = client.SendMessageAsync(new(sender, room, $"{sender}:Message{msgIdx} -> {receiver}", DateTime.Now));
        try {
            sendTask.Wait(TimeSpan.FromSeconds(200));
        } catch (AggregateException) {
            tags = new();
            return;
        }
        var duration = (float)(DateTime.Now - msgStart).TotalMilliseconds;
        lock (benchmarkDataCollection) {
            benchmarkDataCollection.InsertData(new Data(
                runIndexIdentifier,
                serviceType,
                endpoint,
                msgStart,
                duration,
                sender,
                receiver,
                sendTask.Result.Tag,
                200)); // HttpStatusCode parameter
        }
        tags = sendTask.Result.Tag;
        //Thread.Sleep(100);
    }
}