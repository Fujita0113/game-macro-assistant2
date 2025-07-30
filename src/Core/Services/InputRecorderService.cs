using System;
using System.Threading;
using System.Threading.Tasks;
using GameMacroAssistant.Core.Models;

namespace GameMacroAssistant.Core.Services;

/// <summary>
/// マウス・キーボード入力をフックし、イベントを記録するサービス実装
/// R-002, R-003: 座標・ボタン種別・押下時間・仮想キーコードを記録
/// </summary>
public class InputRecorderService : IInputRecorder
{
    private bool _isRecording;
    private bool _disposed;
    private CancellationTokenSource? _recordingCancellation;
    private readonly object _lockObject = new();
    private IWindowsApiHook? _hookService;
    private int? _stopInitiatedTick;
    private const int STOP_EVENT_SUPPRESS_MS = 1000;

    /// <summary>
    /// 入力記録が開始されているかどうか
    /// </summary>
    public bool IsRecording 
    { 
        get 
        { 
            lock (_lockObject) 
            { 
                return _isRecording; 
            } 
        } 
    }

    /// <summary>
    /// 記録停止キー（デフォルト: ESC = 27）
    /// </summary>
    public int StopRecordingKey { get; set; } = 27; // ESC キー

    /// <summary>
    /// 入力イベントがキャプチャされた時に発生
    /// </summary>
    public event EventHandler<InputEventArgs>? InputCaptured;

    /// <summary>
    /// マクロ記録を開始
    /// R-001: UI上のボタンクリックで開始
    /// </summary>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>記録開始タスク</returns>
    public async Task StartRecordingAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        
        lock (_lockObject)
        {
            if (_isRecording)
                throw new InvalidOperationException("Recording is already started");

            _recordingCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _isRecording = true;
            _stopInitiatedTick = null;
        }

        try
        {
            // Windows APIフックサービスを初期化（実際のフック機能）
            _hookService ??= new WindowsApiHookService();
            
            // フックイベントをInputCapturedに転送
            _hookService.InputDetected += OnHookInputDetected;

            // フック開始（適切な待機を行う）
            try
            {
                Console.WriteLine("[DEBUG] フック開始を試行中...");
                await _hookService.StartHookAsync(_recordingCancellation.Token);
                Console.WriteLine("[DEBUG] フック開始成功");
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("[DEBUG] フック正常キャンセル");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DEBUG] フック開始失敗: {ex.Message}");
                throw new InvalidOperationException($"フック開始に失敗しました: {ex.Message}", ex);
            }
        }
        catch (OperationCanceledException)
        {
            // 正常なキャンセル処理
            lock (_lockObject)
            {
                _isRecording = false;
            }
        }
    }

    /// <summary>
    /// 停止準備（停止操作前に呼び出して停止関連イベントを抑制）
    /// </summary>
    public void PrepareForStop()
    {
        if (_isRecording)
        {
            _stopInitiatedTick = Environment.TickCount;
            _hookService?.StartStoppingMode();
            Console.WriteLine($"[DEBUG] 停止準備開始 - 停止関連イベントを抑制 (開始時刻: {_stopInitiatedTick}ms)");
        }
        else
        {
            Console.WriteLine("[DEBUG] 停止準備: 記録中ではないためスキップ");
        }
    }

    /// <summary>
    /// マクロ記録を停止
    /// R-005: デフォルトはESCキー
    /// </summary>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>記録停止タスク</returns>
    public async Task StopRecordingAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        CancellationTokenSource? cancellationToStop;
        
        lock (_lockObject)
        {
            if (!_isRecording)
                return;

            cancellationToStop = _recordingCancellation;
            _isRecording = false;
        }

        // 少し待機してから停止処理を実行（停止操作が確実に抑制されるように）
        await Task.Delay(50, cancellationToken);

        // フックサービスを停止
        if (_hookService != null)
        {
            _hookService.InputDetected -= OnHookInputDetected;
            await _hookService.StopHookAsync();
        }

        _stopInitiatedTick = null;

        cancellationToStop?.Cancel();
        
        await Task.Run(() =>
        {
            while (_isRecording && !cancellationToken.IsCancellationRequested)
            {
                Thread.Sleep(10);
            }
        }, cancellationToken);
    }

    /// <summary>
    /// イベントを発生させる（テスト用およびフック実装時に使用）
    /// </summary>
    /// <param name="inputEvent">入力イベント</param>
    /// <param name="screenshot">スクリーンショット</param>
    public void SimulateInputCapture(InputEvent inputEvent, byte[]? screenshot = null)
    {
        ThrowIfDisposed();
        
        if (!IsRecording)
            return;

        OnInputCaptured(inputEvent, screenshot);
    }

    /// <summary>
    /// Windows APIフックからの入力イベントを処理
    /// </summary>
    private void OnHookInputDetected(object? sender, InputEvent inputEvent)
    {
        var eventDetail = inputEvent switch
        {
            MouseInputEvent m => $"Mouse[{m.Button} {m.Action}] at ({m.X},{m.Y}) ts={m.TimestampMs}",
            KeyboardInputEvent k => $"Keyboard[VK{k.VirtualKeyCode} {k.Action}] ts={k.TimestampMs}",
            _ => $"{inputEvent.GetType().Name} ts={inputEvent.TimestampMs}"
        };
        
        Console.WriteLine($"[DEBUG] フックイベント受信: {eventDetail}, 記録中: {IsRecording}");
        
        // 停止抑制期間チェック
        if (_stopInitiatedTick.HasValue)
        {
            var timeSinceStop = Math.Abs(inputEvent.TimestampMs - _stopInitiatedTick.Value);
            Console.WriteLine($"[DEBUG] 停止開始からの経過時間: {timeSinceStop}ms (抑制閾値: {STOP_EVENT_SUPPRESS_MS}ms)");
            
            if (timeSinceStop <= STOP_EVENT_SUPPRESS_MS)
            {
                Console.WriteLine("[DEBUG] 停止操作に伴うイベントを無視 - 抑制期間内");
                return;
            }
            else
            {
                Console.WriteLine("[DEBUG] 抑制期間を超過 - 通常処理に移行");
            }
        }
        
        // ESCキー押下での自動停止チェック（将来の機能）
        if (IsRecording && inputEvent is KeyboardInputEvent keyEvent)
        {
            if (keyEvent.VirtualKeyCode == StopRecordingKey && keyEvent.Action == KeyboardAction.Down)
            {
                Console.WriteLine($"[DEBUG] 停止キー（VK{StopRecordingKey}）検出 - 記録停止を開始");
                // 非同期で停止処理を実行（フックスレッドをブロックしないため）
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await StopRecordingAsync();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[DEBUG] 自動停止エラー: {ex.Message}");
                    }
                });
                return; // 停止キー自体は記録しない
            }
        }
        
        if (IsRecording)
        {
            Console.WriteLine($"[DEBUG] InputCapturedイベントを発火: {eventDetail}");
            OnInputCaptured(inputEvent, null); // スクリーンショットは今後実装
        }
        else
        {
            Console.WriteLine($"[DEBUG] 記録中ではないため、イベントを無視: {eventDetail}");
        }
    }

    /// <summary>
    /// イベントを発生させる（継承クラスおよび内部処理用）
    /// </summary>
    /// <param name="inputEvent">入力イベント</param>
    /// <param name="screenshot">スクリーンショット</param>
    protected virtual void OnInputCaptured(InputEvent inputEvent, byte[]? screenshot)
    {
        var args = new InputEventArgs
        {
            Event = inputEvent,
            Screenshot = screenshot
        };
        
        try
        {
            Console.WriteLine($"[DEBUG] InputCapturedイベント発火開始: {inputEvent.GetType().Name}");
            InputCaptured?.Invoke(this, args);
            Console.WriteLine($"[DEBUG] InputCapturedイベント発火完了: {inputEvent.GetType().Name}");
        }
        catch (Exception ex)
        {
            // イベントハンドラでの例外は記録処理を停止させない
            // TODO: ログ出力機能実装時にログ記録
            Console.WriteLine($"Warning: InputCaptured event handler threw exception: {ex.Message}");
        }
    }

    /// <summary>
    /// リソースを解放
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            try
            {
                if (_isRecording)
                {
                    // 非同期メソッドを同期的に呼び出し（Disposeパターン）
                    StopRecordingAsync().GetAwaiter().GetResult();
                }
                
                // フックサービスのリソース解放
                _hookService?.Dispose();
            }
            catch (Exception ex)
            {
                // Dispose中の例外は飲み込む
                Console.WriteLine($"Warning: Exception during dispose: {ex.Message}");
            }
            finally
            {
                lock (_lockObject)
                {
                    _recordingCancellation?.Dispose();
                    _recordingCancellation = null;
                    _isRecording = false;
                }
            }
        }

        _disposed = true;
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(InputRecorderService));
    }
}