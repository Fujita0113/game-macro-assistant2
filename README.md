# GameMacroAssistant

Windows 11向けPCゲーム操作自動化マクロツール

## 概要

GameMacroAssistantは、プログラミング知識がないライトユーザーでも直感的にマクロを作成・編集できるWindows 11専用ツールです。マウス・キーボード操作を記録し、ビジュアルエディタで編集、グローバルホットキーで実行できます。

## 主な機能

- **マクロ記録**: マウス・キーボード操作をリアルタイム記録
- **ビジュアルエディタ**: ドラッグ&ドロップによる直感的な編集
- **画像認識**: SSIM/ピクセル差分による条件分岐
- **グローバルホットキー**: バックグラウンド実行対応
- **セキュリティ**: パスフレーズによるマクロ保護

## 技術スタック

- C# / .NET 8
- WPF (MVVM アーキテクチャ)
- CommunityToolkit.Mvvm
- Desktop Duplication API / GDI BitBlt

## ビルド・実行方法

### 必要環境
- Windows 11 (21H2以降)
- .NET 8 SDK
- Visual Studio 2022 または Visual Studio Code

### ビルド
```powershell
dotnet restore
dotnet build --configuration Release
```

### 実行
```powershell
dotnet run --project src/GameMacroAssistant.Wpf
```

### テスト実行
```powershell
dotnet test --configuration Release --collect:"XPlat Code Coverage"
```

## ライセンス

MIT License

## 開発状況

現在開発中。詳細は `development_plan.md` を参照。