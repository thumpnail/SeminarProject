# Analyse der Benchmark- und Test-Reports im Chat.Tests-Ordner

## Einleitung

Im Rahmen des Seminarprojekts wurden verschiedene Architekturansätze für einen Chat-Service (Microservice vs. Monolith) getestet und deren Performance mittels automatisierter Benchmarks dokumentiert. Die Ergebnisse liegen in Form von Report-Dateien im Ordner `Chat.Tests` vor. Ziel dieses Berichts ist es, die wichtigsten Erkenntnisse aus den Reports wissenschaftlich zu analysieren und die zugrundeliegenden Implementierungen zu betrachten.

## Methodik

Die Reports wurden automatisiert generiert und enthalten tabellarische Auswertungen zu verschiedenen Endpunkten (`/room`, `/history`, `/send`) und Metriken wie durchschnittliche, minimale und maximale Dauer pro Request sowie Speicherverbrauch. Die Tests wurden sowohl für die Microservice- als auch für die Monolith-Architektur durchgeführt. Die Analyse basiert auf den Dateien `combine-report-*`, `microservice-report-*`, `monolith-report-*` und `final-report_*`.

## Ergebnisse

### Performance-Vergleich

- **Microservice-Architektur:**
  - Geringere durchschnittliche und minimale Request-Dauer (z.B. `/room` ca. 11 ms, `/history` ca. 12 ms).
  - Höhere maximale Dauer bei einzelnen Requests, was auf Ausreißer oder Lastspitzen hindeuten kann.
  - Speicherverbrauch laut Einzelreports meist 0 MB, im final-report jedoch hohe Werte (vermutlich kumuliert).

- **Monolith-Architektur:**
  - Höhere durchschnittliche und minimale Request-Dauer (z.B. `/room` ca. 21 ms, `/history` ca. 15 ms).
  - Maximalwerte sind teils stabiler und weniger stark ausgeprägt als beim Microservice.
  - Speicherverbrauch ebenfalls hoch im final-report.

### Beispielhafte Report-Auszüge

**combine-report-0_22.08.2025 20-40-48.txt**
```
-- Benchmark Summary --
│ ServiceType             │ Duration   │
│ ChatMicroserviceATester │ 59,27 s    │
│ ChatMonolithATester     │ 31,87 s    │
... SubReports zu Endpunkten ...
```

**microservice-report-0_22.08.2025 20-40-16.txt**
```
Endpoint: /room
  Avg Duration: 11,43 ms
Endpoint: /history
  Avg Duration: 12,05 ms
```

**monolith-report-0_22.08.2025 20-40-48.txt**
```
Endpoint: /room
  Avg Duration: 21,02 ms
Endpoint: /history
  Avg Duration: 15,63 ms
```

**final-report_22.08.2025 23-04-28.txt**
```
│ Endpoint                        │ ServerType   │ MinAvgDuration │ AvgAvgDuration │ MaxAvgDuration │
│ ChatMicroserviceATester/room    │ microservice │ 16,36 ms       │ 37,77 ms       │ 59,08 ms       │
│ ChatMonolithATester/room        │ monolith     │ 15,56 ms       │ 22,39 ms       │ 38,49 ms       │
... weitere Endpunkte ...
```

## Code-Analyse

Die Benchmarks werden durch die Klassen `ChatMicroserviceATester` und `ChatMonolithATester` im Ordner `Chat.Tests` durchgeführt. Die Auswertung und Report-Erstellung erfolgt über die Hilfsklassen `BenchmarkReport`, `BenchmarkTesterBase` und `ReportHelper`. Die Endpunkte werden dynamisch gemessen und die Ergebnisse in Listen von SubReports aggregiert.

### Beispiel: BenchmarkReport.cs
```csharp
// ...existing code...
public class BenchmarkReport {
    public Guid RunIndexIdentifier { get; set; }
    public int ThreadCount { get; set; }
    public int MsgCount { get; set; }
    public int ThreadThrottle { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string ServiceType { get; set; }
    public double Duration { get; set; }
    public List<BenchmarkSubReport> SubReports { get; set; }
}
// ...existing code...
```

### Beispiel: ChatMicroserviceATester.cs
```csharp
// ...existing code...
public class ChatMicroserviceATester : BenchmarkTesterBase {
    // ...Testlogik für Microservice-Endpunkte...
}
// ...existing code...
```

## Interpretation und Diskussion

Die Ergebnisse zeigen, dass die Microservice-Architektur in den Tests meist bessere Durchschnittswerte bei der Request-Dauer liefert. Dies ist typisch für entkoppelte, spezialisierte Services, die parallel und unabhängig voneinander arbeiten können. Die höheren Maximalwerte deuten jedoch auf mögliche Lastspitzen oder Synchronisationsprobleme hin. Der Monolith ist weniger flexibel, zeigt aber stabilere Maximalwerte, was auf eine konsistentere Verarbeitung hindeutet.

Die Speicherverbrauchswerte im final-report sind auffällig hoch und sollten weiter untersucht werden, da sie auf ineffiziente Speicherverwaltung oder kumulierte Messungen hindeuten könnten.

## Fazit

Die Reports bieten eine solide Grundlage für die wissenschaftliche Bewertung der beiden Architekturansätze. Die Microservice-Architektur ist performanter im Durchschnitt, der Monolith stabiler bei Ausreißern. Für eine produktive Umgebung empfiehlt sich eine weitere Optimierung und Analyse der Speicherverwaltung.

---
*Erstellt am 23.08.2025 von GitHub Copilot*

