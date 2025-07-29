using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace GameMacroAssistant.Core.Services;

/// <summary>
/// Desktop Duplication API と GDI BitBlt フォールバックによるスクリーンショット実装
/// R-004, R-006: 50ms以内キャプチャ + リトライ + フォールバック
/// </summary>
public class ScreenshotProviderService : IScreenshotProvider
{
    private bool _disposed = false;
    private CaptureMethod _currentMethod = CaptureMethod.DesktopDuplication;
    private CaptureMethod? _forcedMethod = null;

    // GDI32.dll P/Invoke 宣言（フォールバック用）
    [DllImport("gdi32.dll")]
    private static extern IntPtr CreateDC(string lpszDriver, string lpszDevice, string lpszOutput, IntPtr lpInitData);

    [DllImport("gdi32.dll")]
    private static extern bool DeleteDC(IntPtr hdc);

    [DllImport("gdi32.dll")]
    private static extern IntPtr CreateCompatibleDC(IntPtr hdc);

    [DllImport("gdi32.dll")]
    private static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);

    [DllImport("gdi32.dll")]
    private static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

    [DllImport("gdi32.dll")]
    private static extern bool BitBlt(IntPtr hdcDest, int nXDest, int nYDest, int nWidth, int nHeight,
        IntPtr hdcSrc, int nXSrc, int nYSrc, uint dwRop);

    [DllImport("user32.dll")]
    private static extern int GetSystemMetrics(int nIndex);

    // システムメトリクス定数
    private const int SM_CXSCREEN = 0;
    private const int SM_CYSCREEN = 1;
    private const uint SRCCOPY = 0x00CC0020;

    public bool IsDesktopDuplicationAvailable
    {
        get
        {
            // Desktop Duplication API の可用性チェック
            // Windows 10/11 (Major=10以降) または Windows 8/8.1 (Major=6, Minor=2以降)
            var version = Environment.OSVersion.Version;
            return version.Major >= 10 || 
                   (version.Major == 6 && version.Minor >= 2);
        }
    }

    public CaptureMethod CurrentMethod => _forcedMethod ?? _currentMethod;

    public void ForceMethod(CaptureMethod method)
    {
        _forcedMethod = method;
    }

    public async Task<ScreenshotResult> CaptureAsync(int timeoutMs = 50, CancellationToken cancellationToken = default)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(ScreenshotProviderService));

        var startTime = DateTime.UtcNow;
        var retryCount = 0;
        const int maxRetries = 2;
        const int backoffMs = 10;

        while (retryCount <= maxRetries)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var method = _forcedMethod ?? _currentMethod;
                
                using var timeoutCts = new CancellationTokenSource(timeoutMs);
                using var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(
                    cancellationToken, timeoutCts.Token);

                byte[] imageData;
                
                if (method == CaptureMethod.DesktopDuplication && IsDesktopDuplicationAvailable)
                {
                    // Desktop Duplication API を試行
                    try
                    {
                        imageData = await CaptureWithDesktopDuplicationAsync(combinedCts.Token);
                        _currentMethod = CaptureMethod.DesktopDuplication;
                    }
                    catch (Exception)
                    {
                        // フォールバックに切り替え
                        imageData = await CaptureWithGdiFallbackAsync(combinedCts.Token);
                        _currentMethod = CaptureMethod.GdiFallback;
                    }
                }
                else
                {
                    // GDI フォールバック
                    imageData = await CaptureWithGdiFallbackAsync(combinedCts.Token);
                    _currentMethod = CaptureMethod.GdiFallback;
                }

                var duration = (int)(DateTime.UtcNow - startTime).TotalMilliseconds;

                return new ScreenshotResult
                {
                    ImageData = imageData,
                    Timestamp = startTime,
                    Method = _currentMethod,
                    RetryCount = retryCount,
                    DurationMs = duration,
                    ErrorCode = null,
                    ErrorMessage = null
                };
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw; // ユーザーキャンセルは再スロー
            }
            catch (Exception ex)
            {
                retryCount++;
                
                if (retryCount > maxRetries)
                {
                    var duration = (int)(DateTime.UtcNow - startTime).TotalMilliseconds;
                    return new ScreenshotResult
                    {
                        ImageData = Array.Empty<byte>(),
                        Timestamp = startTime,
                        Method = _currentMethod,
                        RetryCount = retryCount - 1,
                        DurationMs = duration,
                        ErrorCode = "Err-CAP",
                        ErrorMessage = $"キャプチャに失敗しました: {ex.Message}"
                    };
                }

                // リトライ前にバックオフ
                await Task.Delay(backoffMs, cancellationToken);
            }
        }

        // 到達しないはずだが、コンパイラエラー回避
        throw new InvalidOperationException("予期しないキャプチャ失敗");
    }

    private async Task<byte[]> CaptureWithDesktopDuplicationAsync(CancellationToken cancellationToken)
    {
        // Desktop Duplication API の実装
        // 現在は簡略化のため NotImplementedException をスロー
        // 実際の実装では DXGI を使用してキャプチャ
        await Task.Delay(1, cancellationToken); // 非同期の形式を維持
        throw new NotImplementedException("Desktop Duplication API は今後の実装で対応予定");
    }

    private async Task<byte[]> CaptureWithGdiFallbackAsync(CancellationToken cancellationToken)
    {
        return await Task.Run(() =>
        {
#pragma warning disable CA1416 // Windows専用APIの使用を許可
            cancellationToken.ThrowIfCancellationRequested();

            // スクリーン解像度を取得
            var screenWidth = GetSystemMetrics(SM_CXSCREEN);
            var screenHeight = GetSystemMetrics(SM_CYSCREEN);

            // GDI リソース作成
            var screenDC = CreateDC("DISPLAY", null!, null!, IntPtr.Zero);
            var memoryDC = CreateCompatibleDC(screenDC);
            var bitmap = CreateCompatibleBitmap(screenDC, screenWidth, screenHeight);
            var oldBitmap = SelectObject(memoryDC, bitmap);

            try
            {
                // スクリーンをメモリDCにコピー
                BitBlt(memoryDC, 0, 0, screenWidth, screenHeight, screenDC, 0, 0, SRCCOPY);

                // Bitmap オブジェクトに変換してPNGとして保存
                using var managedBitmap = Bitmap.FromHbitmap(bitmap);
                using var stream = new MemoryStream();
                managedBitmap.Save(stream, ImageFormat.Png);
                return stream.ToArray();
            }
            finally
            {
                // GDI リソースをクリーンアップ
                SelectObject(memoryDC, oldBitmap);
                DeleteDC(memoryDC);
                DeleteDC(screenDC);
            }
#pragma warning restore CA1416
        }, cancellationToken);
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            // 必要に応じてリソースクリーンアップ
            _disposed = true;
        }
    }
}