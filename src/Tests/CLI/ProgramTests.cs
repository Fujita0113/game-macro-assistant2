using FluentAssertions;
using GameMacroAssistant.CLI;
using GameMacroAssistant.Core.Models;
using Xunit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace GameMacroAssistant.Tests.CLI;

/// <summary>
/// CLI Program.cs のテスト（カバレッジ向上用）
/// </summary>
public class ProgramTests
{
    [Fact]
    public async Task Main_WithoutHeadless_ShouldReturnSuccess()
    {
        // Arrange
        var args = new string[0];

        // Act
        var result = await Program.Main(args);

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public async Task Main_WithHeadlessButNoMacroPath_ShouldReturnError()
    {
        // Arrange
        var args = new string[] { "--headless" };

        // Act
        var result = await Program.Main(args);

        // Assert
        result.Should().Be(1);
    }

    [Fact]
    public async Task Main_WithValidMacro_ShouldExecuteSuccessfully()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        var macro = CreateTestMacro("CLI Test", new List<MacroStep>
        {
            new MacroStep
            {
                Id = Guid.NewGuid(),
                Type = MacroStepType.Mouse,
                TimestampMs = 500,
                Data = new { X = 10, Y = 20 }
            }
        });

        try
        {
            File.WriteAllText(tempFile, JsonSerializer.Serialize(macro));
            var args = new string[] { "--headless", tempFile };

            // Act
            var result = await Program.Main(args);

            // Assert
            result.Should().Be(0);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task Main_WithInvalidJson_ShouldReturnError()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        
        try
        {
            File.WriteAllText(tempFile, "invalid json");
            var args = new string[] { "--headless", tempFile };

            // Act
            var result = await Program.Main(args);

            // Assert
            result.Should().Be(1);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task Main_WithNonExistentFile_ShouldReturnError()
    {
        // Arrange
        var args = new string[] { "--headless", "nonexistent.json" };

        // Act
        var result = await Program.Main(args);

        // Assert
        result.Should().Be(1);
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