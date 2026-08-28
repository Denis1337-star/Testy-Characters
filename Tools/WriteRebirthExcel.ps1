$ErrorActionPreference = "Stop"
Get-Process Excel -ErrorAction SilentlyContinue | Stop-Process -Force
Start-Sleep -Milliseconds 800

$xlsx = (Get-ChildItem "$env:USERPROFILE\Desktop" -Filter "*.xlsx" |
  Where-Object { $_.Name -like "*Clicer*" } | Select-Object -First 1).FullName
if (-not $xlsx) { throw "Excel file not found" }

function Get-XpToNext([int]$level) {
  if ($level -le 6)  { return 1000 + ($level - 1) * 25 }
  if ($level -le 10) { return 1125 + ($level - 6) * 50 }
  if ($level -le 21) { return 1325 + ($level - 10) * 15 }
  if ($level -le 41) { return 1490 + ($level - 21) * 30 }
  return 2110 + ($level - 42) * 20
}

$excel = New-Object -ComObject Excel.Application
$excel.Visible = $false
$excel.DisplayAlerts = $false
$wb = $excel.Workbooks.Open($xlsx)

foreach ($s in @($wb.Worksheets)) {
  if ($s.Name -eq "Rebirth" -or $s.Name -eq "Prestige" -or $s.Name -eq "FormulasRebirth") {
    $s.Delete()
  }
}

$ws = $wb.Worksheets.Add()
$ws.Name = "Rebirth"

function Set-Cell($r, $c, $v, $bold = $false) {
  $cell = $ws.Cells.Item([int]$r, [int]$c)
  $cell.Value2 = "$v"
  if ($bold) { $cell.Font.Bold = $true }
}

Set-Cell 1 1 "REBIRTH / CRYSTALS (Firestone formulas)" $true
Set-Cell 2 1 "Gold in crystal formula = gold EARNED this run (kills), not gold in pocket."

Set-Cell 4 1 "CONSTANTS" $true
Set-Cell 5 1 "Name"
Set-Cell 5 2 "Value"
Set-Cell 5 3 "Excel / C#"
$ws.Cells.Item(5,1).Font.Bold = $true
$ws.Cells.Item(5,2).Font.Bold = $true

Set-Cell 6 1 "CrystalGoldDivisor"
Set-Cell 6 2 18000
Set-Cell 6 3 "18000"

Set-Cell 7 1 "CrystalExponent"
$ws.Cells.Item(7,2).Formula = "=LOG(2)/LOG(6)"
# Russian Excel uses ';' in functions with 2+ args — LOG has 1 arg, OK.
Set-Cell 7 3 "log(2)/log(6) ~= 0.38685280723"

Set-Cell 8 1 "XpScale"
Set-Cell 8 2 3000
Set-Cell 8 3 "(mantissa/10 + exponent) * 3000"

Set-Cell 9 1 "FirestoneEffectBase"
Set-Cell 9 2 0.01
Set-Cell 9 3 "1% gold (Firestone Effect without trees)"

Set-Cell 10 1 "StandInTreesCoef"
Set-Cell 10 2 0.15
Set-Cell 10 3 "TEMPORARY until research trees exist"

Set-Cell 12 1 "CALCULATOR (edit yellow cells)" $true
Set-Cell 13 1 "goldEarnedThisRun"
Set-Cell 13 2 1000000
$ws.Cells.Item(13,2).Interior.Color = 65535

Set-Cell 14 1 "crystalsNOW (before prestige)"
Set-Cell 14 2 0
$ws.Cells.Item(14,2).Interior.Color = 65535

Set-Cell 16 1 "pendingCrystals"
$ws.Cells.Item(16,2).Formula = "=IF(B13<1,0,INT((B13/B6)^B7))"
Set-Cell 16 3 "floor( (goldEarned/18000) ^ (log2/log6) )"

Set-Cell 17 1 "crystalsAFTER"
$ws.Cells.Item(17,2).Formula = "=B14+B16"

Set-Cell 18 1 "prestigeX vs current crystals"
$ws.Cells.Item(18,2).Formula = "=IF(B14<=0,""first run"",B16/B14)"
Set-Cell 18 3 "hint x2 when pending >= crystalsNOW"

Set-Cell 20 1 "XP FROM TOTAL CRYSTALS" $true
Set-Cell 21 1 "exponent"
$ws.Cells.Item(21,2).Formula = "=IF(B17<1,0,INT(LOG10(B17)))"
Set-Cell 22 1 "mantissa"
$ws.Cells.Item(22,2).Formula = "=IF(B17<1,0,B17/(10^B21))"
Set-Cell 23 1 "totalXp"
$ws.Cells.Item(23,2).Formula = "=IF(B17<1,0,(B22/10+B21)*B8)"
Set-Cell 23 3 "(mantissa/10 + exponent) * 3000"

Set-Cell 25 1 "GOLD MULTIPLIER (stand-in)" $true
Set-Cell 26 1 "goldMult after prestige"
$ws.Cells.Item(26,2).Formula = "=IF(B17<1,1,1+B9+B10*LOG10(1+B17))"
Set-Cell 26 3 "1 + 0.01 + 0.15*log10(1+crystals)"
Set-Cell 27 1 "goldMult now (before)"
$ws.Cells.Item(27,2).Formula = "=IF(B14<1,1,1+B9+B10*LOG10(1+B14))"

Set-Cell 29 1 "EXAMPLES pending" $true
Set-Cell 30 1 "goldEarned"
Set-Cell 30 2 "pending"
$ws.Cells.Item(30,1).Font.Bold = $true
$ws.Cells.Item(30,2).Font.Bold = $true
$examples = @(18000.0, 100000.0, 1000000.0, 10000000.0, 100000000.0, 1000000000.0, 1.0e12)
for ($i = 0; $i -lt $examples.Count; $i++) {
  $row = 31 + $i
  $ws.Cells.Item($row, 1).Value2 = [double]$examples[$i]
  $ws.Cells.Item($row, 2).Formula = "=IF(A$row<1,0,INT((A$row/`$B`$6)^`$B`$7))"
}

Set-Cell 39 1 "CHARACTER LEVEL TABLE (Firestone wiki early levels)" $true
Set-Cell 40 1 "Level"
Set-Cell 40 2 "XpToNext"
Set-Cell 40 3 "CumulativeXpToReachThisLevel"
Set-Cell 40 4 "Bar example"
$ws.Rows.Item(40).Font.Bold = $true

$cum = 0
for ($lvl = 1; $lvl -le 41; $lvl++) {
  $row = 40 + $lvl
  $need = Get-XpToNext $lvl
  $ws.Cells.Item($row, 1).Value2 = $lvl
  $ws.Cells.Item($row, 2).Value2 = $need
  $ws.Cells.Item($row, 3).Value2 = $cum
  if ($lvl -eq 5) { $ws.Cells.Item($row, 4).Value2 = "e.g. 40 / 1100 on level 5" }
  $cum += $need
}

Set-Cell 83 1 "HOW TO READ THE BAR"
Set-Cell 84 1 "totalXp = from crystals (cell B23)"
Set-Cell 85 1 "Start level=1. While totalXp >= XpToNext(level): subtract it, level++."
Set-Cell 86 1 "Remainder = current bar fill. Denominator = XpToNext(current level)."
Set-Cell 87 1 "UI example: level 5 and 40/1100 means remainder 40, need 1100 this level."

Set-Cell 89 1 "RESET ON REBIRTH"
Set-Cell 90 1 "Reset: gold, goldEarnedThisRun, hero levels (player=1), skills, waves, boss, enemy HP"
Set-Cell 91 1 "Keep: crystals (after add pending), profile level (derived from crystals)"

$ws.Columns.Item(1).ColumnWidth = 42
$ws.Columns.Item(2).ColumnWidth = 22
$ws.Columns.Item(3).ColumnWidth = 48
$ws.Columns.Item(4).ColumnWidth = 36

$wb.Save()
$wb.Close($true)
$excel.Quit()
[System.Runtime.Interopservices.Marshal]::ReleaseComObject($wb) | Out-Null
[System.Runtime.Interopservices.Marshal]::ReleaseComObject($excel) | Out-Null
Write-Output "OK $xlsx"
