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
        }

        try
        {
            // Windows APIフックサービスを初期化（実際のフック機能）
            _hookService ??= new WindowsApiHookService();
            
            // フックイベントをInputCapturedに転送
            _hookService.InputDetected += OnHookInputDetected;

            // 非同期でフック開始（UIをブロックしない）
            _ = Task.Run(async () =>
            {
                try
                {
                    Console.WriteLine("[DEBUG] フック開始を試行中...");
                    await _hookService.StartHookAsync(_recordingCancellation.Token);
                    Console.WriteLine("[DEBUG] フック開始成功");
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine("[DEBUG] フック正常キャンセル");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[DEBUG] フック開始失敗: {ex.Message}");
                    // フック開始失敗は無視（権限不足等）
                }
            }, _recordingCancellation.Token);
            
            // 即座にreturn（UIをブロックしない）
            await Task.CompletedTask;
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

        // フックサービスを停止
        if (_hookService != null)
        {
            _hookService.InputDetected -= OnHookInputDetected;
            await _hookService.StopHookAsync();
        }

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
        Console.WriteLine($"[DEBUG] フックイベント受信: {inputEvent.GetType().Name}, 記録中: {IsRecording}");
        
        if (IsRecording)
        {
            Console.WriteLine("[DEBUG] InputCapturedイベントを発火");
            OnInputCaptured(inputEvent, null); // スクリーンショットは今後実装
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
            InputCaptured?.Invoke(this, args);
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