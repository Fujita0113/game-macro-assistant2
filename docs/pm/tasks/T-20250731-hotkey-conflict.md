---
id: T-20250731-hotkey-conflict
title: "Detect and resolve global hotkey conflicts"
priority: Medium
depends_on: [T-20250731-validate-coords]
branch: "feature/T-20250731-hotkey-conflict"
uat_file: "../../user_tests/T-20250731-hotkey-conflict.md"
source: requirement
covers: [R-012]
---

## 📋 背景
- グローバルホットキー登録時の競合解決ロジックが未実装。

## ✅ Acceptance Criteria
1. 既存のホットキーと衝突する場合、代替候補を3つ提案するダイアログが表示される。
2. ユーザーが選択して保存するまで登録処理が完了しない。
3. 単体テストで候補提案のロジックを確認する。

## 🔧 Implementation Steps (suggested)
- [ ] ホットキー登録クラスを実装し競合検出機能を追加
- [ ] UIダイアログをReactiveUIで作成
- [ ] テスト `HotkeyConflictTests` を実装

## 🧪 Integration-Test Notes
- **UAT ファイル** に示した手順が通ること
