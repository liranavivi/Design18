# =============================================================================
# Design16 Docker Services - Endpoint Testing Script
# =============================================================================
# This script tests all Docker service endpoints to verify they are running
# and accessible. Run this after starting services with docker-compose up -d
# =============================================================================

param(
    [switch]$Detailed,
    [switch]$HealthOnly,
    [string]$Service = "all"
)

# Color functions for better output
function Write-Success { param($Message) Write-Host "✅ $Message" -ForegroundColor Green }
function Write-Error { param($Message) Write-Host "❌ $Message" -ForegroundColor Red }
function Write-Warning { param($Message) Write-Host "⚠️  $Message" -ForegroundColor Yellow }
function Write-Info { param($Message) Write-Host "ℹ️  $Message" -ForegroundColor Cyan }
function Write-Header { param($Message) Write-Host "`n🔍 $Message" -ForegroundColor Magenta -BackgroundColor Black }

# Test MongoDB
function Test-MongoDB {
    Write-Header "Testing MongoDB (Port 27017)"

    try {
        $result = Test-NetConnection -ComputerName localhost -Port 27017 -WarningAction SilentlyContinue
        if ($result.TcpTestSucceeded) {
            Write-Success "MongoDB is accessible on port 27017"

            if ($Detailed) {
                Write-Info "Connection String: mongodb://admin:admin123@localhost:27017/EntitiesManagerDb"
                Write-Info "Database: EntitiesManagerDb"
                Write-Info "Credentials: admin/admin123"
            }
        } else {
            Write-Error "MongoDB is not accessible on port 27017"
        }
    } catch {
        Write-Error "Failed to test MongoDB: $($_.Exception.Message)"
    }
}

# Test RabbitMQ
function Test-RabbitMQ {
    Write-Header "Testing RabbitMQ (Ports 5672, 15672)"

    # Test AMQP port
    try {
        $amqpResult = Test-NetConnection -ComputerName localhost -Port 5672 -WarningAction SilentlyContinue
        if ($amqpResult.TcpTestSucceeded) {
            Write-Success "RabbitMQ AMQP is accessible on port 5672"
        } else {
            Write-Error "RabbitMQ AMQP is not accessible on port 5672"
        }
    } catch {
        Write-Error "Failed to test RabbitMQ AMQP: $($_.Exception.Message)"
    }

    # Test Management UI
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:15672" -TimeoutSec 10 -UseBasicParsing
        if ($response.StatusCode -eq 200) {
            Write-Success "RabbitMQ Management UI is accessible at http://localhost:15672"

            if ($Detailed) {
                Write-Info "Credentials: guest/guest"
                Write-Info "Virtual Host: /"
            }
        }
    } catch {
        Write-Error "RabbitMQ Management UI is not accessible: $($_.Exception.Message)"
    }
}

# Test Hazelcast
function Test-Hazelcast {
    Write-Header "Testing Hazelcast (Ports 5701, 8080)"

    # Test member port
    try {
        $memberResult = Test-NetConnection -ComputerName localhost -Port 5701 -WarningAction SilentlyContinue
        if ($memberResult.TcpTestSucceeded) {
            Write-Success "Hazelcast member port is accessible on port 5701"
        } else {
            Write-Error "Hazelcast member port is not accessible on port 5701"
        }
    } catch {
        Write-Error "Failed to test Hazelcast member port: $($_.Exception.Message)"
    }

    # Test health endpoint
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:5701/hazelcast/health" -TimeoutSec 10 -UseBasicParsing
        if ($response.StatusCode -eq 200) {
            Write-Success "Hazelcast health endpoint is accessible"

            if ($Detailed) {
                Write-Info "Health Status: $($response.Content)"
                Write-Info "Cluster Name: EntitiesManager"
            }
        }
    } catch {
        Write-Warning "Hazelcast health endpoint may not be available: $($_.Exception.Message)"
    }

    # Test Management Center port
    try {
        $mgmtResult = Test-NetConnection -ComputerName localhost -Port 8080 -WarningAction SilentlyContinue
        if ($mgmtResult.TcpTestSucceeded) {
            Write-Success "Hazelcast Management Center port is accessible on port 8080"
        } else {
            Write-Warning "Hazelcast Management Center is not accessible on port 8080 (may be disabled)"
        }
    } catch {
        Write-Warning "Failed to test Hazelcast Management Center: $($_.Exception.Message)"
    }
}

# Test OpenTelemetry Collector
function Test-OpenTelemetry {
    Write-Header "Testing OpenTelemetry Collector (Ports 4317, 4318, 8888, 8889)"

    # Test gRPC port
    try {
        $grpcResult = Test-NetConnection -ComputerName localhost -Port 4317 -WarningAction SilentlyContinue
        if ($grpcResult.TcpTestSucceeded) {
            Write-Success "OpenTelemetry gRPC endpoint is accessible on port 4317"
        } else {
            Write-Error "OpenTelemetry gRPC endpoint is not accessible on port 4317"
        }
    } catch {
        Write-Error "Failed to test OpenTelemetry gRPC: $($_.Exception.Message)"
    }

    # Test HTTP port
    try {
        $httpResult = Test-NetConnection -ComputerName localhost -Port 4318 -WarningAction SilentlyContinue
        if ($httpResult.TcpTestSucceeded) {
            Write-Success "OpenTelemetry HTTP endpoint is accessible on port 4318"
        } else {
            Write-Error "OpenTelemetry HTTP endpoint is not accessible on port 4318"
        }
    } catch {
        Write-Error "Failed to test OpenTelemetry HTTP: $($_.Exception.Message)"
    }

    # Test metrics endpoint
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:8888/metrics" -TimeoutSec 10 -UseBasicParsing
        if ($response.StatusCode -eq 200) {
            Write-Success "OpenTelemetry metrics endpoint is accessible at http://localhost:8888/metrics"
        }
    } catch {
        Write-Error "OpenTelemetry metrics endpoint is not accessible: $($_.Exception.Message)"
    }

    # Test Prometheus exporter
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:8889/metrics" -TimeoutSec 10 -UseBasicParsing
        if ($response.StatusCode -eq 200) {
            Write-Success "Prometheus metrics endpoint is accessible at http://localhost:8889/metrics"

            if ($Detailed) {
                $metricsCount = ($response.Content -split "`n" | Where-Object { $_ -match "^[a-zA-Z]" }).Count
                Write-Info "Available metrics: $metricsCount"
            }
        }
    } catch {
        Write-Error "Prometheus metrics endpoint is not accessible: $($_.Exception.Message)"
    }
}

# Test Docker containers
function Test-DockerContainers {
    Write-Header "Testing Docker Container Status"

    try {
        $containers = docker ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}" | Where-Object { $_ -match "design16-" }

        if ($containers) {
            Write-Success "Docker containers are running:"
            $containers | ForEach-Object { Write-Info $_ }
        } else {
            Write-Error "No Design16 Docker containers are running"
            Write-Info "Run: docker-compose up -d"
        }
    } catch {
        Write-Error "Failed to check Docker containers: $($_.Exception.Message)"
        Write-Info "Make sure Docker Desktop is running"
    }
}

# Main execution
Write-Host "🚀 Design16 Docker Services - Endpoint Testing" -ForegroundColor White -BackgroundColor DarkBlue
Write-Host "=============================================" -ForegroundColor White -BackgroundColor DarkBlue

if (-not $HealthOnly) {
    Test-DockerContainers
}

switch ($Service.ToLower()) {
    "mongodb" { Test-MongoDB }
    "rabbitmq" { Test-RabbitMQ }
    "hazelcast" { Test-Hazelcast }
    "otel" { Test-OpenTelemetry }
    "opentelemetry" { Test-OpenTelemetry }
    default {
        Test-MongoDB
        Test-RabbitMQ
        Test-Hazelcast
        Test-OpenTelemetry
    }
}

Write-Host "`n🎯 Testing Complete!" -ForegroundColor White -BackgroundColor DarkGreen

if (-not $HealthOnly) {
    Write-Host "`n📋 Quick Commands:" -ForegroundColor Yellow
    Write-Host "  docker-compose ps                    # Check container status" -ForegroundColor Gray
    Write-Host "  docker-compose logs -f [service]     # View service logs" -ForegroundColor Gray
    Write-Host "  docker-compose restart [service]     # Restart service" -ForegroundColor Gray
    Write-Host "  .\test-endpoints.ps1 -Detailed       # Run detailed tests" -ForegroundColor Gray
    Write-Host "  .\test-endpoints.ps1 -Service mongodb # Test specific service" -ForegroundColor Gray
}
