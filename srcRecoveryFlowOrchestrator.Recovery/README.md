# FlowOrchestrator.Recovery

This Worker Service handles error recovery and resilience for the FlowOrchestrator system.

## Overview

The `FlowOrchestrator.Recovery` service is designed to:

1. Provide standardized error handling and recovery mechanisms
2. Implement various recovery strategies (Retry, Circuit Breaker, Fallback, etc.)
3. Classify errors according to the system taxonomy
4. Track error patterns and optimize recovery strategies
5. Coordinate recovery operations across the system

## Key Components

### Recovery Strategies

The service implements the following recovery strategies:

1. **RetryRecoveryStrategy**: Automatically retry failed operations with configurable backoff
   - Simple retry with fixed delay
   - Exponential backoff for transient failures
   - Jittered retry for load balancing
   - Maximum retry count enforcement

2. **CircuitBreakerRecoveryStrategy**: Prevent cascading failures by failing fast
   - Failure threshold monitoring
   - Automatic circuit opening on threshold breach
   - Gradual recovery with half-open state
   - Automatic circuit reset after recovery

3. **FallbackRecoveryStrategy**: Use alternative processing paths when primary paths fail
   - Cached result fallback
   - Alternative path fallback
   - Graceful degradation
   - Message queue fallback
   - Manual intervention fallback

4. **BulkheadRecoveryStrategy**: Isolate failures to prevent system-wide impact
   - Service isolation
   - Branch isolation
   - Resource isolation

5. **CompensationRecoveryStrategy**: Reverse completed steps when later steps fail
   - Transaction-like semantics
   - Ordered compensation actions
   - Parallel compensation actions

6. **TimeoutRecoveryStrategy**: Enforce time limits on operations to prevent blocking
   - Operation timeouts
   - Flow timeouts
   - Branch timeouts
   - Graceful termination on timeout

### Services

1. **RecoveryService**: Core service for handling errors and applying recovery strategies
2. **ErrorClassificationService**: Service for classifying errors and tracking error patterns

### Message Consumers

1. **ErrorEventConsumer**: Consumes error events and applies recovery strategies
2. **RecoveryCommandConsumer**: Consumes recovery commands for manual recovery operations

## Configuration

The service is configured through `appsettings.json`:

```json
{
  "Recovery": {
    "RecoveryHistoryRetentionHours": 24,
    "MaxActiveRecoveries": 1000
  },
  "RetryStrategy": {
    "MaxRetryCount": 3,
    "RetryDelayMilliseconds": 1000,
    "BackoffFactor": 2.0,
    "RetryType": "Exponential"
  },
  "CircuitBreakerStrategy": {
    "FailureThreshold": 3,
    "ResetTimeoutSeconds": 60,
    "SuccessThreshold": 2
  },
  "FallbackStrategy": {
    "FallbackType": "GracefulDegradation"
  },
  "BulkheadStrategy": {
    "MaxConcurrentExecutions": 10,
    "BulkheadType": "Service"
  },
  "CompensationStrategy": {
    "CompensationTimeoutSeconds": 60,
    "CompensationOrder": "ReverseOrder"
  },
  "TimeoutStrategy": {
    "OperationTimeoutSeconds": 30,
    "FlowTimeoutSeconds": 300,
    "BranchTimeoutSeconds": 120,
    "TimeoutAction": "Terminate"
  }
}
```

## Dependencies

- `FlowOrchestrator.Abstractions`: Core interfaces and abstract classes
- `FlowOrchestrator.Common`: Common utilities and helpers
- `FlowOrchestrator.Infrastructure.Messaging.MassTransit`: Messaging infrastructure

## Usage Examples

### Handling an Error

```csharp
// In a service class
public class MyService
{
    private readonly RecoveryService _recoveryService;

    public MyService(RecoveryService recoveryService)
    {
        _recoveryService = recoveryService;
    }

    public void HandleError(Exception ex, string serviceId, string executionId)
    {
        // Classify the exception
        var errorContext = _recoveryService.ClassifyException(ex, serviceId, executionId);

        // Handle the error
        var recoveryResult = _recoveryService.HandleError(errorContext);

        // Take action based on the recovery result
        switch (recoveryResult.Action)
        {
            case RecoveryAction.RETRY:
                // Retry the operation
                break;
            case RecoveryAction.FAIL_BRANCH:
                // Fail the branch but continue with other branches
                break;
            case RecoveryAction.FAIL_EXECUTION:
                // Fail the entire execution
                break;
            case RecoveryAction.FAIL_FAST:
                // Fail fast without retrying
                break;
            case RecoveryAction.USE_FALLBACK:
                // Use a fallback operation
                break;
            case RecoveryAction.COMPENSATE:
                // Apply compensation actions
                break;
            case RecoveryAction.CONTINUE_DEGRADED:
                // Continue with degraded functionality
                break;
        }
    }
}
```

### Publishing an Error Event

```csharp
// In a service class
public class MyService
{
    private readonly IMessageBus _messageBus;

    public MyService(IMessageBus messageBus)
    {
        _messageBus = messageBus;
    }

    public async Task PublishErrorEventAsync(ErrorContext errorContext, ExecutionContext executionContext)
    {
        var errorEvent = new ErrorEvent
        {
            ErrorContext = errorContext,
            ExecutionContext = executionContext
        };

        await _messageBus.PublishAsync(errorEvent);
    }
}
```
