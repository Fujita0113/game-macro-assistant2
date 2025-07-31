---
id: T-{{date:YYYYMMDD}}-{{slug}}
title: "{{短い要約}}"
priority: High | Medium | Low
depends_on: [T-20250729-foo, …]
branch: "feature/{{id}}"          # 🆕 開発ブランチ名を明示
uat_file: "../../user_tests/{{id}}.md"
source: requirement | user_feedback
---

## 📋 背景
- 要件定義: [[requirements/….md]]

## ✅ Acceptance Criteria
1. …

## 🔧 Implementation Steps (suggested)
- [ ] …

## 🧪 Integration-Test Notes
- **UAT ファイル** (↑ `uat_file`) に示した手順が通ること
