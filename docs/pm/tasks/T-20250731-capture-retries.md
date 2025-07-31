---
id: T-20250731-capture-retries
title: "Input capture records screenshot with retries"
priority: High
depends_on: []
branch: "feature/T-20250731-capture-retries"
uat_file: "../../user_tests/T-20250731-capture-retries.md"
source: requirement
covers: [R-004]
---

## 📋 背景
- 要件定義: [[requirement.md]]
- R-004 では入力イベント受信から50ms以内にPNGを取得し、失敗時は10msバックオフで最大2回リトライし、最終失敗時Err-CAPを記録する必要がある。
- ScreenshotProviderService は存在するが InputRecorderService との連携およびリトライ処理が未完成。

## ✅ Acceptance Criteria
1. InputRecorderService が入力イベント受信直後に IScreenshotProvider.CaptureAsync を呼び出す。
2. CaptureAsync の失敗時は最大2回リトライし、Err-CAP が ScreenshotResult.ErrorCode に設定されることをテストで確認する。
3. 50ms以内に画像取得が成功した場合は InputEventArgs.Screenshot が非 null である。
4. ユニットテストで上記動作を検証し、カバレッジ90%以上を維持する。

## 🔧 Implementation Steps (suggested)
- [ ] ScreenshotProviderService をモックできるようインターフェース実装を整理
- [ ] InputRecorderService.SimulateInputCapture 内で screenshotProvider.CaptureAsync を呼ぶ
- [ ] 失敗時のリトライと Err-CAP 記録処理を追加
- [ ] 新規テスト `ScreenshotCaptureTests` を作成
- [ ] ドキュメント・サンプル更新

## 🧪 Integration-Test Notes
- **UAT ファイル** (↑ uat_file) に示した手順が通ること
