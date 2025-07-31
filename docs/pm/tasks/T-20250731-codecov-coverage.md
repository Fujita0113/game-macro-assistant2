---
id: T-20250731-codecov-coverage
title: "Upload coverage results to Codecov"
priority: Medium
depends_on: [T-20250731-winappdriver-setup]
branch: "feature/T-20250731-codecov-coverage"
uat_file: "../../user_tests/T-20250731-codecov-coverage.md"
source: requirement
covers: [R-024]
---

## 📋 背景
- カバレッジ計測は行っているがCodecovへの送信が未設定。

## ✅ Acceptance Criteria
1. windows-ci.yml で Coverlet + ReportGenerator の出力を取得し、CODECOV_TOKEN を用いて Codecov へアップロードするステップがある。
2. Core レイヤーのカバレッジが80%以上でCIが成功する。
3. 成功したアップロードを確認できるスクリーンショットまたはログを添付する。

## 🔧 Implementation Steps (suggested)
- [ ] ワークフローにカバレッジ測定と Codecov upload ステップを追加
- [ ] coverlet.runsettings の除外設定を見直し
- [ ] テスト実行で閾値を下回った場合はジョブ失敗とする

## 🧪 Integration-Test Notes
- **UAT ファイル** に示した手順が通ること
