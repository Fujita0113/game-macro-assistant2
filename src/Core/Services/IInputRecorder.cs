using System;
using System.Threading;
using System.Threading.Tasks;
using GameMacroAssistant.Core.Models;

namespace GameMacroAssistant.Core.Services;

/// <summary>
/// マウス・キーボード入力をフックし、イベントを記録するサービス
/// R-002, R-003: 座標・ボタン種別・押下時間・仮想キーコードを記録
/// </summary>
public interface IInputRecorder : IDisposable
{
    /// <summary>
    /// 入力記録が開始されているかどうか
    /// </summary>
    bool IsRecording { get; }

    /// <summary>
    /// 記録停止キー（デフォルト: ESC）
    /// R-005: ユーザーが設定画面で変更可能
    /// </summary>
    int StopRecordingKey { get; set; }

    /// <summary>
    /// 入力イベントがキャプチャされた時に発生
    /// </summary>
    event EventHandler<InputEventArgs>? InputCaptured;

    /// <summary>
    /// マクロ記録を開始
    /// R-001: UI上のボタンクリックで開始
    /// </summary>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>記録開始タスク</returns>
    Task StartRecordingAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// マクロ記録を停止
    /// R-005: デフォルトはESCキー
    /// </summary>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>記録停止タスク</returns>
    Task StopRecordingAsync(CancellationToken cancellationToken = default);
}