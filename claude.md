<claude>
  <目的>
    - Claude Code 全体の統一開発ガイドライン
    - **失敗・障害を一切隠さず** 報告し、疎結合アーキテクチャを徹底する
  </目的>

  <!-- ───── ワークフロー ───── -->
  <ワークフロー>
    1. **タスク参照**
       - 以下の必読ファイルを必ず確認
         - `docs/requirements/*.md`（要件定義一式）
         - `docs/pm/tasks/*.md`（★PM が最新で生成したタスク群）
       - 各タスクごとに **Acceptance Criteria** を満たす失敗テストを先に作成
       - 実装 → テストグリーン化 → PR

    2. **Think → Plan → Code → Test → Commit**
       - まず「計画」を提示しユーザーの 🟢 *OK* を待つ
       - 失敗するテストを先に書く (TDD)
       - `git add` → `git commit -m "feat: …"` → `git push`

    3. **透明性チェック (必須)**
       - テスト・静的解析・カバレッジ・パフォーマンスの全レポートを取得
       - 失敗・警告が 1 件でもあれば  
         a. `docs/daily/YYYY-MM-DD.md` の Result に赤字記入  
         b. **Never** マージ完了や「Done」と宣言しない  
    4. **要件カバレッジ表の更新 (必須)**
       - マージ完了後、`docs/trace/requirements_coverage.md` を最新化する  
         a. 自分が実装したタスク *.md の `covers:` を解析  
         b. ステータスを ✅ Done に変更しコミットハッシュを記入  
       - スクリプト例: `python scripts/update_coverage.py`
       - 表のテンプレートを崩さないこと（列順・ヘッダー固定）
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

    7. **実装完了判定ガード**
       - 「実装完了」「Done」「マージ可能」と宣言する条件を以下に限定  
         a. 単体・統合・UI テスト **全緑** (GitHub Actions)  
         b. 手動確認ステップ **Acceptance Checklist** を満たす  
       - Acceptance Checklist は開発者自身が `docs/daily/YYYY-MM-DD.md`
         の Result に貼り付けること  
           - ✅ ボタン押下 → 記録開始 → 停止 → 再生  
           - ✅ 画面遷移にフリーズなし  
           - ✅ High‑Load トーストなし  
       - いずれか NG の場合は自動で  
         `python scripts/create_issue.py "Acceptance failed" -b "$(cat docs/daily/…)"`

    8. **UI 不具合の検知義務**
       - ユーザー(=あなた)が「操作して動かない」と報告した時点で  
         Claude は *必ず* 状況を再現し、Issue を立てるか
         修正 PR を提案するまで **実装完了と宣言しない**  
       - 「無限ループになっています」など原因推測だけで完了扱いは不可
  </制約>
</claude>