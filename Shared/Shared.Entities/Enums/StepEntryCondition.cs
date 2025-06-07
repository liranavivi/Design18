using System.ComponentModel;

namespace Shared.Entities.Enums;

/// <summary>
/// Defines the entry conditions that determine when a step should be executed.
/// These conditions control the workflow execution logic and step transitions.
/// </summary>
public enum StepEntryCondition
{
    /// <summary>
    /// Execute only if the previous step completed successfully.
    /// This is the default behavior for most workflow steps.
    /// </summary>
    [Description("Execute only if previous step succeeded")]
    PreviousSuccess = 0,

    /// <summary>
    /// Execute only if the previous step failed or encountered an error.
    /// Useful for error handling, cleanup, or alternative processing paths.
    /// </summary>
    [Description("Execute only if previous step failed")]
    PreviousFailure = 1,

    /// <summary>
    /// Execute based on the output/result of the previous step.
    /// The step will evaluate the previous step's output to determine execution.
    /// </summary>
    [Description("Execute based on previous step output")]
    PreviousOutput = 2,

    /// <summary>
    /// Execute using custom logic defined in the step configuration.
    /// Allows for complex conditional logic beyond standard patterns.
    /// </summary>
    [Description("Execute using custom logic")]
    Custom = 3,

    /// <summary>
    /// Always execute this step regardless of previous step results.
    /// Useful for initialization, logging, or mandatory processing steps.
    /// </summary>
    [Description("Always execute regardless of previous results")]
    Always = 4,

    /// <summary>
    /// Never execute this step - it is disabled.
    /// Useful for temporarily disabling steps without removing them.
    /// </summary>
    [Description("Never execute - step is disabled")]
    Never = 5,

    /// <summary>
    /// Execute only if this is the first step in the workflow.
    /// Useful for workflow initialization and setup steps.
    /// </summary>
    [Description("Execute only as the first step")]
    FirstStep = 6,

    /// <summary>
    /// Execute only if this is the last step in the workflow.
    /// Useful for cleanup, finalization, and result processing.
    /// </summary>
    [Description("Execute only as the last step")]
    LastStep = 7,

    /// <summary>
    /// Execute based on a timeout condition.
    /// The step will execute after a specified time period.
    /// </summary>
    [Description("Execute after timeout condition")]
    Timeout = 8,

    /// <summary>
    /// Execute when specific external conditions are met.
    /// Useful for integration with external systems or events.
    /// </summary>
    [Description("Execute when external conditions are met")]
    ExternalTrigger = 9
}
