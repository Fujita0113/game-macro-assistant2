using System;
using System.Threading;
using System.Threading.Tasks;
using GameMacroAssistant.Core.Models;

namespace GameMacroAssistant.Core.Services;

/// <summary>
/// Windows API フック（マウス・キーボード）による入力監視
/// R-001, R-002: 低遅延フック + グローバル入力監視
/// </summary>
public interface IWindowsApiHook : IDisposable
{
    /// <summary>
    /// フックが現在アクティブかどうか
    /// </summary>
    bool IsHookActive { get; }

    /// <summary>
    /// 入力イベントが発生した時のイベント
    /// </summary>
    event EventHandler<InputEvent>? InputDetected;

    /// <summary>
    /// マウス・キーボードフックを開始
    /// R-001: 5ms以内の低遅延で入力をキャプチャ
    /// </summary>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>フック開始のTask</returns>
    Task StartHookAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// フックを停止
    /// </summary>
    /// <returns>フック停止のTask</returns>
    Task StopHookAsync();

    /// <summary>
    /// 特定の入力を一時的に無視（マクロ実行中の自分自身の入力を除外）
    /// </summary>
    /// <param name="suppressDurationMs">抑制時間（ミリ秒）</param>
    void SuppressInput(int suppressDurationMs);
}