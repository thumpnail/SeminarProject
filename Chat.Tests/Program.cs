using BetterConsoles.Tables.Configuration;

using Chat.Tests;

string connectionString = $"../../../../benchmark.db";

if (File.Exists(connectionString)) {
    File.Delete(connectionString);
}

const int MAX_ITERATIONS = 10;
const int ITERATION_THROTTLE = 100;

const int MAX_THREADS = 10;
const int MAX_MESSAGES = 25;

const int THREAD_THROTTLE = 100;

var ReportHelper = new ReportHelper();

for (int i = 0; i < MAX_ITERATIONS; i++) {
    var microserviceTester = new ChatMicroserviceATester(connectionString, MAX_THREADS, MAX_MESSAGES, THREAD_THROTTLE);
    var monolithTester = new ChatMonolithATester(connectionString, MAX_THREADS, MAX_MESSAGES, THREAD_THROTTLE);

    ReportHelper.SetActive(microserviceTester, monolithTester);


    microserviceTester.Run();
    // TODO: Create a report for microserviceTester(Atom/Single/Sub)
    var microserviceBenchmarkReport = ReportHelper.CreateReport(microserviceTester, out var microserviceReport);
    Console.WriteLine(microserviceReport);
    File.WriteAllLinesAsync($"../../../../Reports/microservice-report-{i}_{DateTime.UtcNow.ToString().Replace(':', '-')}.txt", microserviceBenchmarkReport.Split('\n'));

    monolithTester.Run();
    // TODO: Create a report for monolithTester(Atom/Single/Sub)
    var monolithBenchmarkReport = ReportHelper.CreateReport(monolithTester, out var monolithReport);
    Console.WriteLine(monolithReport);
    File.WriteAllLinesAsync($"../../../../Reports/monolith-report-{i}_{DateTime.UtcNow.ToString().Replace(':', '-')}.txt", monolithBenchmarkReport.Split('\n'));



    // TODO: Create a combined/full report / Based on ReportHelper.Active
    var combinedBenchmarkReport = ReportHelper.CreateCombinedReport(microserviceReport, monolithReport);
    Console.WriteLine(combinedBenchmarkReport);
    File.WriteAllText($"../../../../Reports/combine-report-{i}_{DateTime.UtcNow.ToString().Replace(':', '-')}.txt", combinedBenchmarkReport);

    Thread.Sleep(ITERATION_THROTTLE);
}

// TODO: Create the final report
string finalBenchmarkReport = ReportHelper.CreateFinalReport();
Console.WriteLine(finalBenchmarkReport);
File.WriteAllText($"../../../../Reports/final-report_{DateTime.UtcNow.ToString().Replace(':', '-')}.txt", finalBenchmarkReport);