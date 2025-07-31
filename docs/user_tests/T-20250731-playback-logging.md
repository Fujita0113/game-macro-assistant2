---
task_id: T-20250731-playback-logging
---

# 👥 User Acceptance Test – T-20250731-playback-logging

## 手順
1. `dotnet test` を実行し `PlaybackTimingTests` が成功することを確認する。

## 期待結果
- テストで再生誤差が基準を超えた場合 Err-TIM がログに記録されることが確認できる。
