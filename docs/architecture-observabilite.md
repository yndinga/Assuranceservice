# Architecture Observabilité – Microservices, Prometheus, Grafana, Splunk

Ce document décrit comment les microservices et les briques d’observabilité (Prometheus, Grafana, Splunk) sont interconnectés.

---

## 1. Vue d’ensemble (flux de données)

```
                    ┌─────────────────────────────────────────────────────────────────┐
                    │                     COUCHE APPLICATIONS                           │
                    │  ┌──────────────┐ ┌──────────────┐ ┌──────────────┐               │
                    │  │ Assurance    │ │ Declaration  │ │ Stock        │  ... autres   │
                    │  │ Service      │ │ Importation  │ │ Service      │   microservices
                    │  │ (API .NET)   │ │ Service      │ │ (API .NET)   │               │
                    │  └──────┬───────┘ └──────┬───────┘ └──────┬───────┘               │
                    │         │                │                │                        │
                    └─────────┼────────────────┼────────────────┼────────────────────────┘
                              │                │                │
         ┌────────────────────┼────────────────┼────────────────┼────────────────────┐
         │                    │    MÉTRIQUES   │                │                    │
         │                    ▼                ▼                ▼                    │
         │             ┌─────────────────────────────────────────────┐               │
         │             │  Exposition /metrics (OpenTelemetry ou       │               │
         │             │  AspNetCore.Diagnostics) sur chaque service  │               │
         │             └──────────────────────┬──────────────────────┘               │
         │                                      │ scrape (HTTP)                        │
         │                                      ▼                                      │
         │             ┌─────────────────────────────────────────────┐               │
         │             │              PROMETHEUS                      │               │
         │             │  (stockage séries temporelles, PromQL,       │               │
         │             │   règles d’alerting)                        │               │
         │             └──────────────────────┬──────────────────────┘               │
         │                                      │ requêtes (PromQL)                    │
         └──────────────────────────────────────┼─────────────────────────────────────┘
                                                │
         ┌──────────────────────────────────────┼─────────────────────────────────────┐
         │                    LOGS              │         TRACES                      │
         │  Chaque microservice                  │  Chaque microservice                 │
         │  (Serilog / ILogger)                  │  (OpenTelemetry)                     │
         │         │                             │         │                            │
         │         ▼ HEC (HTTP)                  │         ▼ OTLP (gRPC/HTTP)           │
         │  ┌─────────────┐                      │  ┌─────────────┐                    │
         │  │   SPLUNK    │                      │  │   Tempo /   │                    │
         │  │   (HEC)     │                      │  │   Jaeger /  │                    │
         │  │   Logs      │                      │  │   Splunk OTEL│                    │
         │  └──────┬──────┘                      │  └──────┬──────┘                    │
         │         │                             │         │                            │
         └─────────┼─────────────────────────────┼─────────┼───────────────────────────┘
                   │                             │         │
                   │      ┌──────────────────────┴─────────┴──────────────────────┐
                   │      │                    GRAFANA                             │
                   │      │  Datasources : Prometheus, Splunk (logs), Tempo/Jaeger │
                   │      │  Dashboards : métriques, logs, traces (corrélation)     │
                   │      └───────────────────────────────────────────────────────┘
                   │
                   └──────────────► (optionnel) Grafana interroge Splunk pour les logs
```

---

## 2. Schéma détaillé des interconnexions (Mermaid)

```mermaid
flowchart TB
    subgraph Apps["Applications (microservices .NET)"]
        A1[AssuranceService]
        A2[DeclarationImportationService]
        A3[StockService]
        A4[FacturationService]
        A5[PaiementService]
        A6[ApiGateway]
    end

    subgraph Metrics["Métriques"]
        A1 -->|expose /metrics| M1
        A2 -->|expose /metrics| M1
        A3 -->|expose /metrics| M1
        A4 -->|expose /metrics| M1
        A5 -->|expose /metrics| M1
        A6 -->|expose /metrics| M1
        M1[Endpoint /metrics<br>par service]
    end

    subgraph Logs["Logs"]
        A1 -->|Serilog → HEC| L1
        A2 -->|Serilog → HEC| L1
        A3 -->|Serilog → HEC| L1
        A4 -->|Serilog → HEC| L1
        A5 -->|Serilog → HEC| L1
        A6 -->|Serilog → HEC| L1
        L1[Splunk HEC<br>HTTP Event Collector]
    end

    subgraph Traces["Traces"]
        A1 -->|OTLP| T1
        A2 -->|OTLP| T1
        A3 -->|OTLP| T1
        A4 -->|OTLP| T1
        A5 -->|OTLP| T1
        A6 -->|OTLP| T1
        T1[Collecteur OTLP<br>Tempo / Jaeger / Splunk OTel]
    end

    PROM[Prometheus]
    PROM -->|scrape périodique| M1

    SPLUNK[(Splunk<br>Logs + index)]
    L1 --> SPLUNK

    TEMPO[(Tempo / Jaeger<br>ou Splunk APM)]
    T1 --> TEMPO

    GRAFANA[Grafana]
    GRAFANA -->|PromQL| PROM
    GRAFANA -->|datasource logs| SPLUNK
    GRAFANA -->|datasource traces| TEMPO
```

---

## 3. Séquence des flux (qui envoie quoi à qui)

```mermaid
sequenceDiagram
    participant MS as Microservice
    participant Prom as Prometheus
    participant Splunk as Splunk (HEC)
    participant OTLP as Collecteur OTLP
    participant Grafana as Grafana

    Note over MS: Métriques
    Prom->>MS: GET /metrics (scrape)
    MS-->>Prom: exposition des métriques

    Note over MS: Logs
    MS->>Splunk: POST /services/collector/event (HEC token)
    Splunk-->>MS: 200 OK

    Note over MS: Traces
    MS->>OTLP: Export spans (OTLP gRPC/HTTP)
    OTLP-->>MS: OK

    Note over Grafana: Visualisation
    Grafana->>Prom: Requêtes PromQL
    Grafana->>Splunk: Requêtes logs (Splunk DSL)
    Grafana->>OTLP/Backend: Requêtes traces
```

---

## 4. Récapitulatif des connexions

| De                    | Vers              | Protocole / Mécanisme        | Données        |
|-----------------------|-------------------|-----------------------------|----------------|
| Microservices         | Prometheus        | HTTP scrape (Prometheus tire) | Métriques      |
| Microservices         | Splunk            | HTTP (HEC) – push            | Logs           |
| Microservices         | Collecteur OTLP   | OTLP (gRPC ou HTTP) – push  | Traces (spans) |
| Grafana               | Prometheus        | HTTP (PromQL)               | Lecture métriques |
| Grafana               | Splunk            | API / datasource Splunk     | Lecture logs   |
| Grafana               | Backend traces    | API Tempo/Jaeger/Splunk     | Lecture traces |

---

## 5. Déploiement logique (containers / hosts)

```mermaid
flowchart LR
    subgraph Cluster["Cluster / Réseau"]
        subgraph Nodes["Nœuds applicatifs"]
            A[Microservices]
        end
        subgraph Monitoring["Stack monitoring"]
            P[Prometheus]
            G[Grafana]
        end
        subgraph Logging["Logging"]
            S[Splunk / HEC]
        end
        subgraph Tracing["Tracing"]
            O[OTLP Collector +<br>Tempo ou Splunk]
        end
    end

    A -->|metrics| P
    A -->|logs HEC| S
    A -->|OTLP| O
    G --> P
    G --> S
    G --> O
```

---

## 6. Résumé

- **Microservices** : exposent `/metrics`, envoient les **logs** vers **Splunk (HEC)** et les **traces** vers un **collecteur OTLP** (Tempo, Jaeger ou Splunk).
- **Prometheus** : tire les métriques (scrape) depuis les microservices, stocke et sert les séries pour **Grafana** et l’alerting.
- **Splunk** : reçoit les logs via HEC ; peut aussi recevoir traces/métriques si vous utilisez Splunk Observability / OTel.
- **Grafana** : se connecte à **Prometheus** (métriques), **Splunk** (logs) et au backend de **traces** (Tempo/Jaeger/Splunk), pour des dashboards et une corrélation logs / traces / métriques.

Ainsi, les éléments sont interconnectés de façon claire et exploitable en production.
