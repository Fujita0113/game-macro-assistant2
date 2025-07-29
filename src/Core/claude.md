<claude>
  <スコープ>Core 層 — ドメインロジック / サービス</スコープ>

  <規約>
    - UI 依存を持たない純粋な .NET クラス  
    - すべての非同期 API は `CancellationToken` を取る  
    - 単体テストカバレッジ 90 %以上 (xUnit)  
    - 時間単位は **ミリ秒 (int)** で統一
  </規約>

  <TODO>
    - `IInputRecorder` インターフェース実装  
    - `IScreenshotProvider` で Desktop Duplication を抽象化
  </TODO>
</claude>
