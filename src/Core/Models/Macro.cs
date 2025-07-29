using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace GameMacroAssistant.Core.Models;

/// <summary>
/// マクロ定義 (.gma.json スキーマ準拠)
/// R-017: スキーマバージョンを含む
/// </summary>
public class Macro
{
    /// <summary>スキーマバージョン</summary>
    [JsonPropertyName("version")]
    public string Version { get; set; } = "1.0";

    /// <summary>メタデータ</summary>
    [JsonPropertyName("metadata")]
    public required MacroMetadata Metadata { get; set; }

    /// <summary>マクロステップ配列</summary>
    [JsonPropertyName("steps")]
    public required List<MacroStep> Steps { get; set; }
}

/// <summary>
/// マクロメタデータ
/// </summary>
public class MacroMetadata
{
    /// <summary>マクロ表示名</summary>
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    /// <summary>説明（オプション）</summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>作成日時 (ISO 8601)</summary>
    [JsonPropertyName("created")]
    public required DateTime Created { get; set; }

    /// <summary>最終更新日時 (ISO 8601)</summary>
    [JsonPropertyName("modified")]
    public required DateTime Modified { get; set; }

    /// <summary>パスフレーズで暗号化されているか</summary>
    [JsonPropertyName("encrypted")]
    public bool Encrypted { get; set; } = false;
}

/// <summary>
/// マクロステップ（個別操作）
/// </summary>
public class MacroStep
{
    /// <summary>一意識別子</summary>
    [JsonPropertyName("id")]
    public required Guid Id { get; set; }

    /// <summary>ステップ種別</summary>
    [JsonPropertyName("type")]
    public required MacroStepType Type { get; set; }

    /// <summary>相対タイムスタンプ（ミリ秒）</summary>
    [JsonPropertyName("timestamp")]
    public required int TimestampMs { get; set; }

    /// <summary>ステップデータ</summary>
    [JsonPropertyName("data")]
    public required object Data { get; set; }
}

/// <summary>マクロステップ種別</summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MacroStepType
{
    Mouse,
    Keyboard,
    Image,
    Delay,
    Comment
}

/// <summary>マウスデータ</summary>
public class MouseStepData
{
    [JsonPropertyName("x")]
    public required int X { get; set; }

    [JsonPropertyName("y")]
    public required int Y { get; set; }

    [JsonPropertyName("button")]
    public required string Button { get; set; }

    [JsonPropertyName("action")]
    public required string Action { get; set; }
}

/// <summary>キーボードデータ</summary>
public class KeyboardStepData
{
    [JsonPropertyName("virtualKey")]
    public required int VirtualKey { get; set; }

    [JsonPropertyName("action")]
    public required string Action { get; set; }

    [JsonPropertyName("modifiers")]
    public string[]? Modifiers { get; set; }
}

/// <summary>画像認識データ</summary>
public class ImageStepData
{
    /// <summary>Base64エンコードされたPNG画像</summary>
    [JsonPropertyName("template")]
    public required string Template { get; set; }

    /// <summary>認識領域</summary>
    [JsonPropertyName("region")]
    public required ImageRegion Region { get; set; }

    /// <summary>閾値設定</summary>
    [JsonPropertyName("threshold")]
    public ImageThreshold? Threshold { get; set; }
}

/// <summary>画像認識領域</summary>
public class ImageRegion
{
    [JsonPropertyName("x")]
    public required int X { get; set; }

    [JsonPropertyName("y")]
    public required int Y { get; set; }

    [JsonPropertyName("width")]
    public required int Width { get; set; }

    [JsonPropertyName("height")]
    public required int Height { get; set; }
}

/// <summary>画像認識閾値</summary>
public class ImageThreshold
{
    /// <summary>SSIM閾値 (0.0-1.0)</summary>
    [JsonPropertyName("ssim")]
    public double? Ssim { get; set; }

    /// <summary>ピクセル差分閾値 (0.0-1.0)</summary>
    [JsonPropertyName("pixelDiff")]
    public double? PixelDiff { get; set; }
}

/// <summary>遅延データ</summary>
public class DelayStepData
{
    /// <summary>遅延時間（ミリ秒）</summary>
    [JsonPropertyName("duration")]
    public required int Duration { get; set; }
}

/// <summary>コメントデータ</summary>
public class CommentStepData
{
    /// <summary>コメントテキスト</summary>
    [JsonPropertyName("text")]
    public required string Text { get; set; }
}