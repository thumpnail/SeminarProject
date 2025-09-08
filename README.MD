# Monolith vs. Microservices Chat – Benchmark & Analysis Platform

> Comparative study platform measuring performance characteristics of a chat application implemented as a Monolith and as decomposed Microservices.

![Status](https://img.shields.io/badge/status-active-brightgreen.svg)
![.NET](https://img.shields.io/badge/.NET-8.0+-512BD4.svg?logo=dotnet&logoColor=white)
![Architecture](https://img.shields.io/badge/architectures-monolith%20%7C%20microservices-blueviolet)
![Benchmarks](https://img.shields.io/badge/benchmarks-automated-orange)
![Tests](https://img.shields.io/badge/tests-included-blue)
<!-- TODO: Add CI badge / License badge once available -->

---

## Contents
1. Motivation & Scope
2. Key Capabilities
3. Architecture Summary
4. Tech Stack
5. Project Layout
6. Quick Start
7. Running the Benchmark Suite
8. Configuration (`TestConfig.yaml`)
9. Generated Reports & Plots
10. Data Model & Metrics
11. Environment & Logging
12. Extending / Contribution
13. Roadmap
14. License
15. Acknowledgements

---

## 1. Motivation & Scope
This repository provides an executable comparison between a single-process chat application (Monolith) and a service‑oriented variant (Microservices). It focuses on: latency distribution, request throughput, per-endpoint timing, and qualitative operational differences. Output artifacts are reproducible and versionable.

## 2. Key Capabilities
* Dual runnable architectures (shared contracts in `Chat.Common`)
* Automated multi‑iteration benchmark harness (`Chat.Tests`)
* Structured persistence of timing data + per-endpoint aggregation
* Report generation (per run, combined, final) + PNG plots (OxyPlot)
* Tagging of operations (`BenchmarkTag`) to correlate timings with memory snapshots
* Swappable storage implementation (see database info capture in testers)

## 3. Architecture Summary
### Monolith
* Single deployment target (server + lightweight client)
* Simpler startup & lower coordination overhead
* Direct in-process interactions

### Microservices
* Separate concerns (Messaging / History / Database / Gateway layering)
* Network boundaries expose latency & resilience factors
* Enables selective horizontal scaling & failure isolation

Diagrams: See `ClassDiagram.png`, `MicroserviceClassDiagram.puml`, `MonolithClassDiagram.puml` (and `Dokumentation/Architecture.png`).

## 4. Tech Stack
| Domain | Tech |
|--------|------|
| Runtime | .NET 9 (C#) |
| Reporting / Charts | OxyPlot (PNG export) |
| Serialization | JSON + (MsgPack helper placeholders) |
| Persistence (benchmark db) | LiteDB / In-memory abstraction |
| Config | YAML (`TestConfig.yaml`) |
| Logging | Simple file logger (opt-out via env var) |

## 5. Project Layout
| Path | Purpose |
|------|---------|
| `Chat.Common` | Models (`Message`, `User`, `BenchmarkReport`), contracts & utilities | 
| `Chat.Tests` | Benchmark + functional test harness, report + plot generation |
| `Microservice` | Service implementations (Messaging, History, Database, etc.) |
| `Monolith` | Monolithic server + client |
| `Reports` | Auto‑generated run artifacts (txt + PNG) |
| `TestConfig.yaml` | Benchmark parameterization |
| `Dokumentation/` | German documentation & architecture assets |

## 6. Quick Start
### Prerequisites
* .NET 9 SDK
* (Optional) Docker for containerization experiments

### Clone & Build
```powershell
git clone <repo-url>
cd SeminarProject
dotnet build
```

### Run Monolith (example)
```powershell
cd Monolith
dotnet run --project ChatApp.Server
# (Optional) client shell
dotnet run --project ChatApp.Client
```

### Run Microservices (manually)
```powershell
cd Microservice
dotnet run --project ChatMessagingService
dotnet run --project ChatHistoryService
dotnet run --project ChatDatabaseService
# (If present) gateway / API edge
```

Add a `docker-compose.yml` to orchestrate these for reproducible network conditions (future enhancement).

## 7. Running the Benchmark Suite
Benchmarks execute through `Chat.Tests` and spawn architecture‑specific testers (`ChatMicroserviceATester`, `ChatMonolithATester`) derived from `BenchmarkTesterBase`.

```powershell
cd Chat.Tests
dotnet run
```

Workflow performed per iteration:
1. Spawn testers (monolith + microservice) with configured thread/message counts
2. Threads issue: room acquisition -> message sends -> history retrieval
3. Raw request records stored (endpoint, duration, sender/receiver, tag)
4. Per-endpoint aggregation -> `BenchmarkReport` persisted
5. Combined + final summaries & plots exported to `Reports/<timestamp>/`

## 8. Configuration (`TestConfig.yaml`)
Example (current default):
```yaml
MAX_ITERATIONS: 5
ITERATION_THROTTLE: 0
MAX_THREADS: 10
MAX_MESSAGES: 25
THREAD_THROTTLE: 0
```
Meaning:
* MAX_ITERATIONS – how many benchmark passes (produces series & final report)
* MAX_THREADS – concurrent simulated users per tester
* MAX_MESSAGES – messages per thread
* THREAD_THROTTLE – ms delay between starting worker threads
* ITERATION_THROTTLE – delay between iterations

Tune to shape load curves (e.g. ramp concurrency, stress endpoints). Keep an eye on resource saturation when scaling up.

## 9. Generated Reports & Plots
Artifacts per run folder (`Reports/<runTimestamp>/`):
* `microservice-*-report-<i>_<iterationTime>.txt`
* `monolith-*-report-<i>_<iterationTime>.txt`
* `combine-report-...txt` (pairwise comparison for iteration)
* `final-report-_<mainRunTime>_micro_*_mono_*.txt` (aggregate across iterations)
* `final-plot-*.png` (OxyPlot charts; endpoint latency & throughput comparisons)

Each report includes:
* Thread & message configuration
* Timing window (start / end / duration seconds)
* Per-endpoint: count, avg, min, max (ms)
* Invalid HTTP status occurrences (`InvalidStatusCodeCount`)

## 10. Data Model & Metrics
`BenchmarkReport` structure (selected fields):
* RunIndexIdentifier
* ThreadCount / MsgCount / ThreadThrottle
* Duration (seconds)
* InvalidStatusCodeCount
* SubReports[]: per-endpoint aggregates (Count, AvgDurationMs, MinDurationMs, MaxDurationMs)
* DataList: raw filtered rows for the service type

Contracts (`Chat.Common.Contracts`) define message send / history / room operations and attach a `BenchmarkTag` (with timestamp + memory snapshot fields) for potential deeper analysis.

## 11. Environment & Logging
File logging is enabled unless environment variable `DISABLE_FILE_LOG=1` is set. Logs written near project root: `log_<Origin>.log`.

Planned enhancements:
* Structured (JSON) log emission
* Optional log level filtering

## 12. Extending / Contribution
Ideas:
* Add persistence abstraction benchmarks (swap LiteDB / SQLite / PostgreSQL)
* Inject artificial latency / failure for resilience scenarios
* CI pipeline executing reduced iteration config
* Export CSV / parquet for external analytics

Contribution flow:
1. Fork / branch
2. Change + add tests
3. Update docs / diagrams
4. PR with concise rationale

## 13. Roadmap
| Stage | Item |
|-------|------|
| Short | Add license, CI workflow, Docker orchestration |
| Short | CSV export module (`Chat.Reporter`) activation |
| Mid | Latency distribution histograms & percentile charts |
| Mid | Fault injection / chaos toggles |
| Long | Web dashboard for historical run comparison |

## 14. License
Currently UNLICENSED. Select and add (MIT / Apache-2.0 / BSD-3) to enable broader usage. Replace badge after adding `LICENSE`.

## 15. Acknowledgements
* Seminar / academic evaluation context
* .NET & open-source ecosystem
* Future contributors & reviewers

---
Multilingual note: German documentation retained in `Dokumentation/`. A German README (`README.de.md`) may be added later.

---
Questions or improvement proposals welcome – open an issue.
