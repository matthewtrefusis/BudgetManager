# Package-BudgetManager.ps1
# Script to package BudgetManager for distribution

# Create a distribution folder
$distFolder = ".\BudgetManager-Distribution"
if (Test-Path $distFolder) {
    Remove-Item $distFolder -Recurse -Force
}
New-Item -ItemType Directory -Path $distFolder -Force | Out-Null

# Copy the executable
Copy-Item ".\bin\Release\net8.0-windows\win-x64\publish\BudgetManager.exe" -Destination $distFolder

# Create a README file
$readmeContent = @"
# BudgetManager

## Installation
1. Extract all files to a folder of your choice
2. Double-click BudgetManager.exe to run the application

## First Launch
- On first launch, you will need to register an account
- Your data will be stored in %APPDATA%\BudgetManager

## Security Features
- Strong password requirements
- Data encryption
- Account lockout after 5 failed login attempts
- Automatic session timeout after 30 minutes of inactivity

## Support
For issues or questions, please contact support@budgetmanager.example
"@
$readmeContent | Out-File -FilePath "$distFolder\README.txt" -Encoding utf8

# Create a zip file
$zipPath = ".\BudgetManager-1.0.0.zip"
if (Test-Path $zipPath) {
    Remove-Item $zipPath -Force
}

Add-Type -AssemblyName System.IO.Compression.FileSystem
[System.IO.Compression.ZipFile]::CreateFromDirectory($distFolder, $zipPath)

Write-Host "Package created successfully at $zipPath"
Write-Host "Distribution folder: $distFolder"
