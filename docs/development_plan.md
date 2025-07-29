# GameMacroAssistant 開発計画

**Last Update**: 2025-07-29  
**Overall Progress**: 15%

## Phase 1: Core Infrastructure (40%)
- [x] プロジェクト構造セットアップ
- [x] Core レイヤーの基本クラス定義 (InputEvent, IInputRecorder)
- [x] CLI プロジェクト追加 (headlessモード対応)
- [x] MacroExecutor 基本実装
- [ ] Windows API ラッパー (マウス・キーボードフック)
- [ ] Desktop Duplication API 実装
- [ ] 画像比較エンジン (SSIM/ピクセル差分)

## Phase 2: Recording System (0%)
- [ ] マクロ記録サービス実装
- [ ] スクリーンキャプチャとフォールバック機能
- [ ] イベントシリアライゼーション (.gma.json)
- [ ] エラーハンドリングとログ出力

## Phase 3: Visual Editor (0%)
- [ ] WPF UI フレームワークセットアップ
- [ ] ブロックベース編集インターフェース
- [ ] ドラッグ&ドロップ機能
- [ ] Undo/Redo システム
- [ ] 条件画像編集機能

## Phase 4: Playback Engine (0%)
- [ ] マクロ実行エンジン
- [ ] グローバルホットキー登録
- [ ] タイミング制御と誤差測定
- [ ] タイムアウトとエラー処理

## Phase 5: Settings & Security (0%)
- [ ] 設定画面 (閾値、ホットキー等)
- [ ] パスフレーズ暗号化システム
- [ ] DPI スケーリング対応
- [ ] アクセシビリティサポート

## Phase 6: Quality Assurance (0%)
- [ ] UIテスト自動化 (WinAppDriver)
- [ ] パフォーマンス監視
- [ ] クラッシュレポート機能
- [ ] テストカバレッジ 80%以上

## Phase 7: Documentation & Release (0%)
- [ ] ユーザーマニュアル
- [ ] API ドキュメント
- [ ] インストーラー作成
- [ ] リリース準備

## 重要マイルストーン
- **MVP完了**: Phase 1-4 (予定: 未定)
- **ベータリリース**: Phase 1-6 (予定: 未定)
- **正式リリース**: Phase 1-7 (予定: 未定)

## リスク・課題
- Windows API の権限制限
- DPI スケーリング環境での座標変換
- 画像認識の精度調整
- パフォーマンス最適化

## Blockers
- ~~**Issue #3**: コードカバレッジが80%未満~~ ✅ **解決済み** (Core: 92.3%, CLI: 90.9%)

## 次のアクション
1. **優先**: Windows API フック実装調査・実装
2. Desktop Duplication API のプロトタイプ作成
3. 画像比較エンジン (SSIM/ピクセル差分) の設計

## 今日の成果
- **Issue #1**: headlessフラグ機能の実装完了・マージ
- **InputRecorderService**: 具体実装完了 (92.98%カバレッジ)
- **CLI対応**: コマンドライン実行とUI実行の両対応
- **自律issue立て**: create_issue.py による自動Issue作成機能確認
- **透明性確保**: カバレッジ不足検出 → Issue #3 自動作成

---
*このファイルは scripts/cleanup_plan.ps1 により自動メンテナンスされます*