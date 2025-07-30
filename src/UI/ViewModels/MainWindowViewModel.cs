using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using ReactiveUI;
using GameMacroAssistant.Core.Models;
using GameMacroAssistant.Core.Services;

namespace GameMacroAssistant.UI.ViewModels;

/// <summary>
/// メインウィンドウのViewModel
/// R-008: ReactiveUIを使用したMVVMパターン実装
/// </summary>
public class MainWindowViewModel : ReactiveObject, IActivatableViewModel
{
    private readonly IInputRecorder _inputRecorder;
    private readonly IScreenshotProvider _screenshotProvider;
    private bool _isRecording;
    private string _statusText = "準備完了";
    private string _systemStatsText = "CPU: 0% | RAM: 0MB";
    private readonly List<InputEvent> _currentRecordingEvents = new();
    private System.Windows.Rect _stopButtonBounds;

    public ViewModelActivator Activator { get; } = new();

    public ObservableCollection<MacroItem> Macros { get; } = new();

    public bool IsRecording
    {
        get => _isRecording;
        set => this.RaiseAndSetIfChanged(ref _isRecording, value);
    }

    public string StatusText
    {
        get => _statusText;
        set => this.RaiseAndSetIfChanged(ref _statusText, value);
    }

    public string SystemStatsText
    {
        get => _systemStatsText;
        set => this.RaiseAndSetIfChanged(ref _systemStatsText, value);
    }

    // ReactiveCommand定義
    public ReactiveCommand<Unit, Unit> StartRecordingCommand { get; }
    public ReactiveCommand<Unit, Unit> StopRecordingCommand { get; }
    public ReactiveCommand<Unit, Unit> PlayMacroCommand { get; }
    public ReactiveCommand<Unit, Unit> OpenSettingsCommand { get; }

    /// <summary>
    /// UI側から直接呼び出し可能な停止準備メソッド
    /// </summary>
    public void PrepareStopRecording()
    {
        if (_inputRecorder is InputRecorderService recorderService)
        {
            recorderService.PrepareForStop();
        }
    }

    public MainWindowViewModel(
        IInputRecorder? inputRecorder = null,
        IScreenshotProvider? screenshotProvider = null)
    {
        _inputRecorder = inputRecorder ?? new InputRecorderService();
        _screenshotProvider = screenshotProvider ?? new ScreenshotProviderService();
        
        // 停止ボタンの大まかな領域を設定（XAMLから推定）
        // ツールバーの停止ボタン: 記録開始ボタンの隣、Width=100, Height=30
        _stopButtonBounds = new System.Windows.Rect(110, 0, 100, 50); // 余裕を持たせた範囲

        // コマンドの初期化
        var canStartRecording = this.WhenAnyValue(x => x.IsRecording, recording => !recording);
        var canStopRecording = this.WhenAnyValue(x => x.IsRecording);
        var canPlay = this.WhenAnyValue(
            x => x.IsRecording,
            x => x.Macros.Count,
            (recording, count) => !recording && count > 0);

        StartRecordingCommand = ReactiveCommand.CreateFromTask(StartRecordingAsync, canStartRecording);
        StopRecordingCommand = ReactiveCommand.CreateFromTask(StopRecordingAsync, canStopRecording);
        PlayMacroCommand = ReactiveCommand.Create(PlayMacro, canPlay);
        OpenSettingsCommand = ReactiveCommand.Create(OpenSettings);

        // ViewModelアクティベーション時の処理
        this.WhenActivated(disposables =>
        {
            // システム統計の定期更新
            Observable.Interval(TimeSpan.FromSeconds(2))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => UpdateSystemStats())
                .DisposeWith(disposables);

            // 入力レコーダーのイベント監視
            _inputRecorder.InputCaptured += OnInputCaptured;
            Disposable.Create(() => _inputRecorder.InputCaptured -= OnInputCaptured)
                .DisposeWith(disposables);
        });
    }

    private async Task StartRecordingAsync()
    {
        try
        {
            StatusText = "記録開始中...";
            _currentRecordingEvents.Clear(); // 新しい記録用にクリア
            await _inputRecorder.StartRecordingAsync();
            IsRecording = true;
            StatusText = "記録中 🔴";
        }
        catch (Exception ex)
        {
            StatusText = $"記録開始エラー: {ex.Message}";
            IsRecording = false;
        }
    }

    private async Task StopRecordingAsync()
    {
        try
        {
            StatusText = "記録停止中...";
            
            Console.WriteLine($"[DEBUG UI] ReactiveCommand停止処理開始。現在のイベント数: {_currentRecordingEvents.Count}");
            Console.WriteLine($"[DEBUG UI] 現在の記録状態: {IsRecording}");
            
            // 即座に記録状態を無効化（追加のイベント記録を防ぐ）
            IsRecording = false;
            
            await _inputRecorder.StopRecordingAsync();

            Console.WriteLine($"[DEBUG UI] 停止完了後のイベント数: {_currentRecordingEvents.Count}");
            
            // 最終的に記録されたイベントの詳細を出力
            Console.WriteLine($"[DEBUG UI] === 最終記録イベント一覧 ===");
            for (int i = 0; i < _currentRecordingEvents.Count; i++)
            {
                var evt = _currentRecordingEvents[i];
                var info = evt switch 
                {
                    MouseInputEvent m => $"  [{i+1}] Mouse[{m.Button} {m.Action}] at ({m.X},{m.Y}) ts={m.TimestampMs} duration={m.PressDurationMs}ms",
                    KeyboardInputEvent k => $"  [{i+1}] Keyboard[VK{k.VirtualKeyCode} {k.Action}] ts={k.TimestampMs}",
                    _ => $"  [{i+1}] {evt.GetType().Name} ts={evt.TimestampMs}"
                };
                Console.WriteLine(info);
            }
            Console.WriteLine($"[DEBUG UI] === 最終イベント一覧終了 ===");

            // 記録されたイベントをコピー
            var recordedEvents = new List<InputEvent>(_currentRecordingEvents);

            // 記録されたマクロを一覧に追加
            var macroName = $"マクロ {DateTime.Now:HH:mm:ss}";
            var macro = new MacroItem
            {
                Name = macroName,
                EventCount = recordedEvents.Count,
                Created = DateTime.Now,
                Events = recordedEvents
            };

            Macros.Add(macro);
            StatusText = $"記録完了: {recordedEvents.Count}イベント";
        }
        catch (Exception ex)
        {
            StatusText = $"記録停止エラー: {ex.Message}";
            IsRecording = false;
        }
    }

    private void PlayMacro()
    {
        // TODO: マクロ再生機能の実装
        StatusText = "マクロ再生機能は今後実装予定";
    }

    private void OpenSettings()
    {
        // TODO: 設定画面の実装
        StatusText = "設定画面は今後実装予定";
    }

    private void OnInputCaptured(object? sender, InputEventArgs e)
    {
        // UIスレッドで実行
        System.Windows.Application.Current?.Dispatcher.Invoke(() =>
        {
            // デバッグ用：詳細なイベント情報を出力
            var eventInfo = e.Event switch 
            {
                MouseInputEvent mouse => $"Mouse[{mouse.Button} {mouse.Action}] at ({mouse.X},{mouse.Y}) duration={mouse.PressDurationMs}ms",
                KeyboardInputEvent key => $"Keyboard[VK{key.VirtualKeyCode} {key.Action}] modifiers={key.Modifiers}",
                _ => e.Event.GetType().Name
            };
            Console.WriteLine($"[DEBUG UI] イベント受信: {eventInfo}, Timestamp={e.Event.TimestampMs}ms, 記録中: {IsRecording}");
            
            // 停止ボタン領域のクリックをフィルタリング
            if (IsRecording && e.Event is MouseInputEvent mouseEvent)
            {
                if (_stopButtonBounds.Contains(mouseEvent.X, mouseEvent.Y))
                {
                    Console.WriteLine($"[DEBUG UI] 停止ボタン領域のクリックを除外: ({mouseEvent.X},{mouseEvent.Y})");
                    return;
                }
            }
            
            // 記録中の場合は、イベントをリストに追加
            if (IsRecording)
            {
                _currentRecordingEvents.Add(e.Event);
                Console.WriteLine($"[DEBUG UI] イベント追加完了。現在のイベント数: {_currentRecordingEvents.Count}");
                
                // 現在記録されているイベントの詳細を出力
                Console.WriteLine($"[DEBUG UI] === 現在記録中のイベント一覧 ===");
                for (int i = 0; i < _currentRecordingEvents.Count; i++)
                {
                    var evt = _currentRecordingEvents[i];
                    var info = evt switch 
                    {
                        MouseInputEvent m => $"  [{i+1}] Mouse[{m.Button} {m.Action}] at ({m.X},{m.Y}) ts={m.TimestampMs}",
                        KeyboardInputEvent k => $"  [{i+1}] Keyboard[VK{k.VirtualKeyCode} {k.Action}] ts={k.TimestampMs}",
                        _ => $"  [{i+1}] {evt.GetType().Name} ts={evt.TimestampMs}"
                    };
                    Console.WriteLine(info);
                }
                Console.WriteLine($"[DEBUG UI] === イベント一覧終了 ===");
            }
            else
            {
                Console.WriteLine($"[DEBUG UI] 記録中ではないため、イベント追加をスキップ");
            }
            
            StatusText = $"入力検出: {eventInfo} [録:{_currentRecordingEvents.Count}]";
        });
    }

    private void UpdateSystemStats()
    {
        try
        {
            // 簡単なシステム統計取得
            var process = System.Diagnostics.Process.GetCurrentProcess();
            var memoryMB = process.WorkingSet64 / (1024 * 1024);
            SystemStatsText = $"RAM: {memoryMB}MB";
        }
        catch
        {
            SystemStatsText = "統計取得不可";
        }
    }
}

/// <summary>
/// マクロアイテムのデータモデル
/// </summary>
public class MacroItem
{
    public required string Name { get; init; }
    public int EventCount { get; init; }
    public DateTime Created { get; init; }
    public required System.Collections.Generic.List<InputEvent> Events { get; init; }
}