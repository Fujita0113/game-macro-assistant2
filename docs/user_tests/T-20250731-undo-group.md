---
task_id: T-20250731-undo-group
---

# 👥 User Acceptance Test – T-20250731-undo-group

## 手順
1. `dotnet test` を実行する。
2. `UndoGroupingTests` が成功することを確認する。

## 期待結果
- 2秒以内に行った複数操作が1つのUndoとして扱われることをテストが示す。
