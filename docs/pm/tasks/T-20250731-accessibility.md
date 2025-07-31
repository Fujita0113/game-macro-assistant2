---
id: T-20250731-accessibility
title: "Expose UIA properties for all controls"
priority: Low
depends_on: [T-20250731-highload-toast]
branch: "feature/T-20250731-accessibility"
uat_file: "../../user_tests/T-20250731-accessibility.md"
source: requirement
covers: [R-021]
---

## 📋 背景
- アクセシビリティ対応のためUIAプロパティ公開が必要。

## ✅ Acceptance Criteria
1. 主要なボタン・リスト等にAutomationProperties.NameとHelpTextが設定されている。
2. WinAppDriverテストでプロパティ取得が確認できる。

## 🔧 Implementation Steps (suggested)
- [ ] XAMLにAutomationPropertiesを追加
- [ ] UIテスト `AccessibilityTests` を実装
- [ ] ドキュメント更新

## 🧪 Integration-Test Notes
- **UAT ファイル** に示した手順が通ること
