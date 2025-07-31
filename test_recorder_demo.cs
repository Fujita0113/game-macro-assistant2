using System;
using System.Threading;
using System.Threading.Tasks;
using GameMacroAssistant.Core.Models;
using GameMacroAssistant.Core.Services;

// InputRecorderService 動作確認用デモ
class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== InputRecorderService 動作確認デモ ===\n");
        
        using var recorder = new InputRecorderService();
        
        // イベントハンドラを設定
        recorder.InputCaptured += (sender, e) =>
        {
            Console.WriteLine($"📝 キャプチャ: {e.Event}");
            if (e.Screenshot != null)
            {
                Console.WriteLine($"   スクリーンショット: {e.Screenshot.Length} bytes");
            }
        };
        
        Console.WriteLine("1. 記録開始前の状態確認");
        Console.WriteLine($"   IsRecording: {recorder.IsRecording}");
        Console.WriteLine($"   StopRecordingKey: {recorder.StopRecordingKey} (ESC)");
        
        Console.WriteLine("\n2. 記録開始 (3秒間)");
        using var cts = new CancellationTokenSource();
        var recordingTask = recorder.StartRecordingAsync(cts.Token);
        
        // 記録開始まで少し待機
        await Task.Delay(100);
        Console.WriteLine($"   IsRecording: {recorder.IsRecording}");
        
        Console.WriteLine("\n3. テストイベントをシミュレーション");
        
        // マウスクリックイベント
        var mouseEvent = new MouseInputEvent
        {
            Id = Guid.NewGuid(),
            TimestampMs = 1000,
            X = 100,
            Y = 200,
            Button = MouseButton.Left,
            Action = MouseAction.Click,
            PressDurationMs = 50
        };
        recorder.SimulateInputCapture(mouseEvent);
        
        await Task.Delay(500);
        
        // キーボードイベント
        var keyEvent = new KeyboardInputEvent
        {
            Id = Guid.NewGuid(),
            TimestampMs = 1500,
            VirtualKeyCode = 65, // 'A'
            Action = KeyboardAction.Press,
            Modifiers = KeyModifiers.Ctrl,
            PressDurationMs = 30
        };
        var screenshot = new byte[] { 0x89, 0x50, 0x4E, 0x47 }; // PNG header
        recorder.SimulateInputCapture(keyEvent, screenshot);
        
        await Task.Delay(500);
        
        // マウス移動イベント
        var moveEvent = new MouseInputEvent
        {
            Id = Guid.NewGuid(),
            TimestampMs = 2000,
            X = 150,
            Y = 250,
            Button = MouseButton.Left,
            Action = MouseAction.Move
        };
        recorder.SimulateInputCapture(moveEvent);
        
        Console.WriteLine("\n4. 記録停止");
        await recorder.StopRecordingAsync();
        Console.WriteLine($"   IsRecording: {recorder.IsRecording}");
        
        Console.WriteLine("\n5. 記録停止後のイベント送信テスト（無視されるはず）");
        recorder.SimulateInputCapture(mouseEvent);
        
        Console.WriteLine("\n✅ デモ完了！");
        
        // recordingTask の完了を待つ
        await recordingTask;
    }
}