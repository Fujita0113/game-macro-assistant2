---
id: T-20250731-undo-group
title: "Implement undo/redo grouping window"
priority: Medium
depends_on: [T-20250731-drag-selection]
branch: "feature/T-20250731-undo-group"
uat_file: "../../user_tests/T-20250731-undo-group.md"
source: requirement
covers: [R-009]
---

## 📋 背景
- Undo/Redo の単位を2秒以内の複合操作でまとめる機能(R-009)が未実装。

## ✅ Acceptance Criteria
1. EditorViewModel に操作履歴管理クラスを追加し、設定で結合時間を0.5-5.0sの範囲で変更可能。
2. デフォルトは2秒であること。
3. 単体テストでグループ化ロジックを検証する。

## 🔧 Implementation Steps (suggested)
- [ ] 設定モデルに `UndoGroupSeconds` を追加
- [ ] 操作履歴クラスを実装し、ReactiveCommand の履歴を管理
- [ ] テスト `UndoGroupingTests` を追加

## 🧪 Integration-Test Notes
- **UAT ファイル** に示した手順が通ること
