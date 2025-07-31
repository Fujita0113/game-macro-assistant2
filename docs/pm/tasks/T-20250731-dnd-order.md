---
id: T-20250731-dnd-order
title: "Block drag-and-drop reordering with snapping"
priority: Medium
depends_on: [T-20250731-undo-group]
branch: "feature/T-20250731-dnd-order"
uat_file: "../../user_tests/T-20250731-dnd-order.md"
source: requirement
covers: [R-010]
---

## 📋 背景
- ブロックをドラッグ＆ドロップで順序変更するUIが存在しないためR-010未達。

## ✅ Acceptance Criteria
1. エディタでブロックを上下にドラッグすると垂直リストにスナップして入れ替わる。
2. ドラッグ失敗時は元の位置に戻る。
3. UIテストで一連の操作を確認する。

## 🔧 Implementation Steps (suggested)
- [ ] ListBox のアイテムをドラッグ＆ドロップ可能にするビヘイビアを追加
- [ ] スナップ位置計算とキャンセル処理を実装
- [ ] テスト `BlockReorderTests` を追加

## 🧪 Integration-Test Notes
- **UAT ファイル** に示した手順が通ること
