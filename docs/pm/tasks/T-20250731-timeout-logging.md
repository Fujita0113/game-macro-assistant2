---
id: T-20250731-timeout-logging
title: "Handle playback timeout with detailed log"
priority: Medium
depends_on: [T-20250731-playback-logging]
branch: "feature/T-20250731-timeout-logging"
uat_file: "../../user_tests/T-20250731-timeout-logging.md"
source: requirement
covers: [R-015]
---

## 📋 背景
- タイムアウト時のエラー通知およびログ出力が未実装。

## ✅ Acceptance Criteria
1. マクロ再生中に指定時間を超過するとエラーコード付きトーストを表示する。
2. `%APPDATA%\\GameMacroAssistant\\Logs\\YYYY-MM-DD.log` にJSON形式の詳細が追記される。
3. 単体テストでファイル出力内容を確認する。

## 🔧 Implementation Steps (suggested)
- [ ] MacroExecutor にタイムアウト判定とログ出力処理を追加
- [ ] WPF通知トーストサービスを実装
- [ ] テスト `PlaybackTimeoutTests` を作成

## 🧪 Integration-Test Notes
- **UAT ファイル** に示した手順が通ること
