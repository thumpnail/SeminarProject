# Architektur

## Monolith
- Ein einziges System mit allen Komponenten
- Einfaches Deployment
- Gemeinsame Datenbank

## Microservice
- Aufteilung in mehrere Services (Datenbank, Messaging, History)
- Skalierbarkeit und Flexibilität
- Kommunikation über Schnittstellen

## Vergleich
| Kriterium      | Monolith         | Microservice      |
|----------------|------------------|-------------------|
| Deployment     | Einfach          | Komplex           |
| Skalierbarkeit | Eingeschränkt    | Hoch              |
| Wartbarkeit    | Mittel           | Hoch              |
| Performance    | Direkt           | Abhängig von Netzwerk |

---

> Details zu Schnittstellen und Modellen siehe API-Referenz.
