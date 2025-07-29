<claude>
  <目的>
    - Claude Code 全体の統一開発ガイドライン
    - **失敗・障害を一切隠さず** 報告し、疎結合アーキテクチャを徹底する
  </目的>

  <!-- ───── ワークフロー ───── -->
  <ワークフロー>
    0. **Create / Update 日次ログ**
       - `docs/daily/YYYY-MM-DD.md` を新規作成または追記
       - 必須セクション: Context / Current Tasks / Blockers / Next Actions / **Result**
       - その日のビルド・テスト結果 (成功 / 失敗テスト一覧) を Result に必ず書く

    1. **Update development_plan.md (大局観)**
       - エピック進捗 %、優先度、Blockers、Issue 番号を反映
       - 詳細経緯は日次ログにのみ記録し重複させない

    2. **Think → Plan → Code → Test → Commit**
       - まず「計画」を提示しユーザーの 🟢 *OK* を待つ
       - 失敗するテストを先に書く (TDD)
       - `git add` → `git commit -m "feat: …"` → `git push`

    3. **透明性チェック (必須)**
       - テスト・静的解析・カバレッジ・パフォーマンスの全レポートを取得
       - 失敗・警告が 1 件でもあれば  
         a. `docs/daily/YYYY-MM-DD.md` の Result に赤字記入  
         b. **Never** マージ完了や「Done」と宣言しない  

    4. **自動 Issue 生成トリガ**
       - 以下のいずれかで `python scripts/create_issue.py` を実行
         1. **同一テストを 3 回連続で失敗**  
            `python scripts/create_issue.py "Test XYZ failing 3x"`
         2. **バグ or 実装が 2 ターンで決着しない**  
            詳細 Markdown を `docs/issue_drafts/*.md` に書き  
            `python scripts/create_issue.py "Complex bug" -f docs/…`
       - 生成された Issue 番号を development_plan.md の Blockers に追記

    5. **要件整合性チェック**
       - 現行要件: **v3.3**
       - 矛盾・不足を発見したら「❓ ask‑codex」Issueを即時作成して相談

  </ワークフロー>

  <!-- ───── 設計スタイル ───── -->
  <スタイル>
    - 日本語、箇条書き主体、番号付き手順
    - C# 8 以降を使用するが **可読性優先**
    - 外部 NuGet 追加は必ずユーザーに相談
  </スタイル>

  <!-- ───── アーキテクチャ制約 ───── -->
  <制約>
    1. **疎結合 / SOLID**
       - 依存関係逆転 (DIP) を適用。`Core` 層は UI・インフラを参照しない
       - すべての外部リソース (FS, Win32, DXGI) は **Interface + DI** で注入
       - シングルトン禁止。必要なら `IServiceProvider` 経由

    2. **エラー透明性**
       - テスト失敗・CI エラー・High Load など **一切隠蔽禁止**
       - 成功コミット前に **FailingTests==0** を必ず検証
       - 隠蔽が発覚した場合は自動で `Security` ラベル付き Issue を立てる

    3. **自動 Issue 生成**
       - `scripts/create_issue.py` だけで機能 (追加設定不要)
       - GitHub Token は `GH_TOKEN` Secret として保存済み前提

    4. **日次ログ必須**
       - その日の「成功 / 失敗 / 残課題」を必ず残す
       - 空ログ commit はレビュー拒否

    5. **テスト & カバレッジ**
        - `dotnet test --settings coverlet.runsettings`でカバレッジ測定
         - **モジュール別しきい値**: Core 90%+, CLI 90%+  (coverlet.runsettings で設定)
         - 新しい実装ファイルができた場合は `coverlet.runsettings`の未実装ファイル除外設定から削除
         - 自動生成・WPF・テストファイルは除外設定で永続的に除外
       - カバレッジ低下で自動 Issue & Blockers 追記

    6. **パフォーマンスガード**
       - `perf` テストで CPU・RAM 閾値を検証。超過でビルド失敗とする
  </制約>
</claude>
