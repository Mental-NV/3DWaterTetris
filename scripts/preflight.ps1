[CmdletBinding()]
param(
  [string]$BacklogPath = ".agent/backlog.json",
  [switch]$FailOnDirtyTree = $true
)

$ErrorActionPreference = "Stop"

function Fail([string]$msg) {
  Write-Error $msg
  exit 1
}

# 1) git clean check
if ($FailOnDirtyTree) {
  $status = git status --porcelain
  if ($status) { Fail "Working tree is not clean. Commit or stash changes before proceeding." }
}

# 2) backlog exists + parse
if (-not (Test-Path $BacklogPath)) { Fail "Backlog not found at: $BacklogPath" }
$raw = Get-Content $BacklogPath -Raw
$backlog = $raw | ConvertFrom-Json

# Expect either { items: [...] } or [...] (tolerate both)
$items = $null
if ($backlog.PSObject.Properties.Name -contains "items") { $items = $backlog.items } else { $items = $backlog }

if (-not $items) { Fail "Backlog contains no items." }

# 3) invariants: WIP limit
$inProgress = @($items | Where-Object { $_.status -eq "InProgress" })
if ($inProgress.Count -gt 1) { Fail "WIP violation: more than one item is InProgress." }

$done = @($items | Where-Object { $_.status -eq "Done" })
$new  = @($items | Where-Object { $_.status -eq "New" })

# 4) compute NEXT = lowest ID New with all dependsOn Done
$doneIds = New-Object System.Collections.Generic.HashSet[string]
foreach ($d in $done) { [void]$doneIds.Add([string]$d.id) }

function IsEligible($item) {
  if (-not $item.dependsOn) { return $true }
  foreach ($dep in $item.dependsOn) {
    if (-not $doneIds.Contains([string]$dep)) { return $false }
  }
  return $true
}

$eligible = @($new | Where-Object { IsEligible $_ } | Sort-Object { [int]($_.id -replace '\D','') }, id)
$next = if ($eligible.Count -gt 0) { $eligible[0] } else { $null }

# 5) print summary (agent-friendly)
Write-Host "DONE:    $($done.Count)"
if ($inProgress.Count -eq 1) {
  $cur = $inProgress[0]
  Write-Host "CURRENT: $($cur.id) — $($cur.title)"
} else {
  Write-Host "CURRENT: (none)"
}

if ($next) {
  Write-Host "NEXT:    $($next.id) — $($next.title)"
} else {
  Write-Host "NEXT:    (none eligible)"
}

exit 0
