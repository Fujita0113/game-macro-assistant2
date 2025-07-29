using FluentAssertions;
using GameMacroAssistant.Core.Models;
using GameMacroAssistant.Core.Services;
using Xunit;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GameMacroAssistant.Tests.Core;

/// <summary>
/// InputRecorderService の単体テスト
/// 90%カバレッジ目標
/// </summary>
public class InputRecorderServiceTests : IDisposable
{
    private readonly InputRecorderService _service;

    public InputRecorderServiceTests()
    {
        _service = new InputRecorderService();
    }

    public void Dispose()
    {
        _service?.Dispose();
    }

    [Fact]
    public void Constructor_Should_Initialize_Default_Values()
    {
        // Assert
        _service.IsRecording.Should().BeFalse();
        _service.StopRecordingKey.Should().Be(27); // ESC key
    }

    [Fact]
    public void StopRecordingKey_Should_Be_Settable()
    {
        // Act
        _service.StopRecordingKey = 123;

        // Assert
        _service.StopRecordingKey.Should().Be(123);
    }

    [Fact]
    public async Task StartRecordingAsync_Should_Set_IsRecording_True()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        
        // Act
        var recordingTask = _service.StartRecordingAsync(cts.Token);
        
        // Assert - 記録開始を少し待つ
        await Task.Delay(50);
        _service.IsRecording.Should().BeTrue();
        
        // Cleanup
        cts.Cancel();
        await recordingTask;
    }

    [Fact]
    public async Task StartRecordingAsync_When_Already_Recording_Should_Throw_InvalidOperationException()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var recordingTask = _service.StartRecordingAsync(cts.Token);
        await Task.Delay(50); // 記録開始を待つ
        
        try
        {
            // Act & Assert
            await _service.Invoking(s => s.StartRecordingAsync())
                .Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Recording is already started");
        }
        finally
        {
            // Cleanup
            cts.Cancel();
            await recordingTask;
        }
    }

    [Fact]
    public async Task StopRecordingAsync_Should_Set_IsRecording_False()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var recordingTask = _service.StartRecordingAsync(cts.Token);
        await Task.Delay(50); // 記録開始を待つ
        
        // Act
        await _service.StopRecordingAsync();
        
        // Assert
        _service.IsRecording.Should().BeFalse();
        
        // recordingTask も完了することを確認
        await recordingTask;
    }

    [Fact]
    public async Task StopRecordingAsync_When_Not_Recording_Should_Complete_Without_Error()
    {
        // Act & Assert - 例外が発生しないことを確認
        await _service.StopRecordingAsync();
        _service.IsRecording.Should().BeFalse();
    }

    [Fact]
    public async Task SimulateInputCapture_Should_Trigger_InputCaptured_Event()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var recordingTask = _service.StartRecordingAsync(cts.Token);
        await Task.Delay(50); // 記録開始を待つ
        
        var eventTriggered = false;
        InputEventArgs? capturedArgs = null;
        
        _service.InputCaptured += (sender, args) =>
        {
            eventTriggered = true;
            capturedArgs = args;
        };
        
        var testEvent = new MouseInputEvent
        {
            Id = Guid.NewGuid(),
            TimestampMs = 1000,
            X = 100,
            Y = 200,
            Button = MouseButton.Left,
            Action = MouseAction.Click
        };
        
        // Act
        _service.SimulateInputCapture(testEvent);
        
        // Assert
        eventTriggered.Should().BeTrue();
        capturedArgs.Should().NotBeNull();
        capturedArgs!.Event.Should().Be(testEvent);
        capturedArgs.Screenshot.Should().BeNull();
        
        // Cleanup
        cts.Cancel();
        await recordingTask;
    }

    [Fact]
    public async Task SimulateInputCapture_With_Screenshot_Should_Include_Screenshot()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var recordingTask = _service.StartRecordingAsync(cts.Token);
        await Task.Delay(50);
        
        InputEventArgs? capturedArgs = null;
        _service.InputCaptured += (sender, args) => capturedArgs = args;
        
        var testEvent = new KeyboardInputEvent
        {
            Id = Guid.NewGuid(),
            TimestampMs = 2000,
            VirtualKeyCode = 65, // 'A'
            Action = KeyboardAction.Press
        };
        
        var screenshot = new byte[] { 1, 2, 3, 4, 5 };
        
        // Act
        _service.SimulateInputCapture(testEvent, screenshot);
        
        // Assert
        capturedArgs.Should().NotBeNull();
        capturedArgs!.Event.Should().Be(testEvent);
        capturedArgs.Screenshot.Should().BeEquivalentTo(screenshot);
        
        // Cleanup
        cts.Cancel();
        await recordingTask;
    }

    [Fact]
    public void SimulateInputCapture_When_Not_Recording_Should_Not_Trigger_Event()
    {
        // Arrange
        var eventTriggered = false;
        _service.InputCaptured += (sender, args) => eventTriggered = true;
        
        var testEvent = new MouseInputEvent
        {
            Id = Guid.NewGuid(),
            TimestampMs = 1000,
            X = 100,
            Y = 200,
            Button = MouseButton.Left,
            Action = MouseAction.Click
        };
        
        // Act
        _service.SimulateInputCapture(testEvent);
        
        // Assert
        eventTriggered.Should().BeFalse();
    }

    [Fact]
    public async Task InputCaptured_Event_Handler_Exception_Should_Not_Stop_Recording()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var recordingTask = _service.StartRecordingAsync(cts.Token);
        await Task.Delay(50);
        
        var eventCount = 0;
        _service.InputCaptured += (sender, args) =>
        {
            eventCount++;
            throw new InvalidOperationException("Test exception");
        };
        
        var testEvent = new MouseInputEvent
        {
            Id = Guid.NewGuid(),
            TimestampMs = 1000,
            X = 100,
            Y = 200,
            Button = MouseButton.Left,
            Action = MouseAction.Click
        };
        
        // Act - 例外が発生するイベントハンドラがあっても記録が継続することを確認
        _service.SimulateInputCapture(testEvent);
        _service.SimulateInputCapture(testEvent);
        
        // Assert
        _service.IsRecording.Should().BeTrue();
        eventCount.Should().Be(2);
        
        // Cleanup
        cts.Cancel();
        await recordingTask;
    }

    [Fact]
    public async Task Dispose_Should_Stop_Recording()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var recordingTask = _service.StartRecordingAsync(cts.Token);
        await Task.Delay(50); // 記録開始を待つ
        
        _service.IsRecording.Should().BeTrue();
        
        // Act
        _service.Dispose();
        
        // Assert
        _service.IsRecording.Should().BeFalse();
    }

    [Fact]
    public void Methods_After_Dispose_Should_Throw_ObjectDisposedException()
    {
        // Arrange
        _service.Dispose();
        
        // Act & Assert
        _service.Invoking(s => s.StartRecordingAsync())
            .Should().ThrowAsync<ObjectDisposedException>();
            
        _service.Invoking(s => s.StopRecordingAsync())
            .Should().ThrowAsync<ObjectDisposedException>();
            
        _service.Invoking(s => s.SimulateInputCapture(new MouseInputEvent
        {
            Id = Guid.NewGuid(),
            TimestampMs = 1000,
            X = 100,
            Y = 200,
            Button = MouseButton.Left,
            Action = MouseAction.Click
        })).Should().Throw<ObjectDisposedException>();
    }
}