---
task_id: T-20250731-timeout-logging
---

# 👥 User Acceptance Test – T-20250731-timeout-logging

## 手順
1. `dotnet test` を実行し `PlaybackTimeoutTests` が成功することを確認する。

## 期待結果
- ログファイルにタイムアウトエラーが JSON 形式で追記されることをテストが確認する。
