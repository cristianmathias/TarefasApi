# 🔍 Guia de Testes - Observabilidade da TarefasAPI

## 🎯 Como Testar as Funcionalidades de Observabilidade

### 📋 Pré-requisitos
```bash
# 1. Executar a aplicação
dotnet run --project Tarefas.Api

# 2. A aplicação estará disponível em:
# - HTTPS: https://localhost:7001
# - HTTP: http://localhost:5000
```

---

## 🏥 1. Health Checks

### 📍 Endpoints Disponíveis:

```bash
# Health Check Geral - Status de todos os serviços
GET https://localhost:7001/health

# Health Check Readiness - Pronto para receber tráfego
GET https://localhost:7001/health/ready

# Health Check Liveness - Aplicação está viva
GET https://localhost:7001/health/live

# Health Checks UI - Interface visual
GET https://localhost:7001/health-ui
```

### 🧪 Testando Health Checks:

```bash
# Via curl
curl -k https://localhost:7001/health
curl -k https://localhost:7001/health/ready
curl -k https://localhost:7001/health/live

# Via browser
# Acesse: https://localhost:7001/health-ui
```

### ✅ Resultados Esperados:

```json
// /health - Status 200 OK
{
  "status": "Healthy",
  "totalDuration": "00:00:00.0123456",
  "entries": {
    "application": {
      "status": "Healthy",
      "description": "Application is running"
    },
    "database": {
      "status": "Healthy", 
      "description": "Database is accessible"
    },
    "rate_limit": {
      "status": "Healthy",
      "description": "Rate limiting is operational"
    }
  }
}
```

---

## 📊 2. Métricas (Prometheus)

### 📍 Endpoint:
```bash
GET https://localhost:7001/metrics
```

### 🧪 Testando Métricas:

```bash
# Visualizar métricas Prometheus
curl -k https://localhost:7001/metrics

# Fazer algumas requisições para gerar métricas
curl -k https://localhost:7001/api/tarefas
curl -k https://localhost:7001/api/tarefas/1
```

### 📈 Métricas Importantes:

```prometheus
# Requisições HTTP
http_requests_total{method="GET",route="/api/tarefas",status_code="200"} 5

# Duração das requisições
http_request_duration_seconds_bucket{method="GET",route="/api/tarefas",le="0.1"} 4

# Tarefas criadas (métrica customizada)
tarefas_created_total 3

# Tarefas por status
tarefas_by_status{status="pendente"} 2
tarefas_by_status{status="concluida"} 1

# Rate limiting
rate_limit_requests_total{allowed="true"} 10
rate_limit_requests_total{allowed="false"} 0
```

---

## 📝 3. Logging Estruturado (Serilog)

### 🔧 Configuração Atual:
- **Console**: Logs coloridos e estruturados
- **File**: Arquivos em `./logs/` (desenvolvimento)
- **Structured**: JSON format para produção

### 🧪 Testando Logs:

```bash
# 1. Executar aplicação e observar logs no console
dotnet run --project Tarefas.Api

# 2. Fazer requisições para gerar logs
curl -k https://localhost:7001/api/tarefas

# 3. Verificar arquivos de log (se habilitado)
ls -la logs/
cat logs/tarefas-api-*.log
```

### 📋 Exemplos de Logs Estruturados:

```json
// Request Log
{
  "@t": "2024-12-30T10:30:45.123Z",
  "@l": "Information", 
  "@m": "HTTP GET /api/tarefas responded 200 in 45.67 ms",
  "RequestMethod": "GET",
  "RequestPath": "/api/tarefas",
  "StatusCode": 200,
  "Elapsed": 45.67,
  "RequestId": "0HMVN1234567890",
  "CorrelationId": "abc-def-123",
  "ClientIP": "127.0.0.1",
  "UserAgent": "curl/7.68.0"
}

// Business Logic Log
{
  "@t": "2024-12-30T10:30:45.150Z",
  "@l": "Information",
  "@m": "Tarefa criada com sucesso: {TarefaId}",
  "TarefaId": 123,
  "TarefaTitulo": "Nova Tarefa",
  "UserId": "user@example.com",
  "OperationDuration": 12.34
}
```

---

## 🔗 4. Tracing Distribuído (OpenTelemetry)

### 🧪 Testando Tracing:

```bash
# Fazer uma operação completa
curl -k -X POST https://localhost:7001/api/tarefas \
  -H "Content-Type: application/json" \
  -d '{"titulo":"Test Trace","descricao":"Testing tracing"}'

# Observar traces no console (Console Exporter ativo)
```

### 📋 Trace Example (Console Output):

```
Activity.TraceId:            80e1afed08e019fc1110464cfa66635c
Activity.SpanId:             7a085853722dc6d2  
Activity.TraceFlags:         Recorded
Activity.ActivitySourceName: TarefasAPI
Activity.DisplayName:        POST api/tarefas
Activity.Kind:               Server
Activity.StartTime:          2024-12-30T10:30:45.0000000Z
Activity.Duration:           00:00:00.0456789
Activity.Tags:
    http.method: POST
    http.route: api/tarefas
    http.status_code: 201
    custom.operation: CreateTarefa
    custom.tarefa_id: 123
Resource associated with Activity:
    service.name: TarefasAPI
    service.version: 1.0.0
    deployment.environment: Development
```

---

## 🧪 5. Testes Práticos de Cenários

### 🚀 Cenário 1: Aplicação Saudável
```bash
# 1. Iniciar aplicação
dotnet run --project Tarefas.Api

# 2. Verificar health checks
curl -k https://localhost:7001/health

# 3. Fazer algumas operações
curl -k https://localhost:7001/api/tarefas
curl -k -X POST https://localhost:7001/api/tarefas \
  -H "Content-Type: application/json" \
  -d '{"titulo":"Teste","descricao":"Teste observabilidade"}'

# 4. Verificar métricas
curl -k https://localhost:7001/metrics | grep tarefa
```

### ⚠️ Cenário 2: Rate Limiting
```bash
# Fazer muitas requisições rapidamente (>100/min)
for i in {1..150}; do
  curl -k https://localhost:7001/api/tarefas &
done

# Verificar logs de rate limiting
# Verificar métricas: rate_limit_requests_total{allowed="false"}
curl -k https://localhost:7001/metrics | grep rate_limit
```

### 🔧 Cenário 3: Erro de Aplicação
```bash
# Tentar acessar tarefa inexistente
curl -k https://localhost:7001/api/tarefas/99999

# Verificar:
# - Log de erro estruturado
# - Health check ainda healthy
# - Métricas de erro (http_requests_total{status_code="404"})
```

---

## 📊 6. Dashboards e Monitoramento

### 🔧 Configuração Local com Docker (Opcional):

```bash
# Criar docker-compose.yml para Grafana + Prometheus
version: '3.8'
services:
  prometheus:
    image: prom/prometheus
    ports:
      - "9090:9090"
    volumes:
      - ./prometheus.yml:/etc/prometheus/prometheus.yml
    
  grafana:
    image: grafana/grafana
    ports:
      - "3000:3000"
    environment:
      - GF_SECURITY_ADMIN_PASSWORD=admin

# Executar
docker-compose up -d
```

### 📋 Configuração Prometheus (prometheus.yml):
```yaml
global:
  scrape_interval: 15s

scrape_configs:
  - job_name: 'tarefas-api'
    static_configs:
      - targets: ['host.docker.internal:5000']
    metrics_path: '/metrics'
    scrape_interval: 5s
```

---

## 🎯 7. Validação de Observabilidade

### ✅ Checklist de Testes:

- [ ] **Health Checks**: Todos os endpoints respondem adequadamente
- [ ] **UI Dashboard**: /health-ui carrega e mostra status
- [ ] **Logs Estruturados**: Aparecem no console com contexto
- [ ] **Métricas Prometheus**: /metrics expõe dados corretos
- [ ] **Request Tracing**: Activities aparecem no console
- [ ] **Rate Limiting**: Métricas de limite funcionam
- [ ] **Error Handling**: Erros são logados e métricas atualizadas
- [ ] **Performance**: Overhead < 5% com observabilidade

### 🎛️ Comandos de Verificação:

```bash
# Verificar se todos os serviços estão rodando
curl -k https://localhost:7001/health

# Verificar métricas básicas
curl -k https://localhost:7001/metrics | grep -E "(http_requests|tarefa|rate_limit)"

# Verificar logs estruturados (devem aparecer no console)
# Fazer requisições e observar output colorido/estruturado

# Verificar UI de health checks
# Browser: https://localhost:7001/health-ui
```

---

## 🚨 8. Troubleshooting

### ❌ Problemas Comuns:

1. **Health UI não carrega**:
   - Verificar se não está em ambiente "Testing"
   - Verificar se HealthChecksUI está registrado

2. **Métricas não aparecem**:
   - Verificar se OpenTelemetry está configurado
   - Verificar endpoint /metrics

3. **Logs não estruturados**:
   - Verificar configuração Serilog
   - Verificar se está usando ILogger corretamente

4. **Rate limit não funciona**:
   - Verificar configuração no Program.cs
   - Verificar middleware order

### 🔧 Debug Commands:

```bash
# Verificar configuração
dotnet run --project Tarefas.Api --verbosity detailed

# Verificar dependências
dotnet list Tarefas.Api package | grep -E "(Serilog|OpenTelemetry|HealthChecks)"

# Verificar logs detalhados
export ASPNETCORE_ENVIRONMENT=Development
dotnet run --project Tarefas.Api
```

---

## 📖 Recursos Adicionais

- **Serilog**: https://serilog.net/
- **OpenTelemetry**: https://opentelemetry.io/docs/instrumentation/net/
- **Health Checks**: https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks
- **Prometheus**: https://prometheus.io/docs/guides/go-application/

---

*🔍 Guia criado em: 30/12/2024*  
*📊 Cobertura: Health Checks, Logging, Métricas, Tracing*  
*🎯 Status: Fase 3 - Observabilidade Completa*