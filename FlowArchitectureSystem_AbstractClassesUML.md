# Textual UML View of Abstract Classes and Interfaces

## Core Service Abstractions

```
interface IService
{
    string ServiceId { get; }
    string Version { get; }
    string ServiceType { get; }
    void Initialize(ConfigurationParameters parameters);
    void Terminate();
    ServiceState GetState();
}

abstract class AbstractServiceBase : IService
{
    # ServiceState _state
    + abstract string ServiceId { get; }
    + abstract string Version { get; }
    + abstract string ServiceType { get; }
    + abstract void Initialize(ConfigurationParameters parameters)
    + abstract void Terminate()
    + ServiceState GetState()
    # void SetState(ServiceState newState)
}

interface IImporterService : IService
{
    string Protocol { get; }
    ProtocolCapabilities GetCapabilities();
    ImportResult Import(ImportParameters parameters, ExecutionContext context);
    ValidationResult ValidateConfiguration(ConfigurationParameters parameters);
}

interface IMessageConsumer<T>
{
    Task Consume(ConsumeContext<T> context);
}

abstract class AbstractImporterService : AbstractServiceBase, IImporterService, IMessageConsumer<ImportCommand>
{
    + abstract string Protocol { get; }
    + abstract ProtocolCapabilities GetCapabilities()
    + abstract ImportResult Import(ImportParameters parameters, ExecutionContext context)
    + abstract Task Consume(ConsumeContext<ImportCommand> context)
    # abstract string ClassifyException(Exception ex)
    # abstract Dictionary<string, object> GetErrorDetails(Exception ex)
    # abstract void TryRecover()
    # abstract void OnInitialize()
    # abstract void OnReady()
    # abstract void OnProcessing()
    # abstract void OnError(Exception ex)
    # abstract void OnTerminate()
}

interface IProcessorService : IService
{
    ProcessingResult Process(ProcessParameters parameters, ExecutionContext context);
    SchemaDefinition GetInputSchema();
    SchemaDefinition GetOutputSchema();
    ValidationResult ValidateConfiguration(ConfigurationParameters parameters);
}

abstract class AbstractProcessorService : AbstractServiceBase, IProcessorService, IMessageConsumer<ProcessCommand>
{
    + abstract ProcessingResult Process(ProcessParameters parameters, ExecutionContext context)
    + abstract SchemaDefinition GetInputSchema()
    + abstract SchemaDefinition GetOutputSchema()
    # virtual ValidationResult ValidateInputSchema(DataPackage input)
    # virtual ValidationResult ValidateOutputSchema(DataPackage output)
    # abstract ValidationResult ValidateDataAgainstSchema(DataPackage data, SchemaDefinition schema)
    + abstract Task Consume(ConsumeContext<ProcessCommand> context)
    # abstract string ClassifyException(Exception ex)
    # abstract Dictionary<string, object> GetErrorDetails(Exception ex)
    # abstract void TryRecover()
    # abstract void OnInitialize()
    # abstract void OnReady()
    # abstract void OnProcessing()
    # abstract void OnError(Exception ex)
    # abstract void OnTerminate()
}

interface IExporterService : IService
{
    string Protocol { get; }
    ProtocolCapabilities GetCapabilities();
    MergeCapabilities GetMergeCapabilities();
    ExportResult Export(ExportParameters parameters, ExecutionContext context);
    ValidationResult ValidateConfiguration(ConfigurationParameters parameters);
}

abstract class AbstractExporterService : AbstractServiceBase, IExporterService, IMessageConsumer<ExportCommand>
{
    + abstract string Protocol { get; }
    + abstract ProtocolCapabilities GetCapabilities()
    + abstract MergeCapabilities GetMergeCapabilities()
    + abstract ExportResult Export(ExportParameters parameters, ExecutionContext context)
    + abstract Task Consume(ConsumeContext<ExportCommand> context)
    + abstract ExportResult MergeBranches(Dictionary<string, DataPackage> branchData, MergeStrategy strategy, ExecutionContext context)
    # abstract string ClassifyException(Exception ex)
    # abstract Dictionary<string, object> GetErrorDetails(Exception ex)
    # abstract void TryRecover()
    # abstract void OnInitialize()
    # abstract void OnReady()
    # abstract void OnProcessing()
    # abstract void OnError(Exception ex)
    # abstract void OnTerminate()
}

interface IServiceManager<TService, TServiceId>
{
    RegistrationResult Register(TService service);
    ValidationResult Validate(TService service);
    TService GetService(TServiceId serviceId, string version);
    IEnumerable<TService> GetAllServices(TServiceId serviceId);
    IEnumerable<TService> GetAllServices();
    bool UnregisterService(TServiceId serviceId, string version);
    ServiceStatus GetServiceStatus(TServiceId serviceId, string version);
    void UpdateServiceStatus(TServiceId serviceId, string version, ServiceStatus status);
}

abstract class AbstractManagerService<TService, TServiceId> : AbstractServiceBase, IServiceManager<TService, TServiceId>, IMessageConsumer<ServiceRegistrationCommand<TService>>
{
    + abstract RegistrationResult Register(TService service)
    + abstract ValidationResult Validate(TService service)
    + abstract TService GetService(TServiceId serviceId, string version)
    + abstract IEnumerable<TService> GetAllServices(TServiceId serviceId)
    + abstract IEnumerable<TService> GetAllServices()
    + abstract bool UnregisterService(TServiceId serviceId, string version)
    + abstract ServiceStatus GetServiceStatus(TServiceId serviceId, string version)
    + abstract void UpdateServiceStatus(TServiceId serviceId, string version, ServiceStatus status)
    + abstract Task Consume(ConsumeContext<ServiceRegistrationCommand<TService>> context)
    # abstract string ClassifyException(Exception ex)
    # abstract Dictionary<string, object> GetErrorDetails(Exception ex)
    # abstract void TryRecover()
    # abstract void OnInitialize()
    # abstract void OnReady()
    # abstract void OnProcessing()
    # abstract void OnError(Exception ex)
    # abstract void OnTerminate()
}
```

## Protocol Abstractions

```
interface IProtocol
{
    string Name { get; }
    string Description { get; }
    ProtocolCapabilities GetCapabilities();
    ConnectionParameters GetConnectionParameters();
    ValidationResult ValidateConnectionParameters(Dictionary<string, string> parameters);
    ProtocolHandler CreateHandler(Dictionary<string, string> parameters);
}

abstract class AbstractProtocol : IProtocol
{
    + abstract string Name { get; }
    + abstract string Description { get; }
    + abstract ProtocolCapabilities GetCapabilities()
    + abstract ConnectionParameters GetConnectionParameters()
    + abstract ValidationResult ValidateConnectionParameters(Dictionary<string, string> parameters)
    + abstract ProtocolHandler CreateHandler(Dictionary<string, string> parameters)
}

interface IProtocolHandler
{
    Connection Connect();
    void Disconnect(Connection connection);
    DataPackage Retrieve(Connection connection, RetrieveParameters parameters);
    void Deliver(Connection connection, DeliverParameters parameters);
}

abstract class AbstractProtocolHandler : IProtocolHandler
{
    + abstract override Connection Connect()
    + abstract override void Disconnect(Connection connection)
    + abstract override DataPackage Retrieve(Connection connection, RetrieveParameters parameters)
    + abstract override void Deliver(Connection connection, DeliverParameters parameters)
    # abstract string ClassifyException(Exception ex)
    # abstract Dictionary<string, object> GetErrorDetails(Exception ex)
}
```

## Entity Abstractions

```
interface IEntity
{
    string Version { get; }
    DateTime CreatedTimestamp { get; }
    DateTime LastModifiedTimestamp { get; }
    string VersionDescription { get; }
    string PreviousVersionId { get; }
    VersionStatus VersionStatus { get; }
    string GetEntityId();
    string GetEntityType();
    ValidationResult Validate();
}

abstract class AbstractEntity : IEntity
{
    + string Version { get; set; }
    + DateTime CreatedTimestamp { get; set; }
    + DateTime LastModifiedTimestamp { get; set; }
    + string VersionDescription { get; set; }
    + string PreviousVersionId { get; set; }
    + VersionStatus VersionStatus { get; set; }
    + abstract string GetEntityId()
    + abstract string GetEntityType()
    + abstract ValidationResult Validate()
}

interface IFlowEntity : IEntity
{
    string Id { get; }
    string ImporterServiceId { get; }
    string ImporterServiceVersion { get; }
    IEnumerable<ProcessingChainReference> ProcessingChains { get; }
    IEnumerable<ExporterReference> Exporters { get; }
    IDictionary<string, ConnectionDefinition> Connections { get; }
}

abstract class AbstractFlowEntity : AbstractEntity, IFlowEntity
{
    + string FlowId { get; set; }
    + string Description { get; set; }
    + string ImporterServiceId { get; set; }
    + string ImporterServiceVersion { get; set; }
    + List<ProcessingChainReference> ProcessingChains { get; set; }
    + List<ExporterReference> Exporters { get; set; }
    + Dictionary<string, ConnectionDefinition> Connections { get; set; }
    + override string GetEntityId()
    + override string GetEntityType()
    + abstract override ValidationResult Validate()
    # abstract ValidationResult ValidateSchemaCompatibility()
}
```

## Observability Interfaces

```
interface IStatisticsProvider
{
    void RecordCounter(string name, long increment, Dictionary<string, string> attributes);
    void RecordGauge(string name, double value, Dictionary<string, string> attributes);
    void RecordHistogram(string name, double value, Dictionary<string, string> attributes);
    void RecordTimer(string name, double milliseconds, Dictionary<string, string> attributes);
    void RecordMeter(string name, long count, Dictionary<string, string> attributes);
    void BatchRecord(List<MetricRecord> metrics);
}

interface IStatisticsConsumer
{
    QueryResult QueryMetrics(MetricQuery query);
    List<MetricDefinition> GetAvailableMetrics();
    List<AlertDefinition> GetConfiguredAlerts();
    SubscriptionHandle SubscribeToMetric(string metricName, NotificationHandler handler);
    void UnsubscribeFromMetric(SubscriptionHandle handle);
}

interface IStatisticsLifecycle
{
    void BeginScope(string scopeName, Dictionary<string, string> attributes);
    void EndScope(Dictionary<string, object> results);
    AutomaticTimer StartTimer(string name, Dictionary<string, string> attributes);
    void ConfigureAggregation(string metricName, AggregationPolicy policy);
    void SetRetentionPolicy(string metricName, RetentionPolicy policy);
}

interface IVersionable
{
    VersionInfo GetVersionInfo();
    CompatibilityMatrix GetCompatibilityMatrix();
    bool IsCompatibleWith(ComponentType componentType, string version);
    void OnVersionStatusChange(VersionStatus status);
}
```

## Inheritance Hierarchies

### Service Hierarchy
```
AbstractServiceBase
├── AbstractImporterService
├── AbstractProcessorService
├── AbstractExporterService
└── AbstractManagerService<TService, TServiceId>
```

### Entity Hierarchy
```
AbstractEntity
├── AbstractFlowEntity
├── AbstractProcessingChainEntity
├── AbstractSourceEntity
├── AbstractDestinationEntity
├── AbstractSourceAssignmentEntity
├── AbstractDestinationAssignmentEntity
├── AbstractScheduledFlowEntity
└── AbstractTaskSchedulerEntity
```

### Protocol Hierarchy
```
AbstractProtocol
└── AbstractProtocolHandler
```

### Manager Service Specializations
```
AbstractManagerService<TService, TServiceId>
├── ImporterServiceManager
├── ProcessorServiceManager
├── ExporterServiceManager
├── SourceEntityManager
├── DestinationEntityManager
├── SourceAssignmentEntityManager
├── DestinationAssignmentEntityManager
├── TaskSchedulerEntityManager
└── ScheduledFlowEntityManager
```
