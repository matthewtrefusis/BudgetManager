# Package-BudgetManager.ps1
# Script to package BudgetManager for distribution

Write-Host "Starting BudgetManager packaging process..." -ForegroundColor Green

# Create a distribution folder
$distFolder = ".\BudgetManager-Distribution"
if (Test-Path $distFolder) {
    Write-Host "Cleaning existing distribution folder..." -ForegroundColor Yellow
    Remove-Item $distFolder -Recurse -Force
}
New-Item -ItemType Directory -Path $distFolder -Force | Out-Null
Write-Host "Created distribution folder: $distFolder" -ForegroundColor Cyan

# Build the self-contained executable
Write-Host "Building self-contained executable..." -ForegroundColor Cyan
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishReadyToRun=true -o $distFolder
if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed with exit code $LASTEXITCODE" -ForegroundColor Red
    exit $LASTEXITCODE
}
Write-Host "Build completed successfully!" -ForegroundColor Green

# Create a BUILD.md file with version and build info
$buildContent = @"
# BudgetManager Build Information

## Build Date
$(Get-Date -Format "MMMM dd, yyyy")

## Version
1.0.0

## Build Configuration
- Target Framework: .NET 8.0 Windows
- Platform: x64
- Build Type: Self-contained (includes runtime)
- Configuration: Release

## Dependencies
- BCrypt.Net-Next 4.0.3
- Newtonsoft.Json 13.0.3
- Microsoft.VisualBasic 10.3.0

## Notes
This is a self-contained application that does not require .NET to be installed on the target machine.
"@
$buildContent | Out-File -FilePath "$distFolder\BUILD.md" -Encoding utf8
Write-Host "Created BUILD.md file" -ForegroundColor Cyan

# Create a README file with instructions
$readmeContent = @"
# BudgetManager

## Installation
1. Extract all files to a folder of your choice
2. Double-click BudgetManager.exe to run the application
3. No additional installation required - this is a self-contained executable

## First Launch
- On first launch, you will need to register an account
- Your data will be stored in %APPDATA%\BudgetManager

## Features
- Track income and expenses
- Set and monitor budget goals
- Generate financial reports
- Dashboard with financial overview
- Secure data storage

## Security Features
- Strong password requirements
- Data encryption
- Account lockout after 5 failed login attempts
- Automatic session timeout after 30 minutes of inactivity

## System Requirements
- Windows 10 or higher
- 100MB of free disk space
- No internet connection required for basic functionality

## Support
For issues or questions, please contact support@budgetmanager.example
"@
$readmeContent | Out-File -FilePath "$distFolder\README.txt" -Encoding utf8
Write-Host "Created README.txt file" -ForegroundColor Cyan

# Create a batch file for easy launching
$batchContent = @"
@echo off
echo Starting BudgetManager...
start BudgetManager.exe
"@
$batchContent | Out-File -FilePath "$distFolder\RunBudgetManager.bat" -Encoding ascii
Write-Host "Created RunBudgetManager.bat file" -ForegroundColor Cyan

# Create a zip file for distribution
$zipPath = ".\BudgetManager-$((Get-Date).ToString('yyyyMMdd')).zip"
if (Test-Path $zipPath) {
    Remove-Item $zipPath -Force
}

Write-Host "Creating distribution zip file: $zipPath" -ForegroundColor Cyan
Add-Type -AssemblyName System.IO.Compression.FileSystem
[System.IO.Compression.ZipFile]::CreateFromDirectory($distFolder, $zipPath)

Write-Host "`nPackage created successfully!" -ForegroundColor Green
Write-Host "Distribution folder: $distFolder"
Write-Host "Distribution zip: $zipPath"
