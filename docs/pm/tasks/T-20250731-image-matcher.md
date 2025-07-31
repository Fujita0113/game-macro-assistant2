---
id: T-20250731-image-matcher
title: "Implement SSIM/pixel-diff image matcher"
priority: High
depends_on: [T-20250731-hotkey-conflict]
branch: "feature/T-20250731-image-matcher"
uat_file: "../../user_tests/T-20250731-image-matcher.md"
source: requirement
covers: [R-013]
---

## 📋 背景
- 画像一致判定サービスが未実装。R-013ではSSIMとピクセル差分を組み合わせ閾値を設定画面で調整する必要がある。

## ✅ Acceptance Criteria
1. Core層に ImageMatcher クラスを実装し SSIM とピクセル差分の計算を行う。
2. 閾値のデフォルト値は SSIM 0.95, ピクセル差 0.03 である。
3. 設定画面から閾値を変更でき、単体テストで閾値変更が反映されることを確認する。

## 🔧 Implementation Steps (suggested)
- [ ] OpenCvSharp など画像比較ライブラリの導入検討
- [ ] ImageMatcher 実装とインターフェース作成
- [ ] 設定モデルに閾値プロパティを追加
- [ ] テスト `ImageMatcherTests` を作成

## 🧪 Integration-Test Notes
- **UAT ファイル** に示した手順が通ること
