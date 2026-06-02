# Build script for Sim Racing Companion installer
# Requirements: .NET 10 SDK, Inno Setup 6 installed at default location

param(
    [string]$Version = "3.0.0"
)

$ErrorActionPreference = "Stop"

$Root = Split-Path $PSScriptRoot -Parent
$PublishDir = Join-Path $Root "publish\win-x64"
$DistDir = Join-Path $Root "dist"
$InstallerScript = Join-Path $PSScriptRoot "setup.iss"

Write-Host "=== Sim Racing Companion Build ===" -ForegroundColor Cyan
Write-Host "Version: $Version"

# Step 1: Publish the app
Write-Host "`n[1/3] Publishing app..." -ForegroundColor Yellow
$AppProject = Join-Path $Root "src\CompanionAgent.App\CompanionAgent.App.csproj"
dotnet publish $AppProject `
    --configuration Release `
    --runtime win-x64 `
    --self-contained true `
    --output $PublishDir `
    /p:Version=$Version `
    /p:PublishSingleFile=true `
    /p:IncludeNativeLibrariesForSelfExtract=true
if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed" }

# Step 2: Create dist folder
if (-not (Test-Path $DistDir)) { New-Item -ItemType Directory $DistDir | Out-Null }

# Step 3: Compile installer
Write-Host "`n[2/3] Compiling installer..." -ForegroundColor Yellow
$InnoSetup = "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe"
if (-not (Test-Path $InnoSetup)) {
    $InnoSetup = "$env:ProgramFiles\Inno Setup 6\ISCC.exe"
}
if (-not (Test-Path $InnoSetup)) {
    Write-Warning "Inno Setup 6 not found. Please install it from https://jrsoftware.org/isinfo.php"
    Write-Warning "Installer script is ready at: $InstallerScript"
    exit 0
}

& $InnoSetup $InstallerScript /DAppVersion=$Version
if ($LASTEXITCODE -ne 0) { throw "Inno Setup compilation failed" }

Write-Host "`n[3/3] Done!" -ForegroundColor Green
Write-Host "Installer: $DistDir\SimRacingCompanion-Setup-$Version.exe"
