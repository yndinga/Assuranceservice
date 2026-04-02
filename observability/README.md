# Observabilité (stack partagée) — Grafana / Prometheus / Loki / Tempo / OTel Collector

Cette stack est pensée pour **tous** les microservices (mix .NET / Node / Java / etc.) avec **Docker Compose** (Portainer).

## Services inclus

- **Grafana**: dashboards + alerting
- **Prometheus**: metrics (scrape)
- **Loki**: logs
- **Promtail**: collecte des logs Docker/host vers Loki
- **Tempo**: traces
- **OpenTelemetry Collector**: point d’entrée OTLP (4317 gRPC / 4318 HTTP) pour les traces + (optionnel) metrics/logs

## Démarrage (Portainer / Docker Compose)

1. Déployer le stack `observability/docker-compose.observability.yml`
2. Ouvrir Grafana: `http://localhost:3000`
   - login: `admin`
   - password: `admin` (à changer)

## Brancher un microservice (n’importe quel langage)

### Traces (OTLP → Collector)

Dans **chaque** microservice (conteneur) :

- **OTLP gRPC** (recommandé):
  - `OTEL_EXPORTER_OTLP_ENDPOINT=http://otel-collector:4317`
- ou **OTLP HTTP**:
  - `OTEL_EXPORTER_OTLP_ENDPOINT=http://otel-collector:4318`

Identité du service (important pour filtrer) :

- `OTEL_SERVICE_NAME=assurance-service` (un nom par microservice)
- `OTEL_RESOURCE_ATTRIBUTES=deployment.environment=dev,service.version=1.0.0`

### Metrics

Deux options (tu peux faire les deux) :

- **Option A (simple, très courant)**: chaque service expose `/metrics` (Prometheus scrape)
  - ajoute un `job` dans `prometheus.yml`
- **Option B**: exporter des metrics OpenTelemetry vers le Collector (selon langage/lib)

### Logs

Si tes microservices loggent sur **stdout/stderr** (le plus courant en Docker), Promtail les récupère et les envoie à Loki.

## Ajouter un microservice à Prometheus

Dans `observability/prometheus/prometheus.yml`, ajouter un bloc:

```yaml
  - job_name: "assurance-service"
    metrics_path: /metrics
    static_configs:
      - targets: ["assurance-api:8080"]
```

Puis redéployer le stack.

