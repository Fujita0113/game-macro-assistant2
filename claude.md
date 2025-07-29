<claude>
  <目的>ChatGPT o3 / Claude Code 用の統一開発ガイドライン</目的>

  <!-- ★ 改訂: 日次ログと大計画を分離 -->
  <ワークフロー>
    0. **Create / Update 日次ログ**  
       - `docs/daily/YYYY-MM-DD.md` を当日分として新規作成または追記  
       - _Context / Current Tasks / Blockers / Next Actions_ を必ず記述  
    1. **Update development_plan.md (大局観)**  
       - エピックの % や優先度が変わったら随時反映  
       - 日次ログと重複する詳細は書かない  
    2. **Think → Plan → Code → Test → Commit**  
    3. 計画を提示し、ユーザーの 🟢 *OK* を待つ  
    4. 失敗するテストを先に書く  
    5. 深刻なバグ・設計疑問はpython scripts/create_issue.py --title "<課題のタイトル>" --body "<課題の詳細>" を実行
    6. 要件 v3.2 に矛盾があれば必ず Raise
  </ワークフロー>

  <スタイル>
    - 日本語、箇条書き中心、番号付き手順  
    - C# 8 以降を節度ある範囲で利用  
    - 外部 NuGet は議論必須
  </スタイル>
</claude>
