$ErrorActionPreference = "Stop"

$root = "C:\GameTest\Testy Characters"
$csvPath = Join-Path $root "hero_import.csv"
$namesPath = Join-Path $root "Tools\hero-skill-names.json"
$heroesDir = Join-Path $root "Assets\Configs\Heros"
$catalogPath = Join-Path $root "Assets\Configs\Heroes Config.asset"
$iconsDir = Join-Path $root "Assets\500FreeSkillIcons\Icons"
$heroScriptGuid = "4deb44c3f6715be4097d0afa124cdd70"
$catalogScriptGuid = "31eaa72c10a6f5141a24ff1142ca0d84"

$unlocks = @(10, 25, 50, 75, 100, 125)
$bonuses = @(1.0, 1.0, 1.0, 1.5, 2.0, 2.5)
$percents = @(100, 100, 100, 150, 200, 250)

$namesJson = Get-Content $namesPath -Raw -Encoding UTF8 | ConvertFrom-Json

$packs = @{
  Castle  = "f5f1c19ce87450f4b97f8b363746558d"
  Village = "e48ddf4fb7304ce4ab48711eb6617a57"
  Forest  = "b08e570699f1c8a4f9cc388204aef3bc"
  Sea     = "5cf1b4629c7cb104e98d369448084abd"
}

function Get-PackFileIds([string]$metaPath) {
  $map = @{}
  foreach ($line in Get-Content $metaPath) {
    if ($line -match "^\s+(Pack\w+_(\d+)):\s+(-?\d+)\s*$") {
      $map[[int]$Matches[2]] = [int64]$Matches[3]
    }
  }
  return $map
}

$packIds = @{
  Castle  = Get-PackFileIds (Join-Path $root "Assets\Sprite\Heros\PackCastle01.png.meta")
  Village = Get-PackFileIds (Join-Path $root "Assets\Sprite\Heros\PackVillage01.png.meta")
  Forest  = Get-PackFileIds (Join-Path $root "Assets\Sprite\Heros\PackForest01.png.meta")
  Sea     = Get-PackFileIds (Join-Path $root "Assets\Sprite\Heros\PackSea01.png.meta")
}

function ConvertTo-UnityEscaped([string]$s) {
  if ($null -eq $s) { return "" }
  $sb = New-Object System.Text.StringBuilder
  foreach ($ch in $s.ToCharArray()) {
    $code = [int]$ch
    if ($code -gt 127) { [void]$sb.AppendFormat('\u{0:X4}', $code) }
    elseif ($ch -eq [char]92) { [void]$sb.Append('\\') }
    elseif ($ch -eq '"') { [void]$sb.Append('\"') }
    else { [void]$sb.Append($ch) }
  }
  return $sb.ToString()
}

$iconFiles = @(Get-ChildItem $iconsDir -Filter "skill_*.png" | Sort-Object { [int]($_.BaseName -replace '\D','') } | ForEach-Object { $_.BaseName })
if ($iconFiles.Count -lt 354) { throw "Need 354 skill icons, found $($iconFiles.Count)" }

function Get-SkillGuidByIndex([int]$zeroBased) {
  $base = $iconFiles[$zeroBased]
  $meta = Join-Path $iconsDir ($base + ".png.meta")
  $line = (Get-Content $meta | Select-Object -Skip 1 -First 1)
  if ($line -notmatch "guid:\s+([a-f0-9]+)") { throw "No guid in $meta" }
  return @{ Guid = $Matches[1]; File = $base + ".png" }
}

function Format-Double([double]$v) {
  return $v.ToString("G15", [System.Globalization.CultureInfo]::InvariantCulture)
}

$inv = [System.Globalization.CultureInfo]::InvariantCulture
$rows = Import-Csv $csvPath -Encoding UTF8
$heroGuids = @{}

Get-ChildItem $heroesDir -Filter "Hero_*.asset.meta" | ForEach-Object {
  if ($_.Name -match "Hero_(\d+)\.asset\.meta") {
    $idx = [int]$Matches[1]
    $g = (Get-Content $_.FullName | Select-Object -Skip 1 -First 1)
    if ($g -match "guid:\s+([a-f0-9]+)") { $heroGuids[$idx] = $Matches[1] }
  }
}

$skillRows = New-Object System.Collections.Generic.List[object]
$catalogHeroLines = New-Object System.Collections.Generic.List[string]

for ($i = 0; $i -lt $rows.Count; $i++) {
  $row = $rows[$i]
  $index = [int]$row.Index
  $name = ([string]$row.Name).Trim()
  if ([string]::IsNullOrWhiteSpace($name)) { $name = [string]$namesJson.fallbackName }

  $costText = ([string]$row.BaseCost).Replace(" ", "").Replace(",", ".")
  $powerText = ([string]$row.BasePower).Replace(" ", "").Replace(",", ".")
  $baseCost = [double]::Parse($costText, $inv)
  $basePower = [double]::Parse($powerText, $inv)

  $propNames = @($row.PSObject.Properties.Name)
  $spriteRaw = ([string]$row.($propNames[4])) -replace "[\u00A0\s]", ""
  if ($spriteRaw -notmatch "^(Castle|Village|Forest|Sea)_(\d+)$") {
    throw "Bad sprite '$spriteRaw' for hero index $index"
  }
  $packName = $Matches[1]
  $spriteIdx = [int]$Matches[2]
  $fileId = $packIds[$packName][$spriteIdx]
  if ($null -eq $fileId) { throw "Missing sprite $packName $spriteIdx" }
  $packGuid = $packs[$packName]

  $isClick = ($index -eq 0)
  $isClickYaml = if ($isClick) { "1" } else { "0" }
  $statWord = if ($isClick) { [string]$namesJson.clickStat } else { [string]$namesJson.dpsStat }

  if ($isClick) { $skillNames = @($namesJson.click) }
  elseif ($packName -eq "Castle") { $skillNames = @($namesJson.Castle) }
  elseif ($packName -eq "Village") { $skillNames = @($namesJson.Village) }
  elseif ($packName -eq "Forest") { $skillNames = @($namesJson.Forest) }
  else { $skillNames = @($namesJson.Sea) }

  $heroNum = $index + 1
  if (-not $heroGuids.ContainsKey($heroNum)) {
    $heroGuids[$heroNum] = [guid]::NewGuid().ToString("N")
  }
  $heroGuid = $heroGuids[$heroNum]

  $skillYaml = New-Object System.Collections.Generic.List[string]
  for ($s = 0; $s -lt 6; $s++) {
    $iconInfo = Get-SkillGuidByIndex ($index * 6 + $s)
    $iconGuid = $iconInfo.Guid
    $iconFile = $iconInfo.File
    $unlock = $unlocks[$s]
    $bonus = $bonuses[$s]
    $pct = $percents[$s]
    $skillCost = [math]::Floor($baseCost * [math]::Pow(1.07, $unlock) * 10.0)
    $skillName = [string]$skillNames[$s]
    $desc = ([string]$namesJson.descPrefix) + $statWord + ([string]$namesJson.descMid) + $name + ([string]$namesJson.descPctPre) + "$pct" + ([string]$namesJson.descSuffix)

    [void]$skillYaml.Add("  - Name: `"$(ConvertTo-UnityEscaped $skillName)`"")
    [void]$skillYaml.Add("    Description: `"$(ConvertTo-UnityEscaped $desc)`"")
    [void]$skillYaml.Add("    Icon: {fileID: 21300000, guid: $iconGuid, type: 3}")
    [void]$skillYaml.Add("    UnlockLevel: $unlock")
    [void]$skillYaml.Add("    Cost: $(Format-Double $skillCost)")
    [void]$skillYaml.Add("    DamageBonus: $(Format-Double $bonus)")

    $skillRows.Add([pscustomobject]@{
      HeroIndex    = $index
      HeroName     = $name
      SkillIndex   = $s + 1
      Name         = $skillName
      Description  = $desc
      Icon         = $iconFile
      UnlockLevel  = $unlock
      Cost         = $skillCost
      DamageBonus  = $bonus
    })
  }

  $nl = [Environment]::NewLine
  $asset = "%YAML 1.1" + $nl +
    "%TAG !u! tag:unity3d.com,2011:" + $nl +
    "--- !u!114 &11400000" + $nl +
    "MonoBehaviour:" + $nl +
    "  m_ObjectHideFlags: 0" + $nl +
    "  m_CorrespondingSourceObject: {fileID: 0}" + $nl +
    "  m_PrefabInstance: {fileID: 0}" + $nl +
    "  m_PrefabAsset: {fileID: 0}" + $nl +
    "  m_GameObject: {fileID: 0}" + $nl +
    "  m_Enabled: 1" + $nl +
    "  m_EditorHideFlags: 0" + $nl +
    "  m_Script: {fileID: 11500000, guid: $heroScriptGuid, type: 3}" + $nl +
    "  m_Name: Hero_$heroNum" + $nl +
    "  m_EditorClassIdentifier: Assembly-CSharp::HeroConfig" + $nl +
    "  Name: `"$(ConvertTo-UnityEscaped $name)`"" + $nl +
    "  Icon: {fileID: $fileId, guid: $packGuid, type: 3}" + $nl +
    "  IsClickHero: $isClickYaml" + $nl +
    "  BaseCost: $(Format-Double $baseCost)" + $nl +
    "  BasePower: $(Format-Double $basePower)" + $nl +
    "  Skills:" + $nl +
    ($skillYaml -join $nl) + $nl

  $assetPath = Join-Path $heroesDir "Hero_$heroNum.asset"
  $metaPath = Join-Path $heroesDir "Hero_$heroNum.asset.meta"
  [System.IO.File]::WriteAllText($assetPath, $asset)
  if (-not (Test-Path $metaPath)) {
    $meta = "fileFormatVersion: 2" + $nl +
      "guid: $heroGuid" + $nl +
      "NativeFormatImporter:" + $nl +
      "  externalObjects: {}" + $nl +
      "  mainObjectFileID: 11400000" + $nl +
      "  userData: " + $nl +
      "  assetBundleName: " + $nl +
      "  assetBundleVariant: " + $nl
    [System.IO.File]::WriteAllText($metaPath, $meta)
  }

  [void]$catalogHeroLines.Add("  - {fileID: 11400000, guid: $heroGuid, type: 2}")
}

$nl = [Environment]::NewLine
$catalog = "%YAML 1.1" + $nl +
  "%TAG !u! tag:unity3d.com,2011:" + $nl +
  "--- !u!114 &11400000" + $nl +
  "MonoBehaviour:" + $nl +
  "  m_ObjectHideFlags: 0" + $nl +
  "  m_CorrespondingSourceObject: {fileID: 0}" + $nl +
  "  m_PrefabInstance: {fileID: 0}" + $nl +
  "  m_PrefabAsset: {fileID: 0}" + $nl +
  "  m_GameObject: {fileID: 0}" + $nl +
  "  m_Enabled: 1" + $nl +
  "  m_EditorHideFlags: 0" + $nl +
  "  m_Script: {fileID: 11500000, guid: $catalogScriptGuid, type: 3}" + $nl +
  "  m_Name: Heroes Config" + $nl +
  "  m_EditorClassIdentifier: Assembly-CSharp::HeroesConfig" + $nl +
  "  Heroes:" + $nl +
  ($catalogHeroLines -join $nl) + $nl
[System.IO.File]::WriteAllText($catalogPath, $catalog)

$skillsCsv = Join-Path $root "hero_skills_export.csv"
$skillRows | Export-Csv $skillsCsv -NoTypeInformation -Encoding UTF8

Write-Output "HEROES=$($rows.Count)"
Write-Output "SKILLS=$($skillRows.Count)"
Write-Output "CSV=$skillsCsv"
