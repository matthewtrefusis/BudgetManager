# Package-BudgetManager.ps1
# Script to package BudgetManager for distribution

# Create a distribution folder
$distFolder = ".\BudgetManager-Distribution"
if (Test-Path $distFolder) {
    Remove-Item $distFolder -Recurse -Force
}
New-Item -ItemType Directory -Path $distFolder -Force | Out-Null

# Create a BUILD.md file with instructions
$buildContent = @"
# Building BudgetManager

Due to GitHub's file size limitations, we do not include the executable in the repository.
Follow these steps to build the application yourself:

## Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later

## Steps to Build

1. Open a terminal in this directory
2. Run the following command:
   ```
   dotnet publish -r win-x64 -c Release --self-contained true /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true
   ```
3. The executable will be available in the 'bin\Release\net8.0-windows\win-x64\publish' folder

## Running Without Building
If you have the .NET SDK installed, you can also run directly from source:
```
dotnet run
```
"@
$buildContent | Out-File -FilePath "$distFolder\BUILD.md" -Encoding utf8

# Create a README file
$readmeContent = @"
# BudgetManager

## Installation
1. Follow the instructions in BUILD.md to create the executable
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
$zipPath = ".\BudgetManager-Distribution.zip"
if (Test-Path $zipPath) {
    Remove-Item $zipPath -Force
}

Add-Type -AssemblyName System.IO.Compression.FileSystem
[System.IO.Compression.ZipFile]::CreateFromDirectory($distFolder, $zipPath)

Write-Host "Package created successfully at $zipPath"
Write-Host "Distribution folder: $distFolder"
