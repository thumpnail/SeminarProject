using Chat.Tests;

const string connectionString = "../../../benchmark-1.db";
const int maxThreads = 1000;
const int maxMessages = 1;

const int threadThrottle = 100;

var microserviceTester = new ChatMicroserviceATester(connectionString,maxThreads,maxMessages,threadThrottle);
microserviceTester.Run();

var monolithTester = new ChatMonolithATester(connectionString,maxThreads,maxMessages,threadThrottle);
monolithTester.Run();

Console.WriteLine("\n\n-- Benchmark Results --\n");
var microserviceReport = microserviceTester.Report();
Console.WriteLine(microserviceReport);
File.WriteAllLinesAsync($"../../../microservice-report{DateTime.UtcNow.ToString().Replace(':','-')}.txt", microserviceReport.Split('\n'));

var monolithReport = monolithTester.Report();
Console.WriteLine(monolithReport);
File.WriteAllLinesAsync($"../../../monolith-report{DateTime.UtcNow.ToString().Replace(':','-')}.txt", monolithReport.Split('\n'));