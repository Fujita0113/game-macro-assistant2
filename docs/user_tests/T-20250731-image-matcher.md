---
task_id: T-20250731-image-matcher
---

# 👥 User Acceptance Test – T-20250731-image-matcher

## 手順
1. `dotnet test` を実行し `ImageMatcherTests` を確認する。
2. テスト通過後、設定画面から閾値を 0.90 に変更して保存する。

## 期待結果
- テストが成功し、設定変更後の閾値がアプリ再起動後も保持されている。
