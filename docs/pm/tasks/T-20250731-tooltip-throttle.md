---
id: T-20250731-tooltip-throttle
title: "Throttle tooltip updates to 10FPS"
priority: Low
depends_on: [T-20250731-timeout-logging]
branch: "feature/T-20250731-tooltip-throttle"
uat_file: "../../user_tests/T-20250731-tooltip-throttle.md"
source: requirement
covers: [R-016]
---

## 📋 背景
- ステップ完了時のツールチップ更新が無制限で行われており、UIがちらつく恐れがある。

## ✅ Acceptance Criteria
1. Tooltip更新処理にレート制限を設け、1秒間に10回を超えて更新しない。
2. 単体テストで一定間隔より短い更新要求が無視されることを検証する。

## 🔧 Implementation Steps (suggested)
- [ ] ReactiveUI の Throttle を使い更新処理を実装
- [ ] テスト `TooltipThrottleTests` を追加

## 🧪 Integration-Test Notes
- **UAT ファイル** に示した手順が通ること
