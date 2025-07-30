using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using GameMacroAssistant.UI.ViewModels;
using GameMacroAssistant.Core.Models;
using GameMacroAssistant.Core.Services;
using Xunit;

namespace GameMacroAssistant.Tests.UI;

public class MainWindowViewModelTests
{
    private class FakeInputRecorder : IInputRecorder
    {
        public bool IsRecording { get; private set; }
        public int StopRecordingKey { get; set; } = 27;
        public event EventHandler<InputEventArgs>? InputCaptured;

        public Task StartRecordingAsync(CancellationToken cancellationToken = default)
        {
            IsRecording = true;
            return Task.CompletedTask;
        }

        public Task StopRecordingAsync(CancellationToken cancellationToken = default)
        {
            IsRecording = false;
            return Task.CompletedTask;
        }

        public void Dispose() { }

        public void Simulate(InputEvent ev)
        {
            InputCaptured?.Invoke(this, new InputEventArgs { Event = ev, Screenshot = null });
        }
    }

    private class DummyScreenshotProvider : IScreenshotProvider
    {
        public bool IsDesktopDuplicationAvailable => false;
        public CaptureMethod CurrentMethod => CaptureMethod.GdiFallback;
        public void ForceMethod(CaptureMethod method) { }
        public Task<ScreenshotResult> CaptureAsync(int timeoutMs = 50, CancellationToken cancellationToken = default)
            => Task.FromResult(new ScreenshotResult { ImageData = Array.Empty<byte>(), Timestamp = DateTime.UtcNow, Method = CaptureMethod.GdiFallback, RetryCount = 0, DurationMs = 0 });
        public void Dispose() { }
    }

    [Fact]
    public async Task StopRecording_Should_Remove_Last_Event()
    {
        var recorder = new FakeInputRecorder();
        var vm = new MainWindowViewModel(recorder, new DummyScreenshotProvider());

        await vm.StartRecordingCommand.Execute();

        // simulate three user clicks
        for (int i = 0; i < 3; i++)
        {
            recorder.Simulate(new MouseInputEvent
            {
                Id = Guid.NewGuid(),
                TimestampMs = 100 * i,
                X = 0,
                Y = 0,
                Button = MouseButton.Left,
                Action = MouseAction.Click
            });
        }

        // stop button click captured before stop command
        recorder.Simulate(new MouseInputEvent
        {
            Id = Guid.NewGuid(),
            TimestampMs = 999,
            X = 0,
            Y = 0,
            Button = MouseButton.Left,
            Action = MouseAction.Click
        });

        await vm.StopRecordingCommand.Execute();

        vm.Macros.Should().HaveCount(1);
        vm.Macros[0].EventCount.Should().Be(3);
    }
}
