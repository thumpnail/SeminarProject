using BetterConsoles.Tables.Configuration;

using Chat.Tests;

string connectionString = $"../../../../benchmark.db";

if (File.Exists(connectionString)) {
    File.Delete(connectionString);
}

var benchmarkDatabase = new InMemoryBenchmarkDatabase(connectionString);

const int MAX_ITERATIONS = 1;
const int ITERATION_THROTTLE = 100;

const int MAX_THREADS = 10;
const int MAX_MESSAGES = 100;

const int THREAD_THROTTLE = 0;

var ReportHelper = new ReportHelper();
var mainRunTime = DateTime.Now.ToString().Replace(':', '-');
for (int i = 0; i < MAX_ITERATIONS; i++) {
    var iterationRunTime = DateTime.Now.ToString().Replace(':', '-');
    var microserviceTester = new ChatMicroserviceATester(benchmarkDatabase, MAX_THREADS, MAX_MESSAGES, THREAD_THROTTLE);
    var monolithTester = new ChatMonolithATester(benchmarkDatabase, MAX_THREADS, MAX_MESSAGES, THREAD_THROTTLE);

    ReportHelper.SetActive(microserviceTester, monolithTester);

    microserviceTester.Run();
    // TODO: Create a report for microserviceTester(Atom/Single/Sub)
    var microserviceBenchmarkReport = ReportHelper.CreateReport(microserviceTester, out var microserviceReport);
    Console.WriteLine(microserviceReport);
    Directory.CreateDirectory($"../../../../Reports/{mainRunTime}");
    File.WriteAllLinesAsync($"../../../../Reports/{mainRunTime}/microservice-report-{i}_{iterationRunTime}.txt", microserviceBenchmarkReport.Split('\n'));

    monolithTester.Run();
    // TODO: Create a report for monolithTester(Atom/Single/Sub)
    var monolithBenchmarkReport = ReportHelper.CreateReport(monolithTester, out var monolithReport);
    Console.WriteLine(monolithReport);
    Directory.CreateDirectory($"../../../../Reports/{mainRunTime}");
    File.WriteAllLinesAsync($"../../../../Reports/{mainRunTime}/monolith-report-{i}_{iterationRunTime}.txt", monolithBenchmarkReport.Split('\n'));


    // TODO: Create a combined/full report / Based on ReportHelper.Active
    var combinedBenchmarkReport = ReportHelper.CreateCombinedReport(microserviceReport, monolithReport);
    Console.WriteLine(combinedBenchmarkReport);
    Directory.CreateDirectory($"../../../../Reports/{mainRunTime}");
    File.WriteAllText($"../../../../Reports/{mainRunTime}/combine-report-{i}_{iterationRunTime}.txt", combinedBenchmarkReport);

    Thread.Sleep(ITERATION_THROTTLE);
}

// TODO: Create the final report
string finalBenchmarkReport = ReportHelper.CreateFinalReport();
Console.WriteLine(finalBenchmarkReport);
Directory.CreateDirectory($"../../../../Reports/{mainRunTime}");
File.WriteAllText($"../../../../Reports/{mainRunTime}/final-report_{mainRunTime}.txt", finalBenchmarkReport);