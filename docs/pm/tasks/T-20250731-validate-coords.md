---
id: T-20250731-validate-coords
title: "Validate parameter coordinates within display bounds"
priority: Medium
depends_on: [T-20250731-dnd-order]
branch: "feature/T-20250731-validate-coords"
uat_file: "../../user_tests/T-20250731-validate-coords.md"
source: requirement
covers: [R-011]
---

## 📋 背景
- 現在エディタで任意の数値を保存できるが、R-011ではプライマリディスプレイ解像度内に制限する必要がある。

## ✅ Acceptance Criteria
1. 座標入力欄に解像度外の値を入力すると保存ボタンが無効になる。
2. 単体テストで検証ロジックを確認。

## 🔧 Implementation Steps (suggested)
- [ ] Display解像度を取得するユーティリティを追加
- [ ] EditorViewModel に検証処理を組み込み
- [ ] テスト `CoordinateValidationTests` を追加

## 🧪 Integration-Test Notes
- **UAT ファイル** に示した手順が通ること
