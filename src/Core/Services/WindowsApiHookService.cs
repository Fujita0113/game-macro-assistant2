using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
// using System.Windows.Forms; // Core層ではUI依存を避ける
using GameMacroAssistant.Core.Models;

namespace GameMacroAssistant.Core.Services;

/// <summary>
/// Windows API 低レベルフック実装
/// R-001, R-002: マウス・キーボード入力の低遅延監視
/// </summary>
public class WindowsApiHookService : IWindowsApiHook
{
    // Windows API 定数
    private const int WH_MOUSE_LL = 14;
    private const int WH_KEYBOARD_LL = 13;
    private const int WM_MOUSEMOVE = 0x0200;
    private const int WM_LBUTTONDOWN = 0x0201;
    private const int WM_LBUTTONUP = 0x0202;
    private const int WM_RBUTTONDOWN = 0x0204;
    private const int WM_RBUTTONUP = 0x0205;
    private const int WM_MBUTTONDOWN = 0x0207;
    private const int WM_MBUTTONUP = 0x0208;
    private const int WM_KEYDOWN = 0x0100;
    private const int WM_KEYUP = 0x0101;

    // フックハンドル
    private IntPtr _mouseHookHandle = IntPtr.Zero;
    private IntPtr _keyboardHookHandle = IntPtr.Zero;
    private LowLevelMouseProc? _mouseProc;
    private LowLevelKeyboardProc? _keyboardProc;

    // 入力抑制管理
    private long _suppressUntilTicks = 0;
    private readonly object _suppressLock = new();

    // 状態管理
    private volatile bool _isActive = false;
    private CancellationTokenSource? _cancellationTokenSource;
    
    // フック用専用スレッド
    private Thread? _hookThread;
    private AutoResetEvent _threadReady = new(false);
    private uint _hookThreadId;

    public bool IsHookActive => _isActive;
    public event EventHandler<InputEvent>? InputDetected;

    // Windows API P/Invoke 宣言
    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);

    [DllImport("user32.dll")]
    private static extern int GetMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

    [DllImport("user32.dll")]
    private static extern bool TranslateMessage([In] ref MSG lpMsg);

    [DllImport("user32.dll")]
    private static extern IntPtr DispatchMessage([In] ref MSG lpMsg);

    [DllImport("user32.dll")]
    private static extern bool PostThreadMessage(uint idThread, uint Msg, IntPtr wParam, IntPtr lParam);
    
    [DllImport("kernel32.dll")]
    private static extern uint GetCurrentThreadId();

    [StructLayout(LayoutKind.Sequential)]
    public struct MSG
    {
        public IntPtr hwnd;
        public uint message;
        public IntPtr wParam;
        public IntPtr lParam;
        public uint time;
        public POINT pt;
    }

    // フックプロシージャのデリゲート
    private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);
    private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

    // マウス・キーボード構造体
    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int x;
        public int y;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MSLLHOOKSTRUCT
    {
        public POINT pt;
        public uint mouseData;
        public uint flags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct KBDLLHOOKSTRUCT
    {
        public uint vkCode;
        public uint scanCode;
        public uint flags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    public async Task StartHookAsync(CancellationToken cancellationToken = default)
    {
        if (_isActive)
            return;

        _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        _hookThread = new Thread(HookThreadProc)
        {
            IsBackground = true,
            Name = "WindowsApiHookThread"
        };
        _hookThread.Start();
        _threadReady.WaitOne(); // スレッド起動待ち

        _isActive = true;
        await Task.CompletedTask;
    }

    public Task StopHookAsync()
    {
        if (!_isActive)
            return Task.CompletedTask;

        _cancellationTokenSource?.Cancel();
        
        // フック専用スレッドのメッセージループを終了させる
        if (_hookThread?.IsAlive == true && _hookThreadId != 0)
        {
            const uint WM_QUIT = 0x0012;
            PostThreadMessage(_hookThreadId, WM_QUIT, IntPtr.Zero, IntPtr.Zero);
            _hookThread.Join(3000); // 最大3秒待機
        }

        _isActive = false;
        return Task.CompletedTask;
    }

    private void HookThreadProc()
    {
        try
        {
            Console.WriteLine("[DEBUG] フック専用スレッド開始");
            
            // 現在のスレッドIDを保存
            _hookThreadId = GetCurrentThreadId();
            Console.WriteLine($"[DEBUG] フックスレッドID: {_hookThreadId}");
            
            // フックプロシージャをフィールドに保持（GC対策）
            _mouseProc = MouseHookProc;
            _keyboardProc = KeyboardHookProc;

            // マウスフック設定
            _mouseHookHandle = SetWindowsHookEx(WH_MOUSE_LL, _mouseProc,
                GetModuleHandle("kernel32.dll"), 0);

            Console.WriteLine($"[DEBUG] マウスフックハンドル: {_mouseHookHandle}");

            // キーボードフック設定
            _keyboardHookHandle = SetWindowsHookEx(WH_KEYBOARD_LL, _keyboardProc,
                GetModuleHandle("kernel32.dll"), 0);

            Console.WriteLine($"[DEBUG] キーボードフックハンドル: {_keyboardHookHandle}");

            if (_mouseHookHandle == IntPtr.Zero || _keyboardHookHandle == IntPtr.Zero)
            {
                var lastError = Marshal.GetLastWin32Error();
                Console.WriteLine($"[DEBUG] フック設定失敗。Win32エラー: {lastError}");
                _threadReady.Set();
                return;
            }

            Console.WriteLine("[DEBUG] フック設定完了、スレッド準備完了");
            _threadReady.Set(); // メインスレッドに準備完了を通知

            // Windowsメッセージループ
            MSG msg;
            while (!_cancellationTokenSource!.Token.IsCancellationRequested)
            {
                int bRet = GetMessage(out msg, IntPtr.Zero, 0, 0);
                if (bRet == 0) // WM_QUIT
                {
                    Console.WriteLine("[DEBUG] WM_QUIT受信、メッセージループ終了");
                    break;
                }
                if (bRet < 0) // エラー
                {
                    Console.WriteLine("[DEBUG] GetMessageエラー、メッセージループ終了");
                    break;
                }
                
                // キャンセレーション再チェック
                if (_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    Console.WriteLine("[DEBUG] キャンセル要求、メッセージループ終了");
                    break;
                }
                
                TranslateMessage(ref msg);
                DispatchMessage(ref msg);
            }

            Console.WriteLine("[DEBUG] メッセージループ終了");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DEBUG] フックスレッドエラー: {ex.Message}");
            _threadReady.Set();
        }
        finally
        {
            // フック解除
            if (_mouseHookHandle != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_mouseHookHandle);
                _mouseHookHandle = IntPtr.Zero;
            }

            if (_keyboardHookHandle != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_keyboardHookHandle);
                _keyboardHookHandle = IntPtr.Zero;
            }
            
            _hookThreadId = 0; // スレッドID をリセット
            Console.WriteLine("[DEBUG] フック専用スレッド終了");
        }
    }

    public void SuppressInput(int suppressDurationMs)
    {
        lock (_suppressLock)
        {
            _suppressUntilTicks = DateTime.UtcNow.Ticks + (suppressDurationMs * TimeSpan.TicksPerMillisecond);
        }
    }

    private IntPtr MouseHookProc(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && !IsInputSuppressed())
        {
            Console.WriteLine($"[DEBUG] マウスフックイベント: nCode={nCode}, wParam={wParam.ToInt32()}");
            var hookStruct = Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam);
            var mouseEvent = CreateMouseEvent(wParam.ToInt32(), hookStruct);
            
            if (mouseEvent != null)
            {
                Console.WriteLine($"[DEBUG] マウスイベント生成成功: {mouseEvent.Action} at ({mouseEvent.X}, {mouseEvent.Y})");
                InputDetected?.Invoke(this, mouseEvent);
            }
        }

        return CallNextHookEx(_mouseHookHandle, nCode, wParam, lParam);
    }

    private IntPtr KeyboardHookProc(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && !IsInputSuppressed())
        {
            Console.WriteLine($"[DEBUG] キーボードフックイベント: nCode={nCode}, wParam={wParam.ToInt32()}");
            var hookStruct = Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(lParam);
            var keyEvent = CreateKeyboardEvent(wParam.ToInt32(), hookStruct);
            
            if (keyEvent != null)
            {
                Console.WriteLine($"[DEBUG] キーボードイベント生成成功: VK{keyEvent.VirtualKeyCode} {keyEvent.Action}");
                InputDetected?.Invoke(this, keyEvent);
            }
        }

        return CallNextHookEx(_keyboardHookHandle, nCode, wParam, lParam);
    }

    private bool IsInputSuppressed()
    {
        lock (_suppressLock)
        {
            return DateTime.UtcNow.Ticks < _suppressUntilTicks;
        }
    }

    private MouseInputEvent? CreateMouseEvent(int wParam, MSLLHOOKSTRUCT hookStruct)
    {
        var action = wParam switch
        {
            WM_MOUSEMOVE => MouseAction.Move,
            WM_LBUTTONDOWN => MouseAction.Down,
            WM_LBUTTONUP => MouseAction.Up,
            WM_RBUTTONDOWN => MouseAction.Down,
            WM_RBUTTONUP => MouseAction.Up,
            WM_MBUTTONDOWN => MouseAction.Down,
            WM_MBUTTONUP => MouseAction.Up,
            _ => (MouseAction?)null
        };

        var button = wParam switch
        {
            WM_LBUTTONDOWN or WM_LBUTTONUP => MouseButton.Left,
            WM_RBUTTONDOWN or WM_RBUTTONUP => MouseButton.Right,
            WM_MBUTTONDOWN or WM_MBUTTONUP => MouseButton.Middle,
            WM_MOUSEMOVE => MouseButton.None,
            _ => MouseButton.None
        };

        if (action == null) return null;

        return new MouseInputEvent
        {
            Id = Guid.NewGuid(),
            TimestampMs = (int)hookStruct.time,
            X = hookStruct.pt.x,
            Y = hookStruct.pt.y,
            Button = button,
            Action = action.Value,
            PressDurationMs = null // フック段階では持続時間は不明
        };
    }

    private KeyboardInputEvent? CreateKeyboardEvent(int wParam, KBDLLHOOKSTRUCT hookStruct)
    {
        var action = wParam switch
        {
            WM_KEYDOWN => KeyboardAction.Down,
            WM_KEYUP => KeyboardAction.Up,
            _ => (KeyboardAction?)null
        };

        if (action == null) return null;

        return new KeyboardInputEvent
        {
            Id = Guid.NewGuid(),
            TimestampMs = (int)hookStruct.time,
            VirtualKeyCode = (int)hookStruct.vkCode,
            Action = action.Value,
            Modifiers = GetCurrentModifiers(), // 現在のモディファイア状態を取得
            PressDurationMs = null // フック段階では持続時間は不明
        };
    }

    private KeyModifiers GetCurrentModifiers()
    {
        var modifiers = KeyModifiers.None;

        // Core層ではモディファイア状態を簡略化
        // 実際の実装ではGetKeyState APIを使用
        // 現在は基本値を返す
        
        return modifiers;
    }

    public void Dispose()
    {
        if (_isActive)
        {
            StopHookAsync().Wait(3000); // 3秒でタイムアウト
        }
        
        _cancellationTokenSource?.Dispose();
        _threadReady?.Dispose();
        
        // プロシージャ参照をクリア（GC対策）
        _mouseProc = null;
        _keyboardProc = null;
    }
}