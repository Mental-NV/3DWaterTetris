[CmdletBinding()]
param(
  [ValidateSet("Always","M0","M1","M2","M3","M4","M5")]
  [string]$Scope = "Always",

  [ValidateSet("Debug","Release")]
  [string]$Configuration = "Release",

  [switch]$LockedRestore = $true,
  [switch]$UseLockFile = $false,
  [switch]$IncludeFormat = $false,

  # future switches (safe to add later without breaking callers)
  [switch]$Golden = $false,
  [switch]$Replay = $false,
  [switch]$ValidateLevels = $false,
  [switch]$Unity = $false
)

$ErrorActionPreference = "Stop"

function Run([string]$cmd) {
  Write-Host ">> $cmd"
  iex $cmd
  if ($LASTEXITCODE -ne 0) { throw "Command failed with exit code ${LASTEXITCODE}: $cmd" }
}

# Find solution (first *.sln under repo root)
$sln = Get-ChildItem -Path "." -Filter "*.sln" -Recurse -ErrorAction SilentlyContinue | Select-Object -First 1
if (-not $sln) { throw "No .sln found. Create solution/projects before running CI script." }

# Restore
# - UseLockFile: generates/updates packages.lock.json (for initial adoption)
# - LockedRestore: verifies restore uses committed lock files
if ($UseLockFile) {
  Run "dotnet restore `"$($sln.FullName)`" --use-lock-file"
}

if ($LockedRestore) {
  Run "dotnet restore `"$($sln.FullName)`" --locked-mode"
} elseif (-not $UseLockFile) {
  Run "dotnet restore `"$($sln.FullName)`""
}

# Build + test
Run "dotnet build `"$($sln.FullName)`" -c $Configuration"
Run "dotnet test  `"$($sln.FullName)`" -c $Configuration --no-build"

# Formatting (only when gate is introduced)
if ($IncludeFormat) {
  Run "dotnet format `"$($sln.FullName)`" --verify-no-changes"
}

# Future: golden / replay / level validation / unity
# Implement these later when the projects/tools exist; keep switches stable so backlog items don't churn.

Write-Host "CI OK: Scope=$Scope Config=$Configuration LockedRestore=$LockedRestore IncludeFormat=$IncludeFormat"
exit 0
