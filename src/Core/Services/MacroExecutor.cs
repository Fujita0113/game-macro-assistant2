using System;
using System.Threading;
using System.Threading.Tasks;
using GameMacroAssistant.Core.Models;

namespace GameMacroAssistant.Core.Services;

/// <summary>
/// Simplified macro execution engine for headless mode.
/// </summary>
public class MacroExecutor
{
    /// <summary>
    /// Execute a macro. Currently this only logs steps to the console.
    /// </summary>
    /// <param name="macro">Macro definition</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public Task RunAsync(Macro macro, CancellationToken cancellationToken = default)
    {
        foreach (var step in macro.Steps)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Console.WriteLine($"Executing step {step.Type} at {step.TimestampMs}ms");
        }
        return Task.CompletedTask;
    }
}
