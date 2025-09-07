using System.Text;

using BetterConsoles.Tables.Configuration;

using OxyPlot;
using OxyPlot.Series;
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
    //    File.WriteAllLinesAsync($"../../../microservice-report_{DateTime.Now.ToString().Replace(':', '-')}.txt", microserviceReport.Split('\n'));
    //    sb.AppendLine(microserviceReport + "\n");
    //
    //    monolithReport = activeMonolithTester.Report(out BenchmarkReport monolithBenchmarkReport);
    //    sb.AppendLine(monolithReport);
    //    File.WriteAllLinesAsync($"../../../monolith-report_{DateTime.Now.ToString().Replace(':', '-')}.txt", monolithReport.Split('\n'));
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
        foreach (var rep in allMicroserviceReports) {
            foreach (var data in rep.DataList) {
                foreach (var subTag in data.Tag.SubTags) {
                    if (subTag.Origin.Contains("ChatDatabaseService")) {
                        data.Tag.SubTags.Remove(subTag);
                        break;
                    }
                }
            }
        }
        var allMonolithReports = monolithTester.Select(t => t.Report(out _)).ToList();
        // ERROR: Exception when there is only data with the value of 0
        //
        var microserviceFullReport = GetFullReportFromBenchmarkReports(allMicroserviceReports, "microservice");
        var monolithFullReport = GetFullReportFromBenchmarkReports(allMonolithReports, "monolith");

        var table = new BetterConsoles.Tables.Table(TableConfig.Unicode());
        var all = new List<FullReportModel>();
        all.AddRange(microserviceFullReport);
        all.AddRange(monolithFullReport);
        table.From(all.OrderBy(x => x.ServerType).Select(x=>new FullReportViewModel(x)).ToArray());
        sb.AppendLine(table.ToString());

        return sb.ToString();
    }
    private List<FullReportModel> GetFullReportFromBenchmarkReports(List<BenchmarkReport> allMicroserviceReports, string serverType) {
        return allMicroserviceReports.SelectMany(x => x.SubReports)
            .GroupBy(x => x.Endpoint)
            .Select(g => new FullReportModel {
                Endpoint = g.Key,
                Count = g.Count(),
                ServerType = serverType,
                MinMinDuration = g.Min(d => d.MinDurationMs),
                MinAvgDuration = g.Average(d => d.MinDurationMs),
                MinMaxDuration = g.Max(d => d.MinDurationMs),
                AvgMinDuration = g.Min(d => d.AvgDurationMs),
                AvgAvgDuration = g.Average(d => d.AvgDurationMs),
                AvgMaxDuration = g.Max(d => d.AvgDurationMs),
                MaxMinDuration = g.Min(d => d.MaxDurationMs),
                MaxAvgDuration = g.Average(d => d.MaxDurationMs),
                MaxMaxDuration = g.Max(d => d.MaxDurationMs)
            }).ToList();
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
    public List<PlotModel> CreatePlot(BenchmarkReport microserviceReport, BenchmarkReport monolithReport) {
        var list = new List<PlotModel>();

        var plotModelRoom = new PlotModel() { Title = "Room Results(Microservice: Blue, Monolith: Red)" };
        LineSeries roomSeries1 = CreateLineSeriesFromReport(microserviceReport, "/room", OxyColors.Blue);
        LineSeries roomSeries2 = CreateLineSeriesFromReport(monolithReport, "/room", OxyColors.Red);
        plotModelRoom.Series.Add(roomSeries1);
        plotModelRoom.Series.Add(roomSeries2);
        list.Add(plotModelRoom);
        //////////////////////////////////////////
        var plotModelHistory = new PlotModel() { Title = "History Results(Microservice: Blue, Monolith: Red)" };
        LineSeries historySeries1 = CreateLineSeriesFromReport(microserviceReport, "/history", OxyColors.Blue);
        LineSeries historySeries2 = CreateLineSeriesFromReport(monolithReport, "/history",OxyColors.Red );
        plotModelHistory.Series.Add(historySeries1);
        plotModelHistory.Series.Add(historySeries2);
        list.Add(plotModelHistory);
        //////////////////////////////////////////
        var plotModelSend = new PlotModel() { Title = "Send Results(Microservice: Blue, Monolith: Red)" };
        LineSeries sendSeries1 = CreateLineSeriesFromReport(microserviceReport, "/send", OxyColors.Blue);
        LineSeries sendSeries2 = CreateLineSeriesFromReport(monolithReport, "/send", OxyColors.Red);
        plotModelSend.Series.Add(sendSeries1);
        plotModelSend.Series.Add(sendSeries2);
        list.Add(plotModelSend);

        return list;
    }
    private static LineSeries CreateLineSeriesFromReport(BenchmarkReport report1, string endpoint, OxyColor color) {

        var roomSeries = new LineSeries {
            Title = report1.ServiceType,
            MarkerType = MarkerType.Circle,
            MarkerSize = 2,
            MarkerStroke = color,
            Color = color
        };
        int i = 0;
        foreach(var item in report1.DataList.Where(x=>x.Endpoint.Contains(endpoint))) {
            roomSeries.Points.Add(new DataPoint(i++,item.DurationMs));
        }
        return roomSeries;
    }
}

public record FullReportModel {
    public string Endpoint { get; set; }
    public int Count { get; set; }
    public string ServerType { get; set; }
    public float DataMin { get; set; }
    public float MinMinDuration { get; set; }
    public float MinAvgDuration { get; set; }
    public float MinMaxDuration { get; set; }
    public float DataAvg { get; set; }
    public float AvgMinDuration { get; set; }
    public float AvgAvgDuration { get; set; }
    public float AvgMaxDuration { get; set; }
    public float DataMax { get; set; }
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
        var minAvgMean = (((model.MinAvgDuration - model.MinMinDuration) / (model.MinMaxDuration - model.MinMinDuration))*100);
        MinAvgDuration = $"{model.MinAvgDuration:F2} ms{(float.IsNaN(minAvgMean)?"":$"({minAvgMean}%)")}";
        MinMaxDuration = $"{model.MinMaxDuration:F2} ms";
        AvgMinDuration = $"{model.AvgMinDuration:F2} ms";
        var avgAvgMean = (((model.AvgAvgDuration - model.AvgMinDuration) / (model.AvgMaxDuration - model.AvgMinDuration))*100);
        AvgAvgDuration = $"{model.AvgAvgDuration:F2} ms{(float.IsNaN(avgAvgMean)?"":$"({avgAvgMean}%)")}";
        AvgMaxDuration = $"{model.AvgMaxDuration:F2} ms";
        MaxMinDuration = $"{model.MaxMinDuration:F2} ms";
        var maxAvgMean = (((model.MaxAvgDuration - model.MaxMinDuration) / (model.MaxMaxDuration - model.MaxMinDuration))*100);
        MaxAvgDuration = $"{model.MaxAvgDuration:F2} ms{(float.IsNaN(maxAvgMean)?"":$"({maxAvgMean}%)")}";
        MaxMaxDuration = $"{model.MaxMaxDuration:F2} ms";
        //MinMinAllocatedBytes = $"{(model.MinMinAllocatedBytes/1024/1024):N0} MB";
        //MinAvgAllocatedBytes = $"{(model.MinAvgAllocatedBytes/1024/1024):N0} MB";
        //MinMaxAllocatedBytes = $"{(model.MinMaxAllocatedBytes/1024/1024):N0} MB";
        //AvgMinAllocatedBytes = $"{(model.AvgMinAllocatedBytes/1024/1024):N0} MB";
        //AvgAvgAllocatedBytes = $"{(model.AvgAvgAllocatedBytes/1024/1024):N0} MB";
        //AvgMaxAllocatedBytes = $"{(model.AvgMaxAllocatedBytes/1024/1024):N0} MB";
        //MaxMinAllocatedBytes = $"{(model.MaxMinAllocatedBytes/1024/1024):N0} MB";
        //MaxAvgAllocatedBytes = $"{(model.MaxAvgAllocatedBytes/1024/1024):N0} MB";
        //MaxMaxAllocatedBytes = $"{(model.MaxMaxAllocatedBytes/1024/1024):N0} MB";
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
    //public string MinMinAllocatedBytes { get; set; }
    //public string MinAvgAllocatedBytes { get; set; }
    //public string MinMaxAllocatedBytes { get; set; }
    //public string AvgMinAllocatedBytes { get; set; }
    //public string AvgAvgAllocatedBytes { get; set; }
    //public string AvgMaxAllocatedBytes { get; set; }
    //public string MaxMinAllocatedBytes { get; set; }
    //public string MaxAvgAllocatedBytes { get; set; }
    //public string MaxMaxAllocatedBytes { get; set; }
}