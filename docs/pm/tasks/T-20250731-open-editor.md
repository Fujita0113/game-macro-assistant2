---
id: T-20250731-open-editor
title: "Open visual editor automatically after recording"
priority: Medium
depends_on: [T-20250731-capture-retries]
branch: "feature/T-20250731-open-editor"
uat_file: "../../user_tests/T-20250731-open-editor.md"
source: requirement
covers: [R-007]
---

## 📋 背景
- 要件定義: [[requirement.md]]
- 現在、録画停止後にエディタタブは自動で開かないためR-007を満たしていない。

## ✅ Acceptance Criteria
1. StopRecordingCommand 実行後、自動的にエディタタブが選択状態になる。
2. 機能追加後も既存のテストがすべて成功する。

## 🔧 Implementation Steps (suggested)
- [ ] MainWindowViewModel.StopRecordingAsync の完了時に選択タブを変更
- [ ] UIテスト `OpenEditorTests` で停止後にエディタタブへ切り替わることを確認
- [ ] ドキュメント更新

## 🧪 Integration-Test Notes
- **UAT ファイル** に示した手順が通ること
