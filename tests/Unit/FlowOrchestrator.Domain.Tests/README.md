# FlowOrchestrator.Domain.Tests

## Overview

The FlowOrchestrator.Domain.Tests project contains unit tests for the domain models and entities defined in the FlowOrchestrator.Domain project. These tests verify the behavior, validation logic, and functionality of the domain entities.

## Purpose

This test project serves to:

1. Validate the correctness of entity implementations
2. Ensure proper validation logic is applied to entities
3. Verify entity relationships and behaviors
4. Maintain code quality through comprehensive test coverage

## Test Structure

The tests are organized by entity type, with each entity having its own test class:

### Base Entity Tests

- **AbstractEntityTests**: Tests for the base abstract entity class

### Flow Entity Tests

- **FlowEntityTests**: Tests for the FlowEntity class
- **ProcessingChainEntityTests**: Tests for the ProcessingChainEntity class

### Source/Destination Entity Tests

- **FileSourceEntityTests**: Tests for the FileSourceEntity class
- **FileDestinationEntityTests**: Tests for the FileDestinationEntity class

### Assignment Entity Tests

- **SourceAssignmentEntityTests**: Tests for the SourceAssignmentEntity class
- **DestinationAssignmentEntityTests**: Tests for the DestinationAssignmentEntity class

### Scheduling Entity Tests

- **TaskSchedulerEntityTests**: Tests for the TaskSchedulerEntity class
- **ScheduledFlowEntityTests**: Tests for the ScheduledFlowEntity class

## Test Approach

The tests use a combination of:

1. **Constructor testing**: Verifying default values and parameterized constructors
2. **Property testing**: Ensuring properties can be set and retrieved correctly
3. **Validation testing**: Verifying validation logic works as expected
4. **Method testing**: Ensuring entity methods function correctly
5. **Interface implementation testing**: Verifying entities correctly implement their interfaces

## Dependencies

- **FlowOrchestrator.Domain**: The project being tested
- **FlowOrchestrator.Abstractions**: Core interfaces and abstract classes
- **xUnit**: Testing framework
- **Moq**: Mocking framework for creating test doubles

## Running the Tests

The tests can be run using the standard .NET test commands:

```bash
dotnet test tests/Unit/FlowOrchestrator.Domain.Tests
```

Or through Visual Studio's Test Explorer.

## Test Coverage

The tests aim to provide comprehensive coverage of:

- All entity constructors
- All entity properties
- All validation scenarios (both valid and invalid)
- All entity methods
- Edge cases and boundary conditions
