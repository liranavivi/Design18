using FlowOrchestrator.Abstractions.Common;
using FlowOrchestrator.Abstractions.Messaging;
using FlowOrchestrator.Abstractions.Services;

namespace FlowOrchestrator.Processing.Messaging
{
    /// <summary>
    /// Base implementation for handling process commands.
    /// </summary>
    public abstract class ProcessCommandHandler
    {
        /// <summary>
        /// Handles a process command.
        /// </summary>
        /// <param name="command">The process command to handle.</param>
        /// <param name="processor">The processor service.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task HandleProcessCommand(ProcessCommand command, IProcessorService processor)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            if (processor == null)
            {
                throw new ArgumentNullException(nameof(processor));
            }

            try
            {
                // Process the data
                var result = processor.Process(command.Parameters, command.Context);

                // Publish the result
                await PublishProcessResult(command, result);
            }
            catch (Exception ex)
            {
                // Publish the error
                await PublishProcessError(command, ex);
                throw;
            }
        }

        /// <summary>
        /// Publishes a process result.
        /// </summary>
        /// <param name="command">The original process command.</param>
        /// <param name="result">The processing result.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        protected abstract Task PublishProcessResult(ProcessCommand command, ProcessingResult result);

        /// <summary>
        /// Publishes a process error.
        /// </summary>
        /// <param name="command">The original process command.</param>
        /// <param name="exception">The exception that occurred.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        protected abstract Task PublishProcessError(ProcessCommand command, Exception exception);
    }
}
