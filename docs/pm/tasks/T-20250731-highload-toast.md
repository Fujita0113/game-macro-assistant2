---
id: T-20250731-highload-toast
title: "Notify high load conditions"
priority: Low
depends_on: [T-20250731-crash-dump-consent]
branch: "feature/T-20250731-highload-toast"
uat_file: "../../user_tests/T-20250731-highload-toast.md"
source: requirement
covers: [R-020]
---

## 📋 背景
- パフォーマンス監視が未実装でR-020を満たしていない。

## ✅ Acceptance Criteria
1. 実行中CPU使用率が15%超またはメモリ300MB超の場合、進捗バーを赤くし「High Load」トーストを表示する。
2. 負荷が下がると元の色に戻る。
3. テストで監視ロジックを確認する。

## 🔧 Implementation Steps (suggested)
- [ ] System.Diagnostics.PerformanceCounter を利用して監視クラスを作成
- [ ] UI側で進捗バー色変更とトースト表示処理を追加
- [ ] テスト `HighLoadNotificationTests` を実装

## 🧪 Integration-Test Notes
- **UAT ファイル** に示した手順が通ること
