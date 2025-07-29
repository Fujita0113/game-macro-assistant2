# GameMacroAssistant アーキテクチャ設計

## システム概要

GameMacroAssistantは、Clean Architectureパターンに基づく3層構造で設計されています。

```
┌─────────────────┐
│   UI Layer      │ ← WPF Views/ViewModels (MVVM)
├─────────────────┤
│   Core Layer    │ ← Business Logic / Domain Models  
├─────────────────┤
│ Windows API     │ ← P/Invoke Wrappers
└─────────────────┘
```

## 層間責任

### Core Layer
- **Models**: Macro, MacroStep, InputEvent等のドメインオブジェクト
- **Services**: IMacroRecorder, IImageMatcher等のビジネスロジック
- **Windows API**: マウス・キーボードフック、画面キャプチャのP/Invokeラッパー

### UI Layer  
- **Views**: WPF UserControl/Window (XAML)
- **ViewModels**: MVVM パターンによるデータバインディング
- **Converters**: UI表示用データ変換

### Tests Layer
- **Unit Tests**: 各層の単体テスト
- **Integration Tests**: API連携・ファイルI/O検証
- **UI Tests**: WinAppDriver による自動化テスト

## 主要コンポーネント設計

### 1. マクロ記録システム
```csharp
interface IMacroRecorder {
    Task StartRecordingAsync();
    Task StopRecordingAsync();  
    event EventHandler<InputEventArgs> InputCaptured;
}
```

### 2. 画面キャプチャシステム  
```csharp
interface IScreenCaptureService {
    Task<ScreenCapture> CaptureAsync(int timeoutMs = 50);
    bool IsDesktopDuplicationAvailable { get; }
}
```

### 3. 画像比較エンジン
```csharp
interface IImageMatcher {
    double CalculateSSIM(byte[] image1, byte[] image2);
    double CalculatePixelDifference(byte[] image1, byte[] image2);
    bool IsMatch(byte[] template, byte[] target, double threshold);
}
```

## データフロー

1. **記録時**: Input Hook → Event Capture → Screen Capture → Serialization
2. **編集時**: Deserialization → Visual Blocks → User Edit → Serialization  
3. **再生時**: Deserialization → Timing Control → Input Simulation → Image Matching

## 非機能要件

### パフォーマンス
- CPU使用率 ≤15% (R-020)
- メモリ使用量 ≤300MB (R-020) 
- 画面キャプチャ ≤50ms (R-004)
- 再生誤差 平均≤5ms, 最大≤15ms (R-014)

### セキュリティ
- マクロファイル暗号化 (AES-256)
- パスフレーズ 8桁以上必須 (R-018)
- メモリ内機密情報の安全な消去

### 可用性
- Desktop Duplication API 失敗時 GDI フォールバック (R-006)
- 最大2回リトライ + バックオフ (R-004)
- グレースフル degradation

## 拡張性

### プラグインアーキテクチャ (将来)
```csharp
interface IMacroPlugin {
    string Name { get; }
    Task<MacroStep> ProcessStepAsync(MacroStep input);
}
```

### 多言語対応
- リソースファイル (.resx) による国際化
- 現在対応: 日本語
- 将来対応: 英語、中国語（簡体字）

## 技術選定理由

- **C# / .NET 8**: Windows ネイティブ統合、豊富なライブラリ
- **WPF**: 柔軟なUI表現、データバインディング
- **MVVM**: テスタビリティ、関心の分離
- **CommunityToolkit.Mvvm**: ボイラープレート削減