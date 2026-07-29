# ============================================================
# enable-dev.ps1 — Creates dev mode flag files
# Run: .\enable-dev.ps1                  (both flags)
#      .\enable-dev.ps1 -Login           (login only)
#      .\enable-dev.ps1 -Dashboard       (dashboard only)
# ============================================================
param(
    [switch]$Login,
    [switch]$Dashboard
)

$dir = "$env:ProgramData\AccuSync"
New-Item -ItemType Directory -Force -Path $dir | Out-Null

# If no specific flag requested, enable both
if (-not $Login -and -not $Dashboard) {
    $Login = $true
    $Dashboard = $true
}

if ($Login) {
    "DevUser|Admin" | Out-File "$dir\skip_login.flag" -Encoding UTF8
    Write-Host "[OK] skip_login.flag created (DevUser|Admin)" -ForegroundColor Green
}

if ($Dashboard) {
    "PatientInformation" | Out-File "$dir\skip_dashboard.flag" -Encoding UTF8
    Write-Host "[OK] skip_dashboard.flag created (target: PatientInformation)" -ForegroundColor Green
}

Write-Host ""
Write-Host "Flag directory: $dir" -ForegroundColor Cyan


# ============================================================
# disable-dev.ps1 — Removes dev mode flag files
# Run: .\disable-dev.ps1                 (both flags)
#      .\disable-dev.ps1 -Login          (login only)
#      .\disable-dev.ps1 -Dashboard      (dashboard only)
# ============================================================
# Save as separate file: disable-dev.ps1
<#
param(
    [switch]$Login,
    [switch]$Dashboard
)

$dir = "$env:ProgramData\AccuSync"

if (-not $Login -and -not $Dashboard) {
    $Login = $true
    $Dashboard = $true
}

if ($Login -and (Test-Path "$dir\skip_login.flag")) {
    Remove-Item "$dir\skip_login.flag"
    Write-Host "[OK] skip_login.flag removed" -ForegroundColor Yellow
}

if ($Dashboard -and (Test-Path "$dir\skip_dashboard.flag")) {
    Remove-Item "$dir\skip_dashboard.flag"
    Write-Host "[OK] skip_dashboard.flag removed" -ForegroundColor Yellow
}
#>


# ============================================================
# dev-status.ps1 — Shows current dev mode status
# ============================================================
# Save as separate file: dev-status.ps1
<#
$dir = "$env:ProgramData\AccuSync"

Write-Host "AccuSync Dev Mode Status" -ForegroundColor Cyan
Write-Host "========================" -ForegroundColor Cyan
Write-Host "Directory: $dir"
Write-Host ""

if (Test-Path "$dir\skip_login.flag") {
    $content = Get-Content "$dir\skip_login.flag" -Raw
    Write-Host "[ON]  skip_login.flag  → $($content.Trim())" -ForegroundColor Green
} else {
    Write-Host "[OFF] skip_login.flag" -ForegroundColor DarkGray
}

if (Test-Path "$dir\skip_dashboard.flag") {
    $content = Get-Content "$dir\skip_dashboard.flag" -Raw
    Write-Host "[ON]  skip_dashboard.flag → $($content.Trim())" -ForegroundColor Green
} else {
    Write-Host "[OFF] skip_dashboard.flag" -ForegroundColor DarkGray
}
#>
