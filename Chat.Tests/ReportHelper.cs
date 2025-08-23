using System.Text;

using BetterConsoles.Tables.Configuration;
namespace Chat.Tests;

public class ReportHelper {
    public List<ChatMicroserviceATester> microserviceTester { get; set; } = new List<ChatMicroserviceATester>();
    public List<ChatMonolithATester> monolithTester { get; set; } = new List<ChatMonolithATester>();

    public ChatMicroserviceATester activeMicroserviceTester { get; set; }
    public ChatMonolithATester activeMonolithTester { get; set; }

    public string CreateCombinedReport(
        BenchmarkReport microserviceReport,
        BenchmarkReport monolithReport) {

        var sb = new StringBuilder();

        // List<BenchmarkReport> both = BenchmarkReports(out microserviceReport, ref sb, out monolithReport);
        List<BenchmarkReport> both = [microserviceReport, monolithReport];
        //batter console tables
        sb.AppendLine("\n\n-- Benchmark Summary --\n");
        var table = new BetterConsoles.Tables.Table(TableConfig.Unicode());
        table.From(both.ToArray());
        sb.AppendLine(table.ToString());

        sb.AppendLine("\n\n-- Benchmark Sub-Reports --\n");

        table = new BetterConsoles.Tables.Table(TableConfig.Unicode());
        table.From<BenchmarkSubReport>(both.SelectMany(x => x.SubReports.Where(x => x.Endpoint.Contains("/room"))).ToArray());
        sb.AppendLine(table.ToString() + "\n");

        table = new BetterConsoles.Tables.Table(TableConfig.Unicode());
        table.From<BenchmarkSubReport>(both.SelectMany(x => x.SubReports.Where(x => x.Endpoint.Contains("/history"))).ToArray());
        sb.AppendLine(table.ToString() + "\n");

        table = new BetterConsoles.Tables.Table(TableConfig.Unicode());
        table.From<BenchmarkSubReport>(both.SelectMany(x => x.SubReports.Where(x => x.Endpoint.Contains("/send"))).ToArray());
        sb.AppendLine(table.ToString() + "\n");

        return sb.ToString();
    }
    //private List<BenchmarkReport> BenchmarkReports(
    //    out string microserviceReport,
    //    ref StringBuilder sb,
    //    out string monolithReport
    //) {
    //    sb.AppendLine("\n\n-- Benchmark Results --\n");
    //    microserviceReport = activeMicroserviceTester.Report(out BenchmarkReport microserviceBenchmarkReport);
    //    sb.AppendLine(microserviceReport);
    //    File.WriteAllLinesAsync($"../../../microservice-report_{DateTime.UtcNow.ToString().Replace(':', '-')}.txt", microserviceReport.Split('\n'));
    //    sb.AppendLine(microserviceReport + "\n");
    //
    //    monolithReport = activeMonolithTester.Report(out BenchmarkReport monolithBenchmarkReport);
    //    sb.AppendLine(monolithReport);
    //    File.WriteAllLinesAsync($"../../../monolith-report_{DateTime.UtcNow.ToString().Replace(':', '-')}.txt", monolithReport.Split('\n'));
    //    sb.AppendLine(monolithReport + "\n");
    //
    //    var both = new List<BenchmarkReport>();
    //    both.AddRange(microserviceBenchmarkReport);
    //    both.AddRange(monolithBenchmarkReport);
    //
    //    return both;
    //}

    public string CreateFinalReport() {
        var sb = new StringBuilder();
        sb.AppendLine("\n\n-- Final Benchmark Report --\n");

        var allMicroserviceReports = microserviceTester.Select(t => t.Report(out _)).ToList();
        var allMonolithReports = monolithTester.Select(t => t.Report(out _)).ToList();

        var microserviceFullReport = allMicroserviceReports.SelectMany(x => x.SubReports)
            .GroupBy(x => x.Endpoint)
            .Select(g => new FullReportModel {
                Endpoint = g.Key,
                Count = g.Count(),
                ServerType = "microservice",
                MinMinDuration = g.Where(x => x.MinDurationMs > 0).Min(d => d.MinDurationMs),
                MinAvgDuration = g.Where(x => x.MinDurationMs > 0).Average(d => d.MinDurationMs),
                MinMaxDuration = g.Where(x => x.MinDurationMs > 0).Max(d => d.MinDurationMs),
                AvgMinDuration = g.Where(x => x.AvgDurationMs > 0).Min(d => d.AvgDurationMs),
                AvgAvgDuration = g.Where(x => x.AvgDurationMs > 0).Average(d => d.AvgDurationMs),
                AvgMaxDuration = g.Where(x => x.AvgDurationMs > 0).Max(d => d.AvgDurationMs),
                MaxMinDuration = g.Where(x => x.MaxDurationMs > 0).Min(d => d.MaxDurationMs),
                MaxAvgDuration = g.Where(x => x.MaxDurationMs > 0).Average(d => d.MaxDurationMs),
                MaxMaxDuration = g.Where(x => x.MaxDurationMs > 0).Max(d => d.MaxDurationMs),
                MinMinAllocatedBytes = g.Where(x => x.MinAllocatedBytes > 0).Min(d => d.MinAllocatedBytes),
                MinAvgAllocatedBytes = g.Where(x => x.MinAllocatedBytes > 0).Average(d => d.MinAllocatedBytes),
                MinMaxAllocatedBytes = g.Where(x => x.MinAllocatedBytes > 0).Max(d => d.MinAllocatedBytes),
                AvgMinAllocatedBytes = g.Where(x => x.AvgAllocatedBytes > 0).Min(d => d.AvgAllocatedBytes),
                AvgAvgAllocatedBytes = g.Where(x => x.AvgAllocatedBytes > 0).Average(d => d.AvgAllocatedBytes),
                AvgMaxAllocatedBytes = g.Where(x => x.AvgAllocatedBytes > 0).Max(d => d.AvgAllocatedBytes),
                MaxMinAllocatedBytes = g.Where(x => x.MaxAllocatedBytes > 0).Min(d => d.MaxAllocatedBytes),
                MaxAvgAllocatedBytes = g.Where(x => x.MaxAllocatedBytes > 0).Average(d => d.MaxAllocatedBytes),
                MaxMaxAllocatedBytes = g.Where(x => x.MaxAllocatedBytes > 0).Max(d => d.MaxAllocatedBytes),
            }).ToList();
        var monolithFullReport = allMonolithReports.SelectMany(x => x.SubReports)
            .GroupBy(x => x.Endpoint)
            .Select(g => new FullReportModel {
                Endpoint = g.Key,
                Count = g.Count(),
                ServerType = "monolith",
                // Min
                MinMinDuration = g.Where(x => x.MinDurationMs > 0).Min(d => d.MinDurationMs),
                MinAvgDuration = g.Where(x => x.MinDurationMs > 0).Average(d => d.MinDurationMs),
                MinMaxDuration = g.Where(x => x.MinDurationMs > 0).Max(d => d.MinDurationMs),
                AvgMinDuration = g.Where(x => x.AvgDurationMs > 0).Min(d => d.AvgDurationMs),
                AvgAvgDuration = g.Where(x => x.AvgDurationMs > 0).Average(d => d.AvgDurationMs),
                AvgMaxDuration = g.Where(x => x.AvgDurationMs > 0).Max(d => d.AvgDurationMs),
                MaxMinDuration = g.Where(x => x.MaxDurationMs > 0).Min(d => d.MaxDurationMs),
                MaxAvgDuration = g.Where(x => x.MaxDurationMs > 0).Average(d => d.MaxDurationMs),
                MaxMaxDuration = g.Where(x => x.MaxDurationMs > 0).Max(d => d.MaxDurationMs),
                MinMinAllocatedBytes = g.Where(x => x.MinAllocatedBytes > 0).Min(d => d.MinAllocatedBytes),
                MinAvgAllocatedBytes = g.Where(x => x.MinAllocatedBytes > 0).Average(d => d.MinAllocatedBytes),
                MinMaxAllocatedBytes = g.Where(x => x.MinAllocatedBytes > 0).Max(d => d.MinAllocatedBytes),
                AvgMinAllocatedBytes = g.Where(x => x.AvgAllocatedBytes > 0).Min(d => d.AvgAllocatedBytes),
                AvgAvgAllocatedBytes = g.Where(x => x.AvgAllocatedBytes > 0).Average(d => d.AvgAllocatedBytes),
                AvgMaxAllocatedBytes = g.Where(x => x.AvgAllocatedBytes > 0).Max(d => d.AvgAllocatedBytes),
                MaxMinAllocatedBytes = g.Where(x => x.MaxAllocatedBytes > 0).Min(d => d.MaxAllocatedBytes),
                MaxAvgAllocatedBytes = g.Where(x => x.MaxAllocatedBytes > 0).Average(d => d.MaxAllocatedBytes),
                MaxMaxAllocatedBytes = g.Where(x => x.MaxAllocatedBytes > 0).Max(d => d.MaxAllocatedBytes),
            }).ToList();

        var table = new BetterConsoles.Tables.Table(TableConfig.Unicode());
        var all = new List<FullReportModel>();
        all.AddRange(microserviceFullReport);
        all.AddRange(monolithFullReport);
        table.From(all.OrderBy(x => x.ServerType).Select(x=>new FullReportViewModel(x)).ToArray());
        sb.AppendLine(table.ToString());

        return sb.ToString();
    }

    public string CreateReport(BenchmarkTesterBase tester, out BenchmarkReport benchmarkReport) {
        var sb = new StringBuilder();
        sb.AppendLine("\n\n-- Benchmark Results --\n");
        benchmarkReport = tester.Report(out var microserviceReport);
        sb.AppendLine(microserviceReport);
        return sb.ToString();
    }

    public void SetActive(ChatMicroserviceATester chatMicroserviceATester, ChatMonolithATester chatMonolithATester) {
        activeMicroserviceTester = chatMicroserviceATester;
        activeMonolithTester = chatMonolithATester;
        microserviceTester.Add(chatMicroserviceATester);
        monolithTester.Add(chatMonolithATester);
    }
}

public record FullReportModel {
    public string Endpoint { get; set; }
    public int Count { get; set; }
    public string ServerType { get; set; }
    public float MinMinDuration { get; set; }
    public float MinAvgDuration { get; set; }
    public float MinMaxDuration { get; set; }
    public float AvgMinDuration { get; set; }
    public float AvgAvgDuration { get; set; }
    public float AvgMaxDuration { get; set; }
    public float MaxMinDuration { get; set; }
    public float MaxAvgDuration { get; set; }
    public float MaxMaxDuration { get; set; }
    public long MinMinAllocatedBytes { get; set; }
    public double MinAvgAllocatedBytes { get; set; }
    public long MinMaxAllocatedBytes { get; set; }
    public double AvgMinAllocatedBytes { get; set; }
    public double AvgAvgAllocatedBytes { get; set; }
    public double AvgMaxAllocatedBytes { get; set; }
    public long MaxMinAllocatedBytes { get; set; }
    public double MaxAvgAllocatedBytes { get; set; }
    public long MaxMaxAllocatedBytes { get; set; }
}
public record FullReportViewModel {
    public FullReportViewModel(FullReportModel model) {
        Endpoint = model.Endpoint;
        IterationsCount = model.Count;
        ServerType = model.ServerType;
        MinMinDuration = $"{model.MinMinDuration:F2} ms";
        MinAvgDuration = $"{model.MinAvgDuration:F2} ms({(((model.MinAvgDuration - model.MinMinDuration) / (model.MinMaxDuration - model.MinMinDuration))*100):F1}% mean)";
        MinMaxDuration = $"{model.MinMaxDuration:F2} ms";
        AvgMinDuration = $"{model.AvgMinDuration:F2} ms";
        AvgAvgDuration = $"{model.AvgAvgDuration:F2} ms({(((model.AvgAvgDuration - model.AvgMinDuration) / (model.AvgMaxDuration - model.AvgMinDuration))*100):F1}% mean)";
        AvgMaxDuration = $"{model.AvgMaxDuration:F2} ms";
        MaxMinDuration = $"{model.MaxMinDuration:F2} ms";
        MaxAvgDuration = $"{model.MaxAvgDuration:F2} ms({(((model.MaxAvgDuration - model.MaxMinDuration) / (model.MaxMaxDuration - model.MaxMinDuration))*100):F1}% mean)";
        MaxMaxDuration = $"{model.MaxMaxDuration:F2} ms";
        MinMinAllocatedBytes = $"{(model.MinMinAllocatedBytes/1024/1024):N0} MB";
        MinAvgAllocatedBytes = $"{(model.MinAvgAllocatedBytes/1024/1024):N0} MB";
        MinMaxAllocatedBytes = $"{(model.MinMaxAllocatedBytes/1024/1024):N0} MB";
        AvgMinAllocatedBytes = $"{(model.AvgMinAllocatedBytes/1024/1024):N0} MB";
        AvgAvgAllocatedBytes = $"{(model.AvgAvgAllocatedBytes/1024/1024):N0} MB";
        AvgMaxAllocatedBytes = $"{(model.AvgMaxAllocatedBytes/1024/1024):N0} MB";
        MaxMinAllocatedBytes = $"{(model.MaxMinAllocatedBytes/1024/1024):N0} MB";
        MaxAvgAllocatedBytes = $"{(model.MaxAvgAllocatedBytes/1024/1024):N0} MB";
        MaxMaxAllocatedBytes = $"{(model.MaxMaxAllocatedBytes/1024/1024):N0} MB";
    }
    public string Endpoint { get; set; }
    public int IterationsCount { get; set; }
    public string ServerType { get; set; }
    public string MinMinDuration { get; set; }
    public string MinAvgDuration { get; set; }
    public string MinMaxDuration { get; set; }
    public string AvgMinDuration { get; set; }
    public string AvgAvgDuration { get; set; }
    public string AvgMaxDuration { get; set; }
    public string MaxMinDuration { get; set; }
    public string MaxAvgDuration { get; set; }
    public string MaxMaxDuration { get; set; }
    public string MinMinAllocatedBytes { get; set; }
    public string MinAvgAllocatedBytes { get; set; }
    public string MinMaxAllocatedBytes { get; set; }
    public string AvgMinAllocatedBytes { get; set; }
    public string AvgAvgAllocatedBytes { get; set; }
    public string AvgMaxAllocatedBytes { get; set; }
    public string MaxMinAllocatedBytes { get; set; }
    public string MaxAvgAllocatedBytes { get; set; }
    public string MaxMaxAllocatedBytes { get; set; }
}