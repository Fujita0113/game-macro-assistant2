---
id: T-20250731-passphrase-check
title: "Enforce passphrase policy on macro files"
priority: Medium
depends_on: [T-20250731-tooltip-throttle]
branch: "feature/T-20250731-passphrase-check"
uat_file: "../../user_tests/T-20250731-passphrase-check.md"
source: requirement
covers: [R-018]
---

## 📋 背景
- マクロ読み込み時のパスフレーズ認証が未実装。

## ✅ Acceptance Criteria
1. マクロがパスフレーズで暗号化されている場合、8文字以上の入力を要求するダイアログを表示する。
2. 入力ミスが3回続くと読み込みをキャンセルする。
3. 単体テストで失敗回数カウントロジックを確認する。

## 🔧 Implementation Steps (suggested)
- [ ] 暗号化状態フラグをMacroMetadataに参照
- [ ] 認証ダイアログと復号処理を実装
- [ ] テスト `PassphrasePolicyTests` を追加

## 🧪 Integration-Test Notes
- **UAT ファイル** に示した手順が通ること
