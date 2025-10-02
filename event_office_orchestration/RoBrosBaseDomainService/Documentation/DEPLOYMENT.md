# Deployment Guide

Complete guide for deploying RoBrosBaseDomainService in various environments.

## Table of Contents

1. [As a NuGet Package](#as-a-nuget-package)
2. [Direct Project Reference](#direct-project-reference)
3. [Docker Deployment](#docker-deployment)
4. [Cloud Deployments](#cloud-deployments)
5. [Production Considerations](#production-considerations)

---

## As a NuGet Package

### Publishing to NuGet.org

```bash
# Build and pack
dotnet pack -c Release /p:Version=1.0.0

# Publish to NuGet (requires API key)
dotnet nuget push ./nupkgs/RoBrosBaseDomainService.1.0.0.nupkg \
    --api-key YOUR_API_KEY \
    --source https://api.nuget.org/v3/index.json
```

### Publishing to Private Feed

```bash
# Azure DevOps Artifacts
dotnet nuget push ./nupkgs/RoBrosBaseDomainService.1.0.0.nupkg \
    --api-key YOUR_PAT \
    --source https://pkgs.dev.azure.com/yourorg/_packaging/yourfeed/nuget/v3/index.json

# GitHub Packages
dotnet nuget push ./nupkgs/RoBrosBaseDomainService.1.0.0.nupkg \
    --api-key YOUR_GITHUB_TOKEN \
    --source https://nuget.pkg.github.com/yourusername/index.json
```

### Consuming from NuGet

```bash
# Install in your service
dotnet add package RoBrosBaseDomainService --version 1.0.0
```

```csharp
// Program.cs
using RoBrosBaseDomainService.Extensions;

builder.Services.AddRoBrosJournalService(
    builder.Configuration.GetConnectionString("DefaultConnection")!
);
```

---

## Direct Project Reference

For monorepo or local development:

```xml
<!-- YourService.csproj -->
<ItemGroup>
  <ProjectReference Include="../RoBrosBaseDomainService/RoBrosBaseDomainService.csproj" />
</ItemGroup>
```

```bash
# Build both projects
dotnet build

# Or use solution file
dotnet sln add RoBrosBaseDomainService/RoBrosBaseDomainService.csproj
dotnet sln add YourService/YourService.csproj
dotnet build
```

---

## Docker Deployment

### Option 1: Standalone Service (for testing)

```bash
# Build image
docker build -t robros-journal:1.0.0 .

# Run with PostgreSQL
docker-compose up -d
```

### Option 2: Integrate into Your Service

```dockerfile
# Your service's Dockerfile
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy both projects
COPY ["YourService/YourService.csproj", "YourService/"]
COPY ["RoBrosBaseDomainService/RoBrosBaseDomainService.csproj", "RoBrosBaseDomainService/"]

RUN dotnet restore "YourService/YourService.csproj"

COPY . .
WORKDIR "/src/YourService"
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "YourService.dll"]
```

### Option 3: Multi-Service Docker Compose

```yaml
# docker-compose.yml
version: '3.8'

services:
  postgres:
    image: postgres:16-alpine
    environment:
      POSTGRES_DB: myapp_db
      POSTGRES_USER: myapp_user
      POSTGRES_PASSWORD: ${DB_PASSWORD}
    volumes:
      - postgres_data:/var/lib/postgresql/data
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U myapp_user"]
      interval: 10s
      timeout: 5s
      retries: 5

  your-api:
    build:
      context: .
      dockerfile: YourService/Dockerfile
    depends_on:
      postgres:
        condition: service_healthy
    environment:
      - ConnectionStrings__DefaultConnection=Host=postgres;Database=myapp_db;Username=myapp_user;Password=${DB_PASSWORD}
      - ASPNETCORE_URLS=http://+:80
    ports:
      - "8080:80"
    restart: unless-stopped

volumes:
  postgres_data:
```

---

## Cloud Deployments

### AWS ECS/Fargate

**1. Build and push to ECR:**

```bash
# Login to ECR
aws ecr get-login-password --region us-east-1 | \
    docker login --username AWS --password-stdin YOUR_ACCOUNT.dkr.ecr.us-east-1.amazonaws.com

# Build and tag
docker build -t your-service:latest .
docker tag your-service:latest YOUR_ACCOUNT.dkr.ecr.us-east-1.amazonaws.com/your-service:latest

# Push
docker push YOUR_ACCOUNT.dkr.ecr.us-east-1.amazonaws.com/your-service:latest
```

**2. Task Definition (task-definition.json):**

```json
{
  "family": "your-service",
  "networkMode": "awsvpc",
  "requiresCompatibilities": ["FARGATE"],
  "cpu": "512",
  "memory": "1024",
  "containerDefinitions": [
    {
      "name": "your-service",
      "image": "YOUR_ACCOUNT.dkr.ecr.us-east-1.amazonaws.com/your-service:latest",
      "portMappings": [
        {
          "containerPort": 80,
          "protocol": "tcp"
        }
      ],
      "environment": [
        {
          "name": "ASPNETCORE_ENVIRONMENT",
          "value": "Production"
        }
      ],
      "secrets": [
        {
          "name": "ConnectionStrings__DefaultConnection",
          "valueFrom": "arn:aws:secretsmanager:us-east-1:ACCOUNT:secret:db-connection"
        }
      ],
      "logConfiguration": {
        "logDriver": "awslogs",
        "options": {
          "awslogs-group": "/ecs/your-service",
          "awslogs-region": "us-east-1",
          "awslogs-stream-prefix": "ecs"
        }
      }
    }
  ]
}
```

**3. Use RDS PostgreSQL:**

```bash
# Connection string from RDS
Host=your-db.xxxxx.us-east-1.rds.amazonaws.com;Database=myapp;Username=admin;Password=xxx;SSL Mode=Require
```

### Azure Container Apps

**1. Create Azure resources:**

```bash
# Create resource group
az group create --name rg-yourservice --location eastus

# Create Container Registry
az acr create --resource-group rg-yourservice \
    --name yourserviceacr --sku Basic

# Create PostgreSQL
az postgres flexible-server create \
    --resource-group rg-yourservice \
    --name yourservice-db \
    --location eastus \
    --admin-user dbadmin \
    --admin-password YourPassword123! \
    --sku-name Standard_B1ms \
    --version 16

# Create database
az postgres flexible-server db create \
    --resource-group rg-yourservice \
    --server-name yourservice-db \
    --database-name myapp_db
```

**2. Build and push:**

```bash
# Login to ACR
az acr login --name yourserviceacr

# Build and push
az acr build --registry yourserviceacr \
    --image your-service:latest .
```

**3. Deploy Container App:**

```bash
# Create Container App Environment
az containerapp env create \
    --name yourservice-env \
    --resource-group rg-yourservice \
    --location eastus

# Deploy app
az containerapp create \
    --name your-service \
    --resource-group rg-yourservice \
    --environment yourservice-env \
    --image yourserviceacr.azurecr.io/your-service:latest \
    --target-port 80 \
    --ingress external \
    --registry-server yourserviceacr.azurecr.io \
    --secrets db-conn="Host=yourservice-db.postgres.database.azure.com;Database=myapp_db;Username=dbadmin;Password=YourPassword123!;SSL Mode=Require" \
    --env-vars "ConnectionStrings__DefaultConnection=secretref:db-conn"
```

### Google Cloud Run

**1. Build and push to GCR:**

```bash
# Configure Docker for GCR
gcloud auth configure-docker

# Build
gcloud builds submit --tag gcr.io/PROJECT_ID/your-service:latest

# Or use Cloud Build
gcloud builds submit --config cloudbuild.yaml
```

**cloudbuild.yaml:**

```yaml
steps:
  - name: 'gcr.io/cloud-builders/docker'
    args: ['build', '-t', 'gcr.io/$PROJECT_ID/your-service:latest', '.']
images:
  - 'gcr.io/$PROJECT_ID/your-service:latest'
```

**2. Deploy to Cloud Run:**

```bash
# Create Cloud SQL PostgreSQL instance
gcloud sql instances create yourservice-db \
    --database-version=POSTGRES_16 \
    --tier=db-f1-micro \
    --region=us-central1

# Create database
gcloud sql databases create myapp_db --instance=yourservice-db

# Deploy Cloud Run service
gcloud run deploy your-service \
    --image gcr.io/PROJECT_ID/your-service:latest \
    --platform managed \
    --region us-central1 \
    --allow-unauthenticated \
    --add-cloudsql-instances PROJECT_ID:us-central1:yourservice-db \
    --set-env-vars "ConnectionStrings__DefaultConnection=Host=/cloudsql/PROJECT_ID:us-central1:yourservice-db;Database=myapp_db;Username=postgres;Password=xxx"
```

### Kubernetes (K8s)

**deployment.yaml:**

```yaml
apiVersion: v1
kind: Secret
metadata:
  name: db-connection
type: Opaque
stringData:
  connectionString: "Host=postgres-service;Database=myapp_db;Username=admin;Password=yourpassword"
---
apiVersion: apps/v1
kind: Deployment
metadata:
  name: your-service
spec:
  replicas: 3
  selector:
    matchLabels:
      app: your-service
  template:
    metadata:
      labels:
        app: your-service
    spec:
      containers:
      - name: your-service
        image: your-registry/your-service:latest
        ports:
        - containerPort: 80
        env:
        - name: ASPNETCORE_ENVIRONMENT
          value: "Production"
        - name: ConnectionStrings__DefaultConnection
          valueFrom:
            secretKeyRef:
              name: db-connection
              key: connectionString
        resources:
          requests:
            memory: "256Mi"
            cpu: "250m"
          limits:
            memory: "512Mi"
            cpu: "500m"
        livenessProbe:
          httpGet:
            path: /health
            port: 80
          initialDelaySeconds: 30
          periodSeconds: 10
        readinessProbe:
          httpGet:
            path: /health/ready
            port: 80
          initialDelaySeconds: 5
          periodSeconds: 5
---
apiVersion: v1
kind: Service
metadata:
  name: your-service
spec:
  selector:
    app: your-service
  ports:
  - port: 80
    targetPort: 80
  type: LoadBalancer
```

**Deploy:**

```bash
kubectl apply -f deployment.yaml
```

---

## Production Considerations

### 1. Database Configuration

**Connection Pooling:**

```csharp
services.AddRoBrosJournalService(connectionString, channelCapacity: 5000);

// In connection string
"Host=db;Database=myapp;Username=user;Password=pass;Pooling=true;Maximum Pool Size=100;Connection Lifetime=0"
```

**High Availability:**

```csharp
// Use read replicas for queries
services.AddDbContext<JournalDbContext>(options =>
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorCodesToAdd: null);
        npgsqlOptions.CommandTimeout(30);
    }));
```

### 2. Performance Tuning

**Channel Capacity:**

```csharp
// Low traffic: 1000 (default)
// Medium traffic: 5000
// High traffic: 10000+
services.AddRoBrosJournalService(connectionString, channelCapacity: 10000);
```

**Database Indexes:**

```sql
-- Additional custom indexes for your query patterns
CREATE INDEX idx_entity_journals_custom 
    ON entity_journals(entity_type, updated_at) 
    WHERE entity_type = 'YourFrequentType';

-- Partial index for recent data
CREATE INDEX idx_entity_journals_recent 
    ON entity_journals(entity_id, created_at) 
    WHERE created_at > NOW() - INTERVAL '30 days';
```

### 3. Monitoring

**Add health checks:**

```csharp
builder.Services.AddHealthChecks()
    .AddDbContextCheck<JournalDbContext>("journal_db");

app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready");
```

**Logging configuration:**

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "RoBrosBaseDomainService": "Information",
      "Microsoft.EntityFrameworkCore.Database.Command": "Warning"
    }
  }
}
```

**Application Insights (Azure):**

```csharp
builder.Services.AddApplicationInsightsTelemetry(
    builder.Configuration["ApplicationInsights:ConnectionString"]);
```

### 4. Security

**Secrets Management:**

```bash
# AWS Secrets Manager
aws secretsmanager create-secret \
    --name /myapp/db-connection \
    --secret-string "Host=xxx;Database=xxx;Username=xxx;Password=xxx"

# Azure Key Vault
az keyvault secret set \
    --vault-name myvault \
    --name db-connection \
    --value "Host=xxx;Database=xxx;Username=xxx;Password=xxx"

# Google Secret Manager
echo -n "Host=xxx;Database=xxx;Username=xxx;Password=xxx" | \
    gcloud secrets create db-connection --data-file=-
```

**Use managed identities:**

```csharp
// Azure Managed Identity
builder.Configuration.AddAzureKeyVault(
    new Uri($"https://{keyVaultName}.vault.azure.net/"),
    new DefaultAzureCredential());

// AWS IAM for RDS
"Host=mydb.rds.amazonaws.com;Database=myapp;Username=admin;Password=;IAM Authentication=true"
```

### 5. Backup Strategy

**PostgreSQL backups:**

```bash
# Automated backups (daily)
pg_dump -h localhost -U dbuser -d myapp_db > backup_$(date +%Y%m%d).sql

# Backup only journal tables
pg_dump -h localhost -U dbuser -d myapp_db -t entity_journals > journal_backup.sql

# Restore
psql -h localhost -U dbuser -d myapp_db < backup_20250930.sql
```

**Cloud-managed backups:**

- **AWS RDS**: Automated backups with 7-35 day retention
- **Azure PostgreSQL**: Automated backups with 7-35 day retention
- **GCP Cloud SQL**: Automated backups with 7-365 day retention

### 6. Scaling Considerations

**Horizontal Scaling:**

The service supports multiple instances since:
- Database handles concurrency
- Background service in each instance processes independently
- Channel is per-instance (not shared)

**Vertical Scaling:**

```yaml
# Kubernetes resources
resources:
  requests:
    memory: "512Mi"
    cpu: "500m"
  limits:
    memory: "2Gi"
    cpu: "2000m"
```

**Database Scaling:**

- Read replicas for query operations
- Connection pooling (configured above)
- Partitioning for large datasets (future enhancement)

### 7. Disaster Recovery

**RTO/RPO Strategy:**

```bash
# Point-in-time recovery
# AWS RDS: Restore to any point in retention period
aws rds restore-db-instance-to-point-in-time \
    --source-db-instance-identifier mydb \
    --target-db-instance-identifier mydb-restore \
    --restore-time 2025-09-30T12:00:00Z

# Azure: Similar capability
az postgres flexible-server restore \
    --resource-group rg \
    --name mydb-restore \
    --source-server mydb \
    --restore-time "2025-09-30T12:00:00Z"
```

### 8. Cost Optimization

**Archive old data:**

```sql
-- Move old entries to archive table
CREATE TABLE entity_journals_archive (LIKE entity_journals INCLUDING ALL);

-- Archive entries older than 1 year
INSERT INTO entity_journals_archive 
SELECT * FROM entity_journals 
WHERE created_at < NOW() - INTERVAL '1 year';

DELETE FROM entity_journals 
WHERE created_at < NOW() - INTERVAL '1 year';
```

**Use cheaper storage tiers:**

- AWS: Move to S3 Glacier after archiving
- Azure: Use Cool/Archive storage tiers
- GCP: Use Nearline/Coldline storage

---

## Deployment Checklist

- [ ] Environment variables configured
- [ ] Database connection string secured
- [ ] Migrations applied
- [ ] Health checks enabled
- [ ] Logging configured
- [ ] Monitoring set up
- [ ] Backups scheduled
- [ ] Security scanned (container/dependencies)
- [ ] Load testing completed
- [ ] Rollback plan documented
- [ ] Documentation updated
- [ ] Team notified

---

## Troubleshooting Production Issues

### High Memory Usage

```bash
# Check channel backlog
# Add metrics to AsyncJournalService
private int _queueDepth;
await _channel.Writer.WriteAsync(workItem, cancellationToken);
Interlocked.Increment(ref _queueDepth);
```

### Slow Journal Writes

```sql
-- Check for missing indexes
SELECT schemaname, tablename, indexname, idx_scan
FROM pg_stat_user_indexes
WHERE schemaname = 'public' AND tablename = 'entity_journals'
ORDER BY idx_scan;

-- Check for lock contention
SELECT * FROM pg_stat_activity 
WHERE datname = 'myapp_db' AND state = 'active';
```

### Connection Pool Exhaustion

```csharp
// Increase pool size
"Maximum Pool Size=200;Timeout=30"

// Monitor with
services.AddDbContextPool<JournalDbContext>(options => ...);
```

---

## Version Upgrade Path

```bash
# v1.0.0 -> v1.1.0
# 1. Update package
dotnet add package RoBrosBaseDomainService --version 1.1.0

# 2. Run migrations
dotnet ef database update

# 3. Test in staging
# 4. Deploy to production with rolling update

# Rollback if needed
dotnet add package RoBrosBaseDomainService --version 1.0.0
dotnet ef database update
```