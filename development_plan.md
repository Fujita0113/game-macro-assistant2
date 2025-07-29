# GameMacroAssistant 開発計画

**Last Update**: 2025-07-29  
**Overall Progress**: 0%

## Phase 1: Core Infrastructure (0%)
- [ ] プロジェクト構造セットアップ
- [ ] Core レイヤーの基本クラス定義
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

## 次のアクション
1. Core レイヤーのインターフェース設計
2. Windows API フック実装調査
3. Desktop Duplication API のプロトタイプ作成

---
*このファイルは scripts/cleanup_plan.ps1 により自動メンテナンスされます*