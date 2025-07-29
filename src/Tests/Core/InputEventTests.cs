using FluentAssertions;
using GameMacroAssistant.Core.Models;
using Xunit;

namespace GameMacroAssistant.Tests.Core;

/// <summary>
/// InputEvent モデルの単体テスト
/// 90%カバレッジ目標
/// </summary>
public class InputEventTests
{
    [Fact]
    public void MouseInputEvent_Should_Have_Correct_Type()
    {
        // Arrange & Act
        var mouseEvent = new MouseInputEvent
        {
            Id = Guid.NewGuid(),
            TimestampMs = 1000,
            X = 100,
            Y = 200,
            Button = MouseButton.Left,
            Action = MouseAction.Click
        };

        // Assert
        mouseEvent.Type.Should().Be(InputEventType.Mouse);
        mouseEvent.X.Should().Be(100);
        mouseEvent.Y.Should().Be(200);
        mouseEvent.Button.Should().Be(MouseButton.Left);
        mouseEvent.Action.Should().Be(MouseAction.Click);
    }

    [Fact]
    public void KeyboardInputEvent_Should_Have_Correct_Type()
    {
        // Arrange & Act
        var keyboardEvent = new KeyboardInputEvent
        {
            Id = Guid.NewGuid(),
            TimestampMs = 2000,
            VirtualKeyCode = 65, // 'A'
            Action = KeyboardAction.Press,
            Modifiers = KeyModifiers.Ctrl | KeyModifiers.Shift
        };

        // Assert
        keyboardEvent.Type.Should().Be(InputEventType.Keyboard);
        keyboardEvent.VirtualKeyCode.Should().Be(65);
        keyboardEvent.Action.Should().Be(KeyboardAction.Press);
        keyboardEvent.Modifiers.Should().Be(KeyModifiers.Ctrl | KeyModifiers.Shift);
    }

    [Theory]
    [InlineData(MouseButton.Left, "Left")]
    [InlineData(MouseButton.Right, "Right")]
    [InlineData(MouseButton.Middle, "Middle")]
    [InlineData(MouseButton.X1, "X1")]
    [InlineData(MouseButton.X2, "X2")]
    public void MouseButton_Enum_Should_Have_Correct_Values(MouseButton button, string expectedName)
    {
        // Act & Assert
        button.ToString().Should().Be(expectedName);
    }

    [Theory]
    [InlineData(KeyModifiers.None, 0)]
    [InlineData(KeyModifiers.Ctrl, 1)]
    [InlineData(KeyModifiers.Alt, 2)]
    [InlineData(KeyModifiers.Shift, 4)]
    [InlineData(KeyModifiers.Win, 8)]
    public void KeyModifiers_Flags_Should_Have_Correct_Values(KeyModifiers modifier, int expectedValue)
    {
        // Act & Assert
        ((int)modifier).Should().Be(expectedValue);
    }

    [Fact]
    public void InputEventArgs_Should_Initialize_Correctly()
    {
        // Arrange
        var inputEvent = new MouseInputEvent
        {
            Id = Guid.NewGuid(),
            TimestampMs = 1500,
            X = 50,
            Y = 75,
            Button = MouseButton.Right,
            Action = MouseAction.Down
        };
        var screenshot = new byte[] { 1, 2, 3, 4, 5 };

        // Act
        var args = new InputEventArgs
        {
            Event = inputEvent,
            Screenshot = screenshot
        };

        // Assert
        args.Event.Should().Be(inputEvent);
        args.Screenshot.Should().BeEquivalentTo(screenshot);
    }
}