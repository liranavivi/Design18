# Port Assignment Documentation

## Overview
This document defines the standardized port assignment scheme for all manager services in the Design16 system.

## Port Assignment Strategy

### **5000-5099: Core Workflow Services**
Services that handle workflow orchestration, step management, and task assignment.

### **5100-5199: Entity Management Services**  
Services that manage core business entities like schemas, processors, addresses, and deliveries.

### **5200-5299: Reserved for Future Services**
Reserved range for additional services and extensions.

## Current Port Assignments

| **Service Name** | **HTTP Port** | **HTTPS Port** | **Category** | **Purpose** |
|------------------|---------------|----------------|--------------|-------------|
| **Manager.Step** | `5000` | `5001` | Core Workflow | Step Management & Validation |
| **Manager.Assignment** | `5010` | `5011` | Core Workflow | Task Assignment Management |
| **Manager.OrchestrationSession** | `5020` | `5021` | Core Workflow | Orchestration Session Management |
| **Manager.Workflow** | `5030` | `5031` | Core Workflow | Workflow Management |
| **Manager.OrchestratedFlow** | `5040` | `5041` | Core Workflow | Orchestrated Flow Management |
| **Manager.Schema** | `5100` | `5101` | Entity Management | Schema Definition Management |
| **Manager.Processor** | `5110` | `5111` | Entity Management | Processor Management |
| **Manager.Address** | `5120` | `5121` | Entity Management | Address/Location Management |
| **Manager.Delivery** | `5130` | `5131` | Entity Management | Delivery Management |

## Service Dependencies

### **Manager.Processor → Manager.Step**
- **Configuration**: `appsettings.json` → `"StepManager": "http://localhost:5000"`
- **Purpose**: Validates processor references before deletion
- **Endpoint**: `/api/step/processor/{processorId}/exists`

### **Manager.Processor → Manager.Schema**
- **Configuration**: `appsettings.json` → `"Schema": "http://localhost:5100"`
- **Purpose**: Schema definition retrieval and validation

### **Processors → Manager.Processor**
- **Target**: `http://localhost:5110` / `https://localhost:5111`
- **Purpose**: Processor registration and management

### **Processors → Manager.Schema**
- **Target**: `http://localhost:5100` / `https://localhost:5101`
- **Purpose**: Schema definition retrieval

## ManagerUrls Configuration Matrix

| **Manager** | **Depends On** | **URL** | **Purpose** |
|-------------|----------------|---------|-------------|
| **Manager.Processor** | Manager.Step | `http://localhost:5000` | Processor reference validation |
| **Manager.Processor** | Manager.Schema | `http://localhost:5100` | Schema definition retrieval |
| **Manager.Address** | Manager.Assignment | `http://localhost:5010` | Assignment management |
| **Manager.Address** | Manager.Schema | `http://localhost:5100` | Schema validation |
| **Manager.Schema** | Manager.Address | `http://localhost:5120` | Address validation |
| **Manager.Schema** | Manager.Delivery | `http://localhost:5130` | Delivery validation |
| **Manager.Schema** | Manager.Processor | `http://localhost:5110` | Processor validation |
| **Manager.Step** | Manager.Assignment | `http://localhost:5010` | Assignment management |
| **Manager.Step** | Manager.Workflow | `http://localhost:5030` | Workflow management |
| **Manager.Step** | Manager.Processor | `http://localhost:5110` | Processor validation |
| **Manager.Delivery** | Manager.Assignment | `http://localhost:5010` | Assignment management |
| **Manager.Delivery** | Manager.Schema | `http://localhost:5100` | Schema validation |
| **Manager.OrchestratedFlow** | Manager.Workflow | `http://localhost:5030` | Workflow management |
| **Manager.OrchestratedFlow** | Manager.Assignment | `http://localhost:5010` | Assignment management |
| **Manager.Assignment** | Manager.Step | `http://localhost:5000` | Step management |

## Port Range Allocation

```
5000-5009: Step Management
5010-5019: Assignment Management  
5020-5029: Orchestration Session Management
5030-5039: Workflow Management
5040-5049: Orchestrated Flow Management
5050-5099: Reserved (Core Workflow)

5100-5109: Schema Management
5110-5119: Processor Management
5120-5129: Address Management
5130-5139: Delivery Management
5140-5199: Reserved (Entity Management)

5200-5299: Reserved (Future Services)
```

## Configuration Updates Required

### **Manager.Processor**
- ✅ **Updated**: `appsettings.json` → StepManager URL: `http://localhost:5000`
- ✅ **Updated**: `appsettings.json` → Schema URL: `http://localhost:5100`

### **Manager.Address**
- ✅ **Updated**: `appsettings.json` → Assignment URL: `http://localhost:5010`
- ✅ **Updated**: `appsettings.json` → Schema URL: `http://localhost:5100`

### **Manager.Schema**
- ✅ **Updated**: `appsettings.json` → Address URL: `http://localhost:5120`
- ✅ **Updated**: `appsettings.json` → Delivery URL: `http://localhost:5130`
- ✅ **Updated**: `appsettings.json` → Processor URL: `http://localhost:5110`

### **Manager.Step**
- ✅ **Updated**: `appsettings.json` → Assignment URL: `http://localhost:5010`
- ✅ **Updated**: `appsettings.json` → Workflow URL: `http://localhost:5030`
- ✅ **Updated**: `appsettings.json` → Processor URL: `http://localhost:5110`

### **Manager.Delivery**
- ✅ **Updated**: `appsettings.json` → Assignment URL: `http://localhost:5010`
- ✅ **Updated**: `appsettings.json` → Schema URL: `http://localhost:5100`

### **Manager.OrchestratedFlow**
- ✅ **Updated**: `appsettings.json` → WorkflowManager URL: `http://localhost:5030`
- ✅ **Updated**: `appsettings.json` → AssignmentManager URL: `http://localhost:5010`

### **Manager.Assignment**
- ✅ **Updated**: `appsettings.json` → StepManager URL: `http://localhost:5000`

### **All Managers**
- ✅ **Updated**: `Properties/launchSettings.json` → New port assignments applied

## Migration Notes

### **Previous Port Assignments (Legacy)**
```
Manager.Step:               5500/5501 → 5000/5001
Manager.Assignment:         5700/5701 → 5010/5011  
Manager.OrchestrationSession: 5800/5801 → 5020/5021
Manager.Workflow:           5900/5901 → 5030/5031
Manager.OrchestratedFlow:   6000/6001 → 5040/5041
Manager.Address:            6200/6201 → 5120/5121
Manager.Delivery:           6300/6301 → 5130/5131
Manager.Processor:          6400/6401 → 5110/5111
Manager.Schema:             6500/6501 → 5100/5101
```

### **Benefits of New Scheme**
1. **Logical Grouping**: Related services are grouped in adjacent port ranges
2. **Easier Management**: Clear separation between workflow and entity services
3. **Scalability**: Reserved ranges allow for future expansion
4. **Consistency**: Predictable port patterns (HTTP = even, HTTPS = odd)
5. **Documentation**: Clear mapping between service type and port range

## Usage Examples

### **Starting Core Workflow Services**
```bash
# Start Step Manager (Core dependency)
cd Managers/Manager.Step && dotnet run

# Start Assignment Manager  
cd Managers/Manager.Assignment && dotnet run

# Start Orchestration Session Manager
cd Managers/Manager.OrchestrationSession && dotnet run
```

### **Starting Entity Management Services**
```bash
# Start Schema Manager (Required for processors)
cd Managers/Manager.Schema && dotnet run

# Start Processor Manager (Required for processors)
cd Managers/Manager.Processor && dotnet run

# Start Address Manager
cd Managers/Manager.Address && dotnet run
```

### **Health Check URLs**
```bash
# Core Workflow Services
curl http://localhost:5000/health  # Step Manager
curl http://localhost:5010/health  # Assignment Manager
curl http://localhost:5020/health  # Orchestration Session Manager

# Entity Management Services  
curl http://localhost:5100/health  # Schema Manager
curl http://localhost:5110/health  # Processor Manager
curl http://localhost:5120/health  # Address Manager
```

## Security Considerations

### **HTTPS Enforcement**
- All services support both HTTP and HTTPS
- Production deployments should use HTTPS endpoints only
- Development environments can use HTTP for simplicity

### **Port Access Control**
- Ensure firewall rules allow access to required ports
- Consider using reverse proxy for external access
- Implement proper authentication/authorization

## Troubleshooting

### **Port Conflicts**
If you encounter port conflicts:
1. Check if other services are using the assigned ports
2. Use `netstat -an | findstr :5000` to check port usage
3. Update port assignments if necessary
4. Restart affected services

### **Service Discovery**
Services discover each other using:
- **Configuration files**: `appsettings.json`
- **Environment variables**: Can override config values
- **Service registry**: Future enhancement for dynamic discovery

---

**Last Updated**: 2025-06-07  
**Version**: 1.0  
**Author**: System Architecture Team
