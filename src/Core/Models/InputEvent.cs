using System;

namespace GameMacroAssistant.Core.Models;

/// <summary>
/// フック対象の入力イベント（座標・タイムスタンプ）
/// R-002, R-003: マウス・キーボード操作の記録
/// </summary>
public abstract record InputEvent
{
    /// <summary>イベントの一意識別子</summary>
    public required Guid Id { get; init; }

    /// <summary>イベント発生時刻（記録開始からの相対時間、ミリ秒）</summary>
    public required int TimestampMs { get; init; }

    /// <summary>イベント種別</summary>
    public abstract InputEventType Type { get; }

    /// <summary>デバッグ用の詳細情報を文字列で取得</summary>
    public abstract override string ToString();
}

/// <summary>
/// マウス入力イベント
/// R-002: スクリーン絶対座標(px)、ボタン種別、押下時間を記録
/// </summary>
public record MouseInputEvent : InputEvent
{
    public override InputEventType Type => InputEventType.Mouse;

    /// <summary>スクリーン絶対座標 X (px)</summary>
    public required int X { get; init; }

    /// <summary>スクリーン絶対座標 Y (px)</summary>
    public required int Y { get; init; }

    /// <summary>マウスボタン種別</summary>
    public required MouseButton Button { get; init; }

    /// <summary>マウス操作種別</summary>
    public required MouseAction Action { get; init; }

    /// <summary>押下時間（ミリ秒）、Action がClick/Up の場合のみ有効</summary>
    public int? PressDurationMs { get; init; }

    public override string ToString()
    {
        var duration = PressDurationMs.HasValue ? $", Duration={PressDurationMs}ms" : "";
        return $"Mouse[{TimestampMs}ms] {Button} {Action} at ({X},{Y}){duration}";
    }
}

/// <summary>
/// キーボード入力イベント
/// R-003: 仮想キーコード、およびキーの押下・離上時刻を記録
/// </summary>
public record KeyboardInputEvent : InputEvent
{
    public override InputEventType Type => InputEventType.Keyboard;

    /// <summary>仮想キーコード (1-255)</summary>
    public required int VirtualKeyCode { get; init; }

    /// <summary>キーボード操作種別</summary>
    public required KeyboardAction Action { get; init; }

    /// <summary>修飾キー（Ctrl, Alt, Shift, Win）</summary>
    public KeyModifiers Modifiers { get; init; } = KeyModifiers.None;

    /// <summary>押下時間（ミリ秒）、Action がPress/Up の場合のみ有効</summary>
    public int? PressDurationMs { get; init; }

    public override string ToString()
    {
        var modifiers = Modifiers != KeyModifiers.None ? $"{Modifiers}+" : "";
        var duration = PressDurationMs.HasValue ? $", Duration={PressDurationMs}ms" : "";
        return $"Keyboard[{TimestampMs}ms] {modifiers}VK{VirtualKeyCode} {Action}{duration}";
    }
}

/// <summary>入力イベント種別</summary>
public enum InputEventType
{
    Mouse,
    Keyboard
}

/// <summary>マウスボタン種別</summary>
public enum MouseButton
{
    None,      // ボタンなし（移動のみ）
    Left,
    Right,
    Middle,
    X1,
    X2
}

/// <summary>マウス操作種別</summary>
public enum MouseAction
{
    Down,      // ボタン押下
    Up,        // ボタン離上
    Click,     // クリック（Down→Up）
    Move       // 移動のみ
}

/// <summary>キーボード操作種別</summary>
public enum KeyboardAction
{
    Down,      // キー押下
    Up,        // キー離上
    Press      // プレス（Down→Up）
}

/// <summary>修飾キー</summary>
[Flags]
public enum KeyModifiers
{
    None = 0,
    Ctrl = 1,
    Alt = 2,
    Shift = 4,
    Win = 8
}

/// <summary>
/// 入力イベント引数（IInputRecorder.InputCaptured イベント用）
/// </summary>
public class InputEventArgs : EventArgs
{
    public required InputEvent Event { get; init; }
    public required byte[]? Screenshot { get; init; }
}