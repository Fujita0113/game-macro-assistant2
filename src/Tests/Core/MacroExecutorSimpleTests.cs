using FluentAssertions;
using GameMacroAssistant.Core.Models;
using GameMacroAssistant.Core.Services;
using Xunit;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GameMacroAssistant.Tests.Core;

/// <summary>
/// MacroExecutor の簡潔なテスト（カバレッジ向上用）
/// </summary>
public class MacroExecutorSimpleTests
{
    [Fact]
    public async Task RunAsync_WithEmptySteps_ShouldCompleteSuccessfully()
    {
        // Arrange
        var executor = new MacroExecutor();
        var macro = CreateTestMacro("Empty Macro", new List<MacroStep>());

        // Act & Assert
        await executor.RunAsync(macro);
    }

    [Fact]
    public async Task RunAsync_WithSingleStep_ShouldExecuteWithoutException()
    {
        // Arrange
        var executor = new MacroExecutor();
        var steps = new List<MacroStep>
        {
            new MacroStep
            {
                Id = Guid.NewGuid(),
                Type = MacroStepType.Mouse,
                TimestampMs = 1000,
                Data = new { X = 100, Y = 200 }
            }
        };
        var macro = CreateTestMacro("Single Step Macro", steps);

        // Act & Assert
        await executor.RunAsync(macro);
    }

    [Fact]
    public async Task RunAsync_WithMultipleSteps_ShouldExecuteAllSteps()
    {
        // Arrange
        var executor = new MacroExecutor();
        var steps = new List<MacroStep>
        {
            new MacroStep
            {
                Id = Guid.NewGuid(),
                Type = MacroStepType.Mouse,
                TimestampMs = 500,
                Data = new { X = 50, Y = 100 }
            },
            new MacroStep
            {
                Id = Guid.NewGuid(),
                Type = MacroStepType.Keyboard,
                TimestampMs = 1000,
                Data = new { Key = "A" }
            },
            new MacroStep
            {
                Id = Guid.NewGuid(),
                Type = MacroStepType.Delay,
                TimestampMs = 1500,
                Data = new { Duration = 100 }
            }
        };
        var macro = CreateTestMacro("Multi Step Macro", steps);

        // Act & Assert
        await executor.RunAsync(macro);
    }

    [Fact]
    public async Task RunAsync_WithCancellation_ShouldThrowOperationCancelledException()
    {
        // Arrange
        var executor = new MacroExecutor();
        var steps = new List<MacroStep>
        {
            new MacroStep
            {
                Id = Guid.NewGuid(),
                Type = MacroStepType.Mouse,
                TimestampMs = 1000,
                Data = new { X = 100, Y = 100 }
            }
        };
        var macro = CreateTestMacro("Cancellation Test", steps);

        using var cts = new CancellationTokenSource();
        cts.Cancel(); // 即座にキャンセル

        // Act & Assert
        await executor.Invoking(e => e.RunAsync(macro, cts.Token))
            .Should().ThrowAsync<OperationCanceledException>();
    }

    private static Macro CreateTestMacro(string name, List<MacroStep> steps)
    {
        return new Macro
        {
            Metadata = new MacroMetadata
            {
                Name = name,
                Created = DateTime.UtcNow,
                Modified = DateTime.UtcNow
            },
            Steps = steps
        };
    }
}