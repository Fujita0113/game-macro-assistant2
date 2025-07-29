using System;
using System.Threading;
using System.Threading.Tasks;

namespace GameMacroAssistant.Core.Services;

/// <summary>
/// Desktop Duplication API と GDI BitBlt フォールバックによるスクリーンショット取得
/// R-004, R-006: 50ms以内キャプチャ + リトライ + フォールバック
/// </summary>
public interface IScreenshotProvider : IDisposable
{
    /// <summary>
    /// Desktop Duplication API が利用可能かどうか
    /// </summary>
    bool IsDesktopDuplicationAvailable { get; }

    /// <summary>
    /// 現在のキャプチャ方式
    /// </summary>
    CaptureMethod CurrentMethod { get; }

    /// <summary>
    /// スクリーンショットを取得
    /// R-004: 50ms以内、失敗時は10msバックオフで最大2回リトライ
    /// </summary>
    /// <param name="timeoutMs">タイムアウト（ミリ秒）</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>キャプチャ結果</returns>
    Task<ScreenshotResult> CaptureAsync(int timeoutMs = 50, CancellationToken cancellationToken = default);

    /// <summary>
    /// キャプチャ方式を強制変更（主にテスト用）
    /// </summary>
    /// <param name="method">強制使用するキャプチャ方式</param>
    void ForceMethod(CaptureMethod method);
}

/// <summary>
/// キャプチャ方式
/// </summary>
public enum CaptureMethod
{
    /// <summary>Desktop Duplication API</summary>
    DesktopDuplication,
    /// <summary>GDI BitBlt フォールバック</summary>
    GdiFallback
}

/// <summary>
/// スクリーンショットキャプチャ結果
/// </summary>
public record ScreenshotResult
{
    /// <summary>PNG画像データ（アクティブディスプレイのネイティブ解像度）</summary>
    public required byte[] ImageData { get; init; }

    /// <summary>キャプチャ時刻</summary>
    public required DateTime Timestamp { get; init; }

    /// <summary>使用されたキャプチャ方式</summary>
    public required CaptureMethod Method { get; init; }

    /// <summary>リトライ回数</summary>
    public required int RetryCount { get; init; }

    /// <summary>キャプチャにかかった時間（ミリ秒）</summary>
    public required int DurationMs { get; init; }

    /// <summary>エラーが発生したかどうか</summary>
    public bool HasError => !string.IsNullOrEmpty(ErrorCode);

    /// <summary>エラーコード（Err-CAP等）</summary>
    public string? ErrorCode { get; init; }

    /// <summary>エラーメッセージ</summary>
    public string? ErrorMessage { get; init; }
}