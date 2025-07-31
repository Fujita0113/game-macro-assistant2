---
id: T-20250731-winappdriver-setup
title: "Update CI to install WinAppDriver 1.6.2"
priority: Medium
depends_on: [T-20250731-accessibility]
branch: "feature/T-20250731-winappdriver-setup"
uat_file: "../../user_tests/T-20250731-winappdriver-setup.md"
source: requirement
covers: [R-023]
---

## 📋 背景
- Windows CI ワークフローではWinAppDriverのバージョン指定や待機処理が不足している。

## ✅ Acceptance Criteria
1. `.github/workflows/windows-ci.yml` で `choco install winappdriver --version 1.6.2` を実行している。
2. インストール後5秒待機し、UIテスト実行前に対話モードでWinAppDriverを起動する。
3. プルリクCIでUIテストが正常に通る。

## 🔧 Implementation Steps (suggested)
- [ ] ワークフローファイルを修正しバージョン指定と待機処理を追加
- [ ] 試行用UIテストを実行して通ることを確認
- [ ] ドキュメント更新

## 🧪 Integration-Test Notes
- **UAT ファイル** に示した手順が通ること
