---
id: T-20250731-watermark-fallback
title: "Fallback capture adds watermark and logs Err-CAP"
priority: High
depends_on: [T-20250731-capture-retries]
branch: "feature/T-20250731-watermark-fallback"
uat_file: "../../user_tests/T-20250731-watermark-fallback.md"
source: requirement
covers: [R-006]
---

## 📋 背景
- 要件定義: [[requirement.md]]
- ScreenshotProviderService では GDI ビットブロック転送へフォールバックするが、ウォーターマークやログ記録が未実装。

## ✅ Acceptance Criteria
1. Desktop Duplication API が利用できず GDI にフォールバックした場合、取得した画像に半透明の文字列 "CaptureLimited" が重ねられる。
2. フォールバック時には RetryCount と Err-CAP がログに出力される。
3. フォールバック動作を確認するユニットテストが追加されている。

## 🔧 Implementation Steps (suggested)
- [ ] ScreenshotProviderService.CaptureWithGdiFallbackAsync でウォーターマーク描画を追加
- [ ] フォールバック発生時のログ出力を追加
- [ ] テスト `WatermarkFallbackTests` を実装
- [ ] 更新した実装がカバレッジ基準を満たす

## 🧪 Integration-Test Notes
- **UAT ファイル** に示した手順が通ること
