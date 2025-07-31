---
id: T-20250731-drag-selection
title: "Rectangle selection editing for images"
priority: Medium
depends_on: [T-20250731-open-editor]
branch: "feature/T-20250731-drag-selection"
uat_file: "../../user_tests/T-20250731-drag-selection.md"
source: requirement
covers: [R-008]
---

## 📋 背景
- R-008 では矩形選択による条件画像編集が求められているが、現在UIに未実装。

## ✅ Acceptance Criteria
1. エディタタブでマウスドラッグにより矩形を指定し画像を切り出せる。
2. 選択範囲の情報がモデルに保存されることをユニットテストで確認する。

## 🔧 Implementation Steps (suggested)
- [ ] EditorViewModel 及び対応するビューを作成
- [ ] ReactiveUI のコマンドでドラッグ開始・終了を処理
- [ ] 新規テスト `RectangleSelectionTests` を追加

## 🧪 Integration-Test Notes
- **UAT ファイル** に示した手順が通ること
