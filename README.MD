# Seminar Project – Monolith vs. Microservices Chat Architecture

> Comparative study and benchmark framework for evaluating monolithic vs. microservice-based chat system architectures.

<!-- Badges (replace placeholders as needed) -->
![Status](https://img.shields.io/badge/status-active-brightgreen.svg)
![.NET](https://img.shields.io/badge/.NET-8.0+-512BD4.svg?logo=dotnet&logoColor=white)
![Docker](https://img.shields.io/badge/Docker-ready-2496ED?logo=docker&logoColor=white)
![Tests](https://img.shields.io/badge/tests-available-blue)
<!-- Add a real license badge once a LICENSE file is added -->

---

## Table of Contents
1. Overview
2. Project Goals
3. Features
4. Architecture Overview
5. Tech Stack
6. Repository Structure
7. Quick Start
8. Running Benchmarks & Tests
9. Reports & Analysis
10. Development Workflow
11. Contributing
12. Roadmap
13. Project Status
14. License
15. Acknowledgements

---

## 1. Overview
This repository hosts a comparative analysis of two architectural approaches for a chat application: a monolithic implementation and a microservice-based implementation. It includes automated benchmarks, reporting utilities, and supporting tooling to evaluate performance, scalability, and maintainability characteristics.

## 2. Project Goals
- Provide reproducible benchmarks for Monolith vs. Microservices in a chat context
- Evaluate latency, throughput, resource usage, and horizontal scaling behavior
- Automate data collection and report generation
- Offer a foundation for academic or practical architecture evaluation

## 3. Features
- Dual implementation: Monolith & Microservice chat systems
- Automated performance & functional test harness (Chat.Tests)
- Benchmark data aggregation & Markdown / file-based reporting
- Extensible shared model layer (Chat.Common)
- Optional tooling for CSV & graph-friendly exports

## 4. Architecture Overview
### Monolith
- Single deployable unit
- Shared database layer
- Lower operational complexity
- Faster local development

### Microservices
- Decomposed services (e.g. Messaging, Persistence/History, possibly Users)
- Independent scalability of components
- Explicit inter-service communication
- Higher operational overhead, more flexibility long-term

> You can add an architecture diagram (e.g. /docs/architecture.png) and reference it here if desired.

## 5. Tech Stack
- Language / Runtime: .NET (C#)
- Containerization: Docker / Docker Compose (optional)
- Scripting / Analysis: Python (for external tooling)
- Testing: Custom benchmark & functional suites (Chat.Tests)

## 6. Repository Structure
- `Chat.Common` – Shared models, interfaces, utilities
- `Chat.Reporter` – (Currently dormant) Intended for CSV export & reporting helpers
- `Chat.Tests` – Core benchmark + functional test runner & report generator
- `Microservice` – Microservice-based chat architecture implementation
- `Monolith` – Monolithic chat server + client
- `Reports` – Generated analysis outputs (Markdown, data files, etc.)
- `Tools` – Auxiliary scripts / data preparation utilities

## 7. Quick Start
### Prerequisites
- .NET SDK 8.0+
- (Optional) Docker & Docker Compose
- (Optional) Python 3.x for extended analysis scripts

### Clone
```
 git clone <repo-url>
 cd SeminarProject
```

### Build All
```
 dotnet build
```

### Run Monolith
```
 cd Monolith
 dotnet run --project Server
 # In another terminal
 dotnet run --project Client
```

### Run Microservices (Example)
Option A: Manual
```
 cd Microservice
 dotnet run --project Service.Messaging
 dotnet run --project Service.History
 dotnet run --project Gateway
```
Option B: Docker Compose (add a docker-compose.yml if not present)
```
 docker compose up --build
```

### Run Tests / Benchmarks
```
 cd Chat.Tests
 dotnet test
```
(If benchmarks are separated, provide specific commands here.)

### Generate Reports
Reports are emitted automatically into `Reports/` (Markdown + data files). Ensure the folder exists or is created on first run.

## 8. Running Benchmarks & Tests
Typical workflow:
1. Ensure both architectures are buildable.
2. Optionally start required services (for microservice mode).
3. Execute `dotnet test` inside `Chat.Tests`.
4. Inspect `Reports/` for generated summaries, raw metrics, or CSV exports (if enabled).

> Extend this section with concrete metric examples once stable.

## 9. Reports & Analysis
- Default output: Markdown summaries + structured result files
- (Planned) CSV exports for external plotting (Grafana, Python, etc.)
- Suggested next step: add charts (e.g. latency distributions, throughput trends) using a `/docs` or `/notebooks` directory

## 10. Development Workflow
- Standard Git branching (e.g. `main` + feature branches)
- Add tests for any benchmarking or architectural changes
- Keep shared contracts in `Chat.Common` stable; version if necessary
- Consider adding Git hooks or CI for automated benchmark runs (future enhancement)

## 11. Contributing
Contributions welcome. Suggested process:
1. Open an issue describing proposed change
2. Create feature branch
3. Add / update tests & documentation
4. Submit Pull Request with concise summary

## 12. Roadmap
- [ ] Finalize microservice decomposition boundaries
- [ ] Add architecture diagrams
- [ ] Implement / revive Reporter CSV export pipeline
- [ ] Introduce baseline latency & throughput charts
- [ ] Add CI pipeline (build + test + report artifact upload)
- [ ] Add configurable load profiles (users, message frequency)
- [ ] Integrate docker-compose for full microservice orchestration
- [ ] Optional: Add gRPC or message queue variant

## 13. Project Status
Actively evolving. Reporter component currently minimal / dormant; core benchmarking flows functional.

## 14. License
No license specified yet. Add a LICENSE file (e.g. MIT, Apache-2.0) and update badges.

## 15. Acknowledgements
- Academic / seminar context inspiration
- Open-source .NET ecosystem
- Future contributors

---

### Multilingual Note
An earlier German version existed; current canonical README is maintained in English for broader accessibility. You may add a `README.de.md` if bilingual support is desired.

---

Feel free to open issues for clarifications, ideas, or improvement proposals.
