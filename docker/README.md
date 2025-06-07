# Design16 Docker Infrastructure

This directory contains the unified Docker Compose configuration for the Design16 system, providing all required infrastructure services for both EntitiesManager and Processors.

## 🏗️ Architecture Overview

The Docker setup provides the following services:

- **MongoDB**: Document database for EntitiesManager data storage
- **RabbitMQ**: Message broker for asynchronous communication
- **Hazelcast**: Distributed cache for processor health monitoring and data caching
- **OpenTelemetry Collector**: Observability and metrics collection

## 📁 File Structure

```
docker/
├── docker-compose.yml           # Unified Docker Compose configuration
├── otel-collector-config.yaml   # OpenTelemetry Collector configuration
├── README.md                    # This documentation
└── test-endpoints.ps1           # PowerShell script for testing endpoints
```

## 🚀 Quick Start

### Prerequisites

- Docker Desktop installed and running
- Docker Compose v3.8 or higher

### Starting All Services

```bash
# Start all infrastructure services
docker-compose up -d

# View logs for all services
docker-compose logs -f

# View logs for specific service
docker-compose logs -f hazelcast
```

### Starting Specific Services

```bash
# Start only database and message broker
docker-compose up -d mongodb rabbitmq

# Start only caching services
docker-compose up -d hazelcast

# Start observability services
docker-compose up -d otel-collector
```

## 🔧 Service Configuration

### MongoDB
- **Port**: 27017
- **Database**: EntitiesManagerDb
- **Credentials**: admin/admin123
- **Connection String**: `mongodb://admin:admin123@localhost:27017/EntitiesManagerDb`

### RabbitMQ
- **AMQP Port**: 5672
- **Management UI**: http://localhost:15672
- **Credentials**: guest/guest
- **Virtual Host**: /

### Hazelcast
- **Member Port**: 5701
- **Management Center**: http://localhost:8080
- **Cluster Name**: EntitiesManager
- **Configuration**: Uses default Hazelcast configuration with environment variable overrides
- **Memory**: 512MB-1GB heap with optimized settings

### OpenTelemetry Collector
- **OTLP gRPC**: 4317
- **OTLP HTTP**: 4318
- **Prometheus Metrics**: http://localhost:8889/metrics
- **Health Check**: http://localhost:8888/metrics

## 🏃‍♂️ Running Applications

### EntitiesManager API (Local)
```bash
# Navigate to project root
cd ..

# Run EntitiesManager API
dotnet run --project src/Presentation/FlowOrchestrator.EntitiesManagers.Api/

# Available at:
# - Development: http://localhost:5130
# - Production: http://localhost:5000
```

### Processors (Local)
```bash
# Run FileProcessor v3.2.1
dotnet run --project Processors/Processor.File.v3.2.1/

# Run FileProcessor v1.9.9
dotnet run --project Processors/Processor.File/
```

## 🔍 Health Checks & Monitoring

### Service Health Endpoints

```bash
# MongoDB
mongosh --eval "db.adminCommand('ping')"

# RabbitMQ
curl http://localhost:15672/api/healthchecks/node

# Hazelcast
curl http://localhost:5701/hazelcast/health

# OpenTelemetry Collector
curl http://localhost:8888/metrics
```

### Docker Health Status
```bash
# Check all service health
docker-compose ps

# View detailed health status
docker inspect design16-hazelcast --format='{{.State.Health.Status}}'
```

## 🛠️ Maintenance Commands

### Stopping Services
```bash
# Stop all services
docker-compose down

# Stop and remove volumes (⚠️ Data loss!)
docker-compose down -v

# Stop specific service
docker-compose stop hazelcast
```

### Updating Services
```bash
# Pull latest images
docker-compose pull

# Recreate services with new images
docker-compose up -d --force-recreate
```

### Viewing Resource Usage
```bash
# View resource usage
docker stats

# View specific service logs
docker-compose logs -f --tail=100 hazelcast
```

## 🔧 Troubleshooting

### Common Issues

1. **Port Conflicts**
   ```bash
   # Check what's using a port
   netstat -ano | findstr :5701
   
   # Kill process using port (Windows)
   taskkill /PID <PID> /F
   ```

2. **Hazelcast Connection Issues**
   ```bash
   # Check Hazelcast logs
   docker-compose logs hazelcast
   
   # Restart Hazelcast
   docker-compose restart hazelcast
   ```

3. **Volume Permission Issues**
   ```bash
   # Reset volumes
   docker-compose down -v
   docker volume prune
   docker-compose up -d
   ```

### Performance Tuning

1. **Hazelcast Memory**
   - Adjust `JAVA_OPTS` in docker-compose.yml
   - Default: `-Xms512m -Xmx1g`

2. **MongoDB Memory**
   - Add memory limits in docker-compose.yml
   - Configure WiredTiger cache size

## 📊 Monitoring & Observability

### Prometheus Metrics
- Hazelcast metrics: http://localhost:8889/metrics
- Application metrics collected via OpenTelemetry

### Log Aggregation
```bash
# Follow all logs
docker-compose logs -f

# Export logs to file
docker-compose logs > design16-logs.txt
```

## 🔐 Security Considerations

- Default credentials are used for development
- Change passwords for production deployment
- Consider enabling SSL/TLS for production
- Implement proper network segmentation

## 📝 Configuration Files

### Docker Compose Configuration
- File: `docker-compose.yml`
- Unified configuration for all services
- Optimized for both development and production

### OpenTelemetry Configuration
- File: `otel-collector-config.yaml`
- Configured for metrics, traces, and logs
- Exports to Prometheus and debug console

## 🆘 Support

For issues or questions:
1. Check service logs: `docker-compose logs [service-name]`
2. Verify service health: `docker-compose ps`
3. Review this README for troubleshooting steps
4. Check Docker Desktop status and resources
