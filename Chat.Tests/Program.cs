using BetterConsoles.Tables.Configuration;

using Chat.Common;
using Chat.Tests;

using OxyPlot;
using OxyPlot.Core.Drawing;

#if DEBUG
string connectionString = $"../../../../benchmark.db";
#elif RELEASE
string connectionString = $"/benchmark.db";
#endif

if (File.Exists(connectionString)) {
    File.Delete(connectionString);
}

#if DEBUG
string configPath = "../../../../TestConfig.yaml";
#elif RELEASE
string configPath = "./TestConfig.yaml";
#endif
if (!File.Exists(configPath)) {
    using (StreamWriter sw = new StreamWriter(configPath)) {
        sw.Write(
            """
            MAX_ITERATIONS: 5
            ITERATION_THROTTLE: 0
            MAX_THREADS: 10
            MAX_MESSAGES: 25
            THREAD_THROTTLE: 0
            """
            );
    }
}

dynamic cfg = new YamlDotNet.Serialization.Deserializer().Deserialize(File.ReadAllText(configPath));

IBenchmarkDatabase benchmarkDatabase;
if (args.Length > 0) {
    switch (args[0]) {
        case "lite":
            benchmarkDatabase = new LiteBenchmarkDatabase("./chat-monolith.db");
            break;
        case "memory":
            benchmarkDatabase = new InMemoryBenchmarkDatabase();
            break;
        case "help":
        case "--help":
            Console.WriteLine("Usage: ChatDatabaseService [lite|memory]");
            return;
        default:
            Console.WriteLine("No database type specified, defaulting to 'mock'. Use 'help' for options.\n");
            benchmarkDatabase = new FlatMockBenchmarkDatabase();
            break;
    }
} else {
    Console.WriteLine("No database type specified, defaulting to 'memory'. Use 'help' for options.\n");
    benchmarkDatabase = new InMemoryBenchmarkDatabase();
}

int MAX_ITERATIONS = int.Parse(cfg["MAX_ITERATIONS"]) ?? 1;
int ITERATION_THROTTLE = int.Parse(cfg["ITERATION_THROTTLE"]) ?? 0;

int MAX_THREADS = int.Parse(cfg["MAX_THREADS"]) ?? 100;
int MAX_MESSAGES = int.Parse(cfg["MAX_MESSAGES"]) ?? 10;

int THREAD_THROTTLE = int.Parse(cfg["THREAD_THROTTLE"]) ?? 0;

var ReportHelper = new ReportHelper();
var mainRunTime = DateTime.Now.ToString().Replace(':', '-');

var microserviceDBInfo = new Dictionary<string, string>();
var monolithDBInfo = new Dictionary<string, string>();

#if DEBUG
string rootPath = "../../../../";
#elif RELEASE
string rootPath = "./";
#endif


for (int i = 0; i < MAX_ITERATIONS; i++) {
    var iterationRunTime = DateTime.Now.ToString().Replace(':', '-');
    var microserviceTester = new ChatMicroserviceATester(benchmarkDatabase, MAX_THREADS, MAX_MESSAGES, THREAD_THROTTLE) {
        ServiceType = "microservice"
    };
    var monolithTester = new ChatMonolithATester(benchmarkDatabase, MAX_THREADS, MAX_MESSAGES, THREAD_THROTTLE) {
        ServiceType = "monolith"
    };

    ReportHelper.SetActive(microserviceTester, monolithTester);

    microserviceTester.Run();
    microserviceDBInfo = microserviceTester.databaseInfo;
    // TODO: Create a report for microserviceTester(Atom/Single/Sub)
    var microserviceBenchmarkReport = ReportHelper.CreateReport(microserviceTester, out var microserviceReport);
    Console.WriteLine(microserviceReport);
    Directory.CreateDirectory($"{rootPath}Reports/{mainRunTime}");
    File.WriteAllLinesAsync($"{rootPath}Reports/{mainRunTime}/microservice-{microserviceDBInfo["Type"]}-{microserviceDBInfo["DBType"]}-report-{i}_{iterationRunTime}.txt", microserviceBenchmarkReport.Split('\n'));

    monolithTester.Run();
    monolithDBInfo = monolithTester.databaseInfo;
    // TODO: Create a report for monolithTester(Atom/Single/Sub)
    var monolithBenchmarkReport = ReportHelper.CreateReport(monolithTester, out var monolithReport);
    Console.WriteLine(monolithReport);
    Directory.CreateDirectory($"{rootPath}Reports/{mainRunTime}");
    File.WriteAllLinesAsync($"{rootPath}Reports/{mainRunTime}/monolith-{monolithDBInfo["Type"]}-{monolithDBInfo["DBType"]}-report-{i}_{iterationRunTime}.txt", monolithBenchmarkReport.Split('\n'));


    var plotModels = ReportHelper.CreatePlot(microserviceReport, monolithReport);
    var pngExporter = new PngExporter { Width = 800, Height = 600 };
    foreach (var item in plotModels) {
        var fileTitle = item.Title.Split("(").First();
        pngExporter.ExportToFile(item, $"{rootPath}Reports/{mainRunTime}/final-plot-micro_{microserviceDBInfo["Type"]}-{microserviceDBInfo["DBType"]}-mono_{monolithDBInfo["Type"]}-{monolithDBInfo["DBType"]}-{fileTitle}-{mainRunTime}.png");
    }


    // TODO: Create a combined/full report / Based on ReportHelper.Active
    var combinedBenchmarkReport = ReportHelper.CreateCombinedReport(microserviceReport, monolithReport);
    Console.WriteLine(combinedBenchmarkReport);
    Directory.CreateDirectory($"{rootPath}Reports/{mainRunTime}");
    File.WriteAllText($"{rootPath}Reports/{mainRunTime}/combine-report-micro_{microserviceDBInfo["Type"]}-{microserviceDBInfo["DBType"]}-mono_{monolithDBInfo["Type"]}-{monolithDBInfo["DBType"]}-{i}_{iterationRunTime}.txt", combinedBenchmarkReport);

    Thread.Sleep(ITERATION_THROTTLE);
}

// TODO: Create the final report
string finalBenchmarkReport = ReportHelper.CreateFinalReport();
Console.WriteLine(finalBenchmarkReport);
Directory.CreateDirectory($"{rootPath}Reports/{mainRunTime}");
File.WriteAllText($"{rootPath}Reports/{mainRunTime}/final-report-_{mainRunTime}_micro_{microserviceDBInfo["Type"]}-{microserviceDBInfo["DBType"]}-mono_{monolithDBInfo["Type"]}-{monolithDBInfo["DBType"]}.txt", finalBenchmarkReport);