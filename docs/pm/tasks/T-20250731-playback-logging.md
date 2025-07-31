---
id: T-20250731-playback-logging
title: "Log playback timing errors"
priority: Medium
depends_on: [T-20250731-image-matcher]
branch: "feature/T-20250731-playback-logging"
uat_file: "../../user_tests/T-20250731-playback-logging.md"
source: requirement
covers: [R-014]
---

## 📋 背景
- 再生誤差測定機能がなく、R-014を満たしていない。

## ✅ Acceptance Criteria
1. MacroExecutor が各ステップの実行時間を計測し、平均>5ms または 最大>15ms の場合 Err-TIM をログ出力する。
2. 単体テストで基準超過時にログが記録されることを検証する。

## 🔧 Implementation Steps (suggested)
- [ ] MacroExecutor.RunAsync にタイミング計測処理を追加
- [ ] ログ出力クラスを追加し Err-TIM を記録
- [ ] テスト `PlaybackTimingTests` を実装

## 🧪 Integration-Test Notes
- **UAT ファイル** に示した手順が通ること
