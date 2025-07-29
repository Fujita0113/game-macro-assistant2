using FluentAssertions;
using GameMacroAssistant.Core.Models;
using Xunit;
using System;

namespace GameMacroAssistant.Tests.Core;

/// <summary>
/// InputEvent ToString() メソッドのテスト（カバレッジ向上用）
/// </summary>
public class InputEventToStringTests
{
    [Fact]
    public void MouseInputEvent_ToString_ShouldReturnFormattedString()
    {
        // Arrange
        var mouseEvent = new MouseInputEvent
        {
            Id = Guid.NewGuid(),
            TimestampMs = 1500,
            X = 250,
            Y = 350,
            Button = MouseButton.Right,
            Action = MouseAction.Click,
            PressDurationMs = 75
        };

        // Act
        var result = mouseEvent.ToString();

        // Assert
        result.Should().Contain("Mouse[1500ms]");
        result.Should().Contain("Right");
        result.Should().Contain("Click");
        result.Should().Contain("(250,350)");
        result.Should().Contain("Duration=75ms");
    }

    [Fact]
    public void MouseInputEvent_ToString_WithoutDuration_ShouldNotIncludeDuration()
    {
        // Arrange
        var mouseEvent = new MouseInputEvent
        {
            Id = Guid.NewGuid(),
            TimestampMs = 800,
            X = 100,
            Y = 150,
            Button = MouseButton.Left,
            Action = MouseAction.Move,
            PressDurationMs = null
        };

        // Act
        var result = mouseEvent.ToString();

        // Assert
        result.Should().Contain("Mouse[800ms]");
        result.Should().Contain("Left");
        result.Should().Contain("Move");
        result.Should().Contain("(100,150)");
        result.Should().NotContain("Duration");
    }

    [Fact]
    public void KeyboardInputEvent_ToString_ShouldReturnFormattedString()
    {
        // Arrange
        var keyEvent = new KeyboardInputEvent
        {
            Id = Guid.NewGuid(),
            TimestampMs = 2000,
            VirtualKeyCode = 65, // 'A'
            Action = KeyboardAction.Press,
            Modifiers = KeyModifiers.Ctrl | KeyModifiers.Shift,
            PressDurationMs = 50
        };

        // Act
        var result = keyEvent.ToString();

        // Assert
        result.Should().Contain("Keyboard[2000ms]");
        result.Should().Contain("VK65");
        result.Should().Contain("Press");
        result.Should().Contain("Duration=50ms");
        result.Should().Contain("Ctrl");
        result.Should().Contain("Shift");
    }

    [Fact]
    public void KeyboardInputEvent_ToString_WithoutModifiers_ShouldNotIncludeModifiers()
    {
        // Arrange
        var keyEvent = new KeyboardInputEvent
        {
            Id = Guid.NewGuid(),
            TimestampMs = 1200,
            VirtualKeyCode = 32, // Space
            Action = KeyboardAction.Down,
            Modifiers = KeyModifiers.None,
            PressDurationMs = null
        };

        // Act
        var result = keyEvent.ToString();

        // Assert
        result.Should().Contain("Keyboard[1200ms]");
        result.Should().Contain("VK32");
        result.Should().Contain("Down");
        result.Should().NotContain("Duration");
        result.Should().NotContain("Ctrl");
        result.Should().NotContain("Alt");
        result.Should().NotContain("Shift");
        result.Should().NotContain("Win");
    }

    [Theory]
    [InlineData(MouseButton.Left, "Left")]
    [InlineData(MouseButton.Right, "Right")]
    [InlineData(MouseButton.Middle, "Middle")]
    [InlineData(MouseButton.X1, "X1")]
    [InlineData(MouseButton.X2, "X2")]
    public void MouseInputEvent_ToString_ShouldIncludeCorrectButtonName(MouseButton button, string expectedName)
    {
        // Arrange
        var mouseEvent = new MouseInputEvent
        {
            Id = Guid.NewGuid(),
            TimestampMs = 1000,
            X = 0,
            Y = 0,
            Button = button,
            Action = MouseAction.Click
        };

        // Act
        var result = mouseEvent.ToString();

        // Assert
        result.Should().Contain(expectedName);
    }

    [Theory]
    [InlineData(MouseAction.Down, "Down")]
    [InlineData(MouseAction.Up, "Up")]
    [InlineData(MouseAction.Click, "Click")]
    [InlineData(MouseAction.Move, "Move")]
    public void MouseInputEvent_ToString_ShouldIncludeCorrectActionName(MouseAction action, string expectedName)
    {
        // Arrange
        var mouseEvent = new MouseInputEvent
        {
            Id = Guid.NewGuid(),
            TimestampMs = 1000,
            X = 0,
            Y = 0,
            Button = MouseButton.Left,
            Action = action
        };

        // Act
        var result = mouseEvent.ToString();

        // Assert
        result.Should().Contain(expectedName);
    }

    [Theory]
    [InlineData(KeyboardAction.Down, "Down")]
    [InlineData(KeyboardAction.Up, "Up")]
    [InlineData(KeyboardAction.Press, "Press")]
    public void KeyboardInputEvent_ToString_ShouldIncludeCorrectActionName(KeyboardAction action, string expectedName)
    {
        // Arrange
        var keyEvent = new KeyboardInputEvent
        {
            Id = Guid.NewGuid(),
            TimestampMs = 1000,
            VirtualKeyCode = 65,
            Action = action,
            Modifiers = KeyModifiers.None
        };

        // Act
        var result = keyEvent.ToString();

        // Assert
        result.Should().Contain(expectedName);
    }
}