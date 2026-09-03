$ErrorActionPreference = "Stop"

$xlsx = (Get-ChildItem "$env:USERPROFILE\Desktop" -Filter "*.xlsx" |
  Where-Object { $_.Name -like "*Clicer*" } | Select-Object -First 1).FullName
if (-not $xlsx) { throw "xlsx not found" }

$div = 18000.0
$exp = [math]::Log(2) / [math]::Log(6)
$inv = 1.0 / $exp
$xpScale = 3000.0
$baseFx = 0.01
$trees = 0.15
$expLit = $exp.ToString([System.Globalization.CultureInfo]::InvariantCulture)

function Get-Pending([double]$gold) {
  if ($gold -lt 1) { return 0 }
  return [math]::Floor([math]::Pow($gold / $div, $exp))
}
function Get-GoldForPending([double]$n) {
  if ($n -lt 1) { return 0 }
  return $div * [math]::Pow($n, $inv)
}
function Get-GoldMult([double]$c) {
  if ($c -lt 1) { return 1.0 }
  return 1.0 + $baseFx + $trees * [math]::Log10(1.0 + $c)
}
function Get-BonusPct([double]$c) { return ((Get-GoldMult $c) - 1.0) * 100.0 }
function Get-Xp([double]$c) {
  if ($c -lt 1) { return 0.0 }
  $log10 = [math]::Log10($c)
  $e = [int][math]::Floor($log10)
  $m = $c / [math]::Pow(10, $e)
  return ($m / 10.0 + $e) * $xpScale
}
function Get-XpToNext([int]$level) {
  if ($level -le 6)  { return 1000 + ($level - 1) * 25 }
  if ($level -le 10) { return 1125 + ($level - 6) * 50 }
  if ($level -le 21) { return 1325 + ($level - 10) * 15 }
  if ($level -le 41) { return 1490 + ($level - 21) * 30 }
  return 2110 + ($level - 41) * 20
}
function Get-CharLevel([double]$c) {
  $xp = Get-Xp $c
  $level = 1
  while ($true) {
    $need = Get-XpToNext $level
    if ($xp -lt $need) { return @{ Level = $level; Into = $xp; Need = $need } }
    $xp -= $need
    $level++
  }
}

$excel = New-Object -ComObject Excel.Application
$excel.Visible = $false
$excel.DisplayAlerts = $false
try {
$wb = $excel.Workbooks.Open($xlsx)

foreach ($name in @("Rebirth","Firestone","FS_Gold","FS_Kamni","FS_Zabegi","FS_Read","FS_Calc","FS_Levels")) {
  foreach ($s in @($wb.Worksheets)) {
    if ($s.Name -eq $name) { $s.Delete(); break }
  }
}

function Add-Sheet($name) {
  $ws = $wb.Worksheets.Add()
  $ws.Name = $name
  return $ws
}

function Set-Num($ws, $r, $c, $n) {
  $lit = ([double]$n).ToString("G17", [System.Globalization.CultureInfo]::InvariantCulture)
  $ws.Cells.Item($r, $c).Formula = "=$lit"
}

function Write-Row($ws, $r, [object[]]$vals, $bold = $false) {
  for ($i = 0; $i -lt $vals.Length; $i++) {
    $cell = $ws.Cells.Item($r, $i + 1)
    $v = $vals[$i]
    if ($v -is [string]) { $cell.Value2 = [string]$v }
    else { $cell.Value2 = [string]([double]$v) }
  }
  if ($bold) { $ws.Range($ws.Cells.Item($r,1), $ws.Cells.Item($r, $vals.Length)).Font.Bold = $true }
}

$ws = Add-Sheet "FS_Read"
Write-Row $ws 1 @("Firestone 1:1 - how to read these sheets") $true
Write-Row $ws 3 @("pending = FLOOR( (goldEarnedThisRun / 18000) ^ (LOG(2)/LOG(6)) )")
Write-Row $ws 4 @("LOG(2)/LOG(6) =", $exp)
Write-Row $ws 5 @("Previous adventure gold is NOT in the formula. Prestige ADDS crystals.")
Write-Row $ws 6 @("Rule: x2 pending crystals = x6 gold this run. Each next crystal in a run costs more.")
Write-Row $ws 7 @("Inverse: gold = 18000 * pending ^ (1 / 0.38685). Inverse exponent =", $inv)
Write-Row $ws 9 @("FS_Calc = yellow cells: type run gold and crystals NOW")
Write-Row $ws 10 @("FS_Gold = gold needed for N crystals, gold for 2N (always ~x6), extra gold for N+1")
Write-Row $ws 11 @("FS_Kamni = lifetime crystals: XP, profile level, forever gold bonus")
Write-Row $ws 12 @("FS_Zabegi = example adventures, crystals stack, run gold resets")
Write-Row $ws 13 @("FS_Levels = gold per kill by zone level AND crystal count")
$ws.Columns.Item(1).ColumnWidth = 92
$ws.Columns.Item(2).ColumnWidth = 22

$ws = Add-Sheet "FS_Calc"
Write-Row $ws 1 @("CALCULATOR - change YELLOW cells") $true
Write-Row $ws 3 @("goldEarnedThisRun (gold from kills THIS adventure)")
Set-Num $ws 3 2 1000000
$ws.Cells.Item(3,2).Interior.Color = 65535
Write-Row $ws 4 @("crystalsNOW (already saved, before button)")
Set-Num $ws 4 2 0
$ws.Cells.Item(4,2).Interior.Color = 65535
$ws.Cells.Item(6,1).Value2 = "pending (crystals you get)"
$ws.Cells.Item(6,2).Formula = "=IF(B3<1,0,INT((B3/18000)^$expLit))"
$ws.Cells.Item(7,1).Value2 = "crystalsAFTER"
$ws.Cells.Item(7,2).Formula = "=B4+B6"
$ws.Cells.Item(8,1).Value2 = "gold to DOUBLE this pending (x6)"
$ws.Cells.Item(8,2).Formula = "=B3*6"
$ws.Cells.Item(9,1).Value2 = "hint: pending >= crystalsNOW (Firestone x2 ping)"
$ws.Cells.Item(9,2).Formula = '=IF(B4<=0,"first prestige",IF(B6>=B4,"x2+ ready","farm more"))'
$ws.Cells.Item(11,1).Value2 = "gold bonus NOW %"
$ws.Cells.Item(11,2).Formula = "=IF(B4<1,0,(0.01+0.15*LOG10(1+B4))*100)"
$ws.Cells.Item(12,1).Value2 = "gold bonus AFTER %"
$ws.Cells.Item(12,2).Formula = "=IF(B7<1,0,(0.01+0.15*LOG10(1+B7))*100)"
$ws.Cells.Item(13,1).Value2 = "DELTA gold bonus % (temple panel)"
$ws.Cells.Item(13,2).Formula = "=B12-B11"
$ws.Cells.Item(13,1).Font.Bold = $true
$ws.Cells.Item(13,2).Interior.Color = 5296274
$ws.Columns.Item(1).ColumnWidth = 62
$ws.Columns.Item(2).ColumnWidth = 22

$ws = Add-Sheet "FS_Gold"
Write-Row $ws 1 @("Gold this run needed to RECEIVE N crystals (inverse formula)") $true
Write-Row $ws 2 @("Col C = gold to DOUBLE those N crystals (= gold for 2N). Col D must stay ~6.") $true
Write-Row $ws 4 @("N crystals","gold for N","gold for 2N (=x6)","ratio x6","gold for N+1","extra gold for +1 stone") $true
$ns = @(1,2,3,4,5,8,10,16,20,32,50,64,100,200,500,1000,2000,5000,10000)
$r = 5
foreach ($n in $ns) {
  $g = Get-GoldForPending $n
  $g2 = Get-GoldForPending ($n * 2)
  $gNext = Get-GoldForPending ($n + 1)
  $ratio = if ($g -gt 0) { $g2 / $g } else { 0 }
  Set-Num $ws $r 1 $n
  Set-Num $ws $r 2 $g
  Set-Num $ws $r 3 $g2
  Set-Num $ws $r 4 $ratio
  Set-Num $ws $r 5 $gNext
  Set-Num $ws $r 6 ($gNext - $g)
  $r++
}
Write-Row $ws ($r+1) @("Column D is always ~6. Column F grows: each next crystal in the same run is more expensive.")
$ws.Columns.Item(1).ColumnWidth = 14
for ($c=2; $c -le 6; $c++) { $ws.Columns.Item($c).ColumnWidth = 22 }

$ws = Add-Sheet "FS_Kamni"
Write-Row $ws 1 @("Lifetime crystals after prestiges - kept forever") $true
Write-Row $ws 2 @("Gold bonus stand-in (no Firestone trees): 1% + 0.15*LOG10(1+crystals)") $true
Write-Row $ws 4 @("crystals total","gold multiplier","bonus %","profile XP","profile level","XP bar") $true
$cs = @(0,1,10,25,50,100,250,500,1000,2500,10000,100000,1000000,10000000,100000000,1000000000,1e12)
$r = 5
foreach ($c in $cs) {
  $mult = Get-GoldMult $c
  $pct = Get-BonusPct $c
  $xp = Get-Xp $c
  $prog = Get-CharLevel $c
  Set-Num $ws $r 1 $c
  Set-Num $ws $r 2 $mult
  Set-Num $ws $r 3 $pct
  Set-Num $ws $r 4 $xp
  Set-Num $ws $r 5 $prog.Level
  $ws.Cells.Item($r,6).Value2 = ("{0:N0} / {1}" -f $prog.Into, $prog.Need)
  $r++
}
$ws.Columns.Item(1).ColumnWidth = 14
$ws.Columns.Item(6).ColumnWidth = 22
for ($c=2; $c -le 6; $c++) { $ws.Columns.Item($c).ColumnWidth = 18 }

$ws = Add-Sheet "FS_Zabegi"
Write-Row $ws 1 @("Example adventures. Crystals stack. Run gold starts from 0 each time.") $true
Write-Row $ws 3 @("run","run gold","pending","crystals before","crystals after","gold bonus delta %","level before","level after","note") $true
$notes = @(
  "first crystal",
  "x6 gold = x2 pending vs 1",
  "typical early farm",
  "x6 vs previous run",
  "about 100 crystals this run",
  "x6 vs 100-crystal run -> about 200"
)
$golds = @(18000.0, 108000.0, 1000000.0, 6000000.0, 1.8e9, 1.08e10)
$crystals = 0.0
$r = 4
for ($i = 0; $i -lt $golds.Length; $i++) {
  $gold = $golds[$i]
  $pending = Get-Pending $gold
  $before = $crystals
  $after = $before + $pending
  $delta = (Get-BonusPct $after) - (Get-BonusPct $before)
  $lvlB = (Get-CharLevel $before).Level
  $lvlA = (Get-CharLevel $after).Level
  Set-Num $ws $r 1 ($i + 1)
  Set-Num $ws $r 2 $gold
  Set-Num $ws $r 3 $pending
  Set-Num $ws $r 4 $before
  Set-Num $ws $r 5 $after
  Set-Num $ws $r 6 $delta
  Set-Num $ws $r 7 $lvlB
  Set-Num $ws $r 8 $lvlA
  $ws.Cells.Item($r,9).Value2 = $notes[$i]
  $crystals = $after
  $r++
}
$ws.Columns.Item(9).ColumnWidth = 42
for ($c=1; $c -le 8; $c++) { $ws.Columns.Item($c).ColumnWidth = 16 }

$ws = Add-Sheet "FS_Levels"
Write-Row $ws 1 @("Gold per kill by zone level and crystals (same as GameFormulas)") $true
Write-Row $ws 2 @('HP = FLOOR(10 * 1.6^(level-1)); boss every 5th *10. Gold = FLOOR(HP/15) * goldMult(crystals)') $true
Write-Row $ws 4 @("level","boss?","HP","gold x1 (0 crystals)","gold @10 crystals","gold @100","gold @1000","gold @1e6 crystals") $true
$levels = @(1,2,4,5,10,15,20,25,50,75,100,150,200,300,500)
$r = 5
foreach ($lv in $levels) {
  $boss = (($lv % 5) -eq 0)
  $hp = 10.0 * [math]::Pow(1.6, $lv - 1)
  if ($boss) { $hp *= 10.0 }
  $hp = [math]::Max(1.0, [math]::Floor($hp))
  $baseGold = [math]::Max(1.0, [math]::Floor($hp / 15.0))
  Set-Num $ws $r 1 $lv
  $ws.Cells.Item($r,2).Value2 = $(if ($boss) { "BOSS" } else { "" })
  Set-Num $ws $r 3 $hp
  Set-Num $ws $r 4 $baseGold
  Set-Num $ws $r 5 ([math]::Floor($baseGold * (Get-GoldMult 10)))
  Set-Num $ws $r 6 ([math]::Floor($baseGold * (Get-GoldMult 100)))
  Set-Num $ws $r 7 ([math]::Floor($baseGold * (Get-GoldMult 1000)))
  Set-Num $ws $r 8 ([math]::Floor($baseGold * (Get-GoldMult 1e6)))
  $r++
}
for ($c=4; $c -le 8; $c++) { $ws.Columns.Item($c).ColumnWidth = 20 }
$ws.Columns.Item(1).ColumnWidth = 10
$ws.Columns.Item(2).ColumnWidth = 10
$ws.Columns.Item(3).ColumnWidth = 16

$wb.Save()
$wb.Close($true)
Write-Output "OK $xlsx"
}
finally {
  if ($excel) { $excel.Quit() }
}
