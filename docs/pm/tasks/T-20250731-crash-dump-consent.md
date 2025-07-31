---
id: T-20250731-crash-dump-consent
title: "Store crash dumps and request consent for upload"
priority: Low
depends_on: [T-20250731-passphrase-check]
branch: "feature/T-20250731-crash-dump-consent"
uat_file: "../../user_tests/T-20250731-crash-dump-consent.md"
source: requirement
covers: [R-019]
---

## 📋 背景
- クラッシュ時のダンプ保存と送信同意が未実装。

## ✅ Acceptance Criteria
1. 例外発生時 `%LOCALAPPDATA%\\...\\CrashDumps\\` にダンプファイルが生成される。
2. 次回起動時、送信確認ダイアログに同意すると指定URLへ送信される(モックで可)。
3. 単体テストでダンプ生成処理を検証する。

## 🔧 Implementation Steps (suggested)
- [ ] App.xaml.cs に全体例外ハンドラを追加
- [ ] ダンプ保存と確認ダイアログの実装
- [ ] テスト `CrashDumpTests` を作成

## 🧪 Integration-Test Notes
- **UAT ファイル** に示した手順が通ること
