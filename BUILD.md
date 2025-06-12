# Building BudgetManager

This document provides instructions for building the BudgetManager application.

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later
- Windows operating system (Windows 10 or newer recommended)

## Building a Self-Contained Executable

To build a standalone executable that doesn't require .NET to be installed on the target machine:

1. Open a terminal in the project root directory
2. Run the following command:

```powershell
dotnet publish -r win-x64 -c Release --self-contained true /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true
```

3. The executable will be available at:
```
bin\Release\net8.0-windows\win-x64\publish\BudgetManager.exe
```

## Building a Framework-Dependent Executable

If you prefer a smaller executable that requires .NET to be installed on the target machine:

```powershell
dotnet publish -r win-x64 -c Release --self-contained false
```

## Running Without Building

If you have the .NET SDK installed, you can run the application directly:

```powershell
dotnet run
```

## Distribution

When distributing the application:

1. Be aware that the self-contained executable is large (over 150MB) because it includes the .NET runtime
2. Consider using a framework-dependent deployment for smaller file sizes if your users already have .NET installed
3. The application stores user data in the `%APPDATA%\BudgetManager` folder

## Troubleshooting

If you encounter any build issues:

1. Make sure you have the latest .NET 8 SDK installed
2. Run `dotnet restore` to ensure all dependencies are properly resolved
3. Check the output logs for specific error messages
