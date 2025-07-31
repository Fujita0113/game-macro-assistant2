---
task_id: T-20250731-crash-dump-consent
---

# 👥 User Acceptance Test – T-20250731-crash-dump-consent

## 手順
1. 開発環境で強制的に例外を発生させアプリを終了させる。
2. 起動し直すと送信確認ダイアログが表示されるか確認し、キャンセルを選択する。

## 期待結果
- CrashDumps フォルダにダンプが保存され、ダイアログで送信可否を選べる。
