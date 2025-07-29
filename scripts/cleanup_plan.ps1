# GameMacroAssistant Development Plan Cleanup Script
# 30日を超過したタスクを development_plan.md から削除

param(
    [string]$PlanFile = "development_plan.md",
    [int]$MaxDays = 30
)

Write-Host "🧹 Development Plan Cleanup Script" -ForegroundColor Cyan
Write-Host "Target File: $PlanFile" -ForegroundColor Gray
Write-Host "Max Days: $MaxDays" -ForegroundColor Gray

# ファイル存在確認
if (-not (Test-Path $PlanFile)) {
    Write-Error "❌ File not found: $PlanFile"
    exit 1
}

# 現在日時
$Now = Get-Date
$CutoffDate = $Now.AddDays(-$MaxDays)

Write-Host "📅 Cutoff Date: $($CutoffDate.ToString('yyyy-MM-dd'))" -ForegroundColor Yellow

# ファイル読み込み
$Content = Get-Content $PlanFile -Raw -Encoding UTF8
$Lines = $Content -split "`r?`n"

$ModifiedLines = @()
$RemovedTasks = 0
$TotalTasks = 0

foreach ($Line in $Lines) {
    # タスク行の検出 (- [ ] または - [x] で始まる行)
    if ($Line -match '^(\s*)- \[([ x])\] (.+)$') {
        $TotalTasks++
        $Indent = $Matches[1]
        $Status = $Matches[2]
        $TaskText = $Matches[3]
        
        # 日付パターンを探す (YYYY-MM-DD形式)
        if ($TaskText -match '\b(\d{4}-\d{2}-\d{2})\b') {
            $TaskDateStr = $Matches[1]
            try {
                $TaskDate = [DateTime]::ParseExact($TaskDateStr, "yyyy-MM-dd", $null)
                
                # 30日を超過しているかチェック
                if ($TaskDate -lt $CutoffDate) {
                    Write-Host "🗑️  Removing old task: $($TaskText.Substring(0, [Math]::Min(50, $TaskText.Length)))..." -ForegroundColor Red
                    $RemovedTasks++
                    continue  # この行をスキップ
                }
            }
            catch {
                # 日付パースに失敗した場合はそのまま保持
                Write-Host "⚠️  Invalid date format in task: $TaskDateStr" -ForegroundColor Yellow
            }
        }
    }
    
    # 行を保持
    $ModifiedLines += $Line
}

# ファイル更新
if ($RemovedTasks -gt 0) {
    $NewContent = $ModifiedLines -join "`n"
    
    # Last Update 行を更新
    $NewContent = $NewContent -replace '(\*\*Last Update\*\*: )(\d{4}-\d{2}-\d{2})', "`$1$($Now.ToString('yyyy-MM-dd'))"
    
    # UTF-8 で保存 (BOM なし)
    [System.IO.File]::WriteAllText((Resolve-Path $PlanFile), $NewContent, [System.Text.UTF8Encoding]::new($false))
    
    Write-Host "✅ Cleanup completed!" -ForegroundColor Green
    Write-Host "📊 Total Tasks: $TotalTasks" -ForegroundColor Gray
    Write-Host "🗑️  Removed Tasks: $RemovedTasks" -ForegroundColor Gray
    Write-Host "📁 Updated: $PlanFile" -ForegroundColor Gray
}
else {
    Write-Host "✨ No old tasks found. Plan is up to date!" -ForegroundColor Green
    Write-Host "📊 Total Tasks: $TotalTasks" -ForegroundColor Gray
}

Write-Host ""
Write-Host "💡 To run this script regularly, add to Task Scheduler:" -ForegroundColor Cyan
Write-Host "   powershell.exe -ExecutionPolicy Bypass -File `"$(Resolve-Path $PSCommandPath)`"" -ForegroundColor Gray