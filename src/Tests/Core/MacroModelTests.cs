using FluentAssertions;
using GameMacroAssistant.Core.Models;
using Xunit;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace GameMacroAssistant.Tests.Core;

/// <summary>
/// Macroモデルの追加テスト（カバレッジ向上用）
/// </summary>
public class MacroModelTests
{
    [Fact]
    public void Macro_ShouldHaveDefaultVersion()
    {
        // Arrange & Act
        var macro = CreateTestMacro("Version Test", new List<MacroStep>());

        // Assert
        macro.Version.Should().Be("1.0");
    }

    [Fact]
    public void MacroMetadata_ShouldHaveAllRequiredProperties()
    {
        // Arrange & Act
        var metadata = new MacroMetadata
        {
            Name = "Test Metadata",
            Description = "Test Description",
            Created = DateTime.UtcNow,
            Modified = DateTime.UtcNow,
            Encrypted = true
        };

        // Assert
        metadata.Name.Should().Be("Test Metadata");
        metadata.Description.Should().Be("Test Description");
        metadata.Created.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        metadata.Modified.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        metadata.Encrypted.Should().BeTrue();
    }

    [Fact]
    public void MacroStep_ShouldHaveAllRequiredProperties()
    {
        // Arrange & Act
        var step = new MacroStep
        {
            Id = Guid.NewGuid(),
            Type = MacroStepType.Mouse,
            TimestampMs = 1000,
            Data = new { X = 100, Y = 200 }
        };

        // Assert
        step.Id.Should().NotBe(Guid.Empty);
        step.Type.Should().Be(MacroStepType.Mouse);
        step.TimestampMs.Should().Be(1000);
        step.Data.Should().NotBeNull();
    }

    [Theory]
    [InlineData(MacroStepType.Mouse)]
    [InlineData(MacroStepType.Keyboard)]
    [InlineData(MacroStepType.Image)]
    [InlineData(MacroStepType.Delay)]
    [InlineData(MacroStepType.Comment)]
    public void MacroStepType_AllEnumValues_ShouldBeValid(MacroStepType stepType)
    {
        // Act & Assert
        stepType.Should().BeDefined();
    }

    [Fact]
    public void MouseStepData_ShouldSerializeCorrectly()
    {
        // Arrange
        var mouseData = new MouseStepData
        {
            X = 150,
            Y = 250,
            Button = "Left",
            Action = "Click"
        };

        // Act
        var json = JsonSerializer.Serialize(mouseData);
        var deserialized = JsonSerializer.Deserialize<MouseStepData>(json);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.X.Should().Be(150);
        deserialized.Y.Should().Be(250);
        deserialized.Button.Should().Be("Left");
        deserialized.Action.Should().Be("Click");
    }

    [Fact]
    public void KeyboardStepData_ShouldHandleModifiers()
    {
        // Arrange
        var keyboardData = new KeyboardStepData
        {
            VirtualKey = 65,
            Action = "Press",
            Modifiers = new[] { "Ctrl", "Shift" }
        };

        // Act & Assert
        keyboardData.VirtualKey.Should().Be(65);
        keyboardData.Action.Should().Be("Press");
        keyboardData.Modifiers.Should().NotBeNull();
        keyboardData.Modifiers.Should().Contain("Ctrl");
        keyboardData.Modifiers.Should().Contain("Shift");
    }

    [Fact]
    public void DelayStepData_ShouldHaveDuration()
    {
        // Arrange & Act
        var delayData = new DelayStepData
        {
            Duration = 500
        };

        // Assert
        delayData.Duration.Should().Be(500);
    }

    [Fact]
    public void CommentStepData_ShouldHaveText()
    {
        // Arrange & Act
        var commentData = new CommentStepData
        {
            Text = "This is a test comment"
        };

        // Assert
        commentData.Text.Should().Be("This is a test comment");
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