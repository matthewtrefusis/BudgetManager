# Budget Manager

A comprehensive personal finance management application built with C# and .NET 8.

## Features

### 💳 Transaction Management
- Add income and expense transactions
- Categorize transactions for better organization
- View and search transaction history
- Delete unwanted transactions
- Support for transaction notes

### 📝 Budget Management
- Create monthly budgets for different categories
- Real-time budget tracking and spending alerts
- Visual indicators for budget utilization
- Budget performance monitoring
- Over-budget warnings

### 🎯 Financial Goals
- Set and track financial savings goals
- Monitor goal progress with visual indicators
- Calculate required monthly savings
- Target date tracking with countdown
- Goal completion celebrations

### 📊 Comprehensive Reports
- Monthly financial summaries
- Budget performance reports
- Goal progress tracking
- Yearly financial analysis
- Custom transaction reports
- Category-wise expense breakdown

### 🏠 Dashboard
- Quick financial overview
- Budget alerts and warnings
- Upcoming goal deadlines
- Recent transaction summary
- Net income tracking

## Installation

1. Ensure you have .NET 8.0 SDK installed on your system
2. Clone or download this repository
3. Navigate to the project directory
4. Run the following commands:

```bash
dotnet restore
dotnet build
dotnet run
```

## Data Storage

The application stores all data in JSON files located in your user profile's AppData folder:
- `%APPDATA%\BudgetManager\transactions.json`
- `%APPDATA%\BudgetManager\budgets.json`
- `%APPDATA%\BudgetManager\goals.json`

## Usage

1. **Adding Transactions**: Use the transaction management menu to record your income and expenses
2. **Setting Budgets**: Create monthly spending limits for different categories
3. **Financial Goals**: Set savings targets with deadlines
4. **Monitoring**: Use the dashboard and reports to track your financial health

## Architecture

The application follows a clean architecture pattern with:

- **Models**: Core business entities (Transaction, Budget, FinancialGoal)
- **Services**: Business logic and data management
- **UI**: Console-based user interface
- **Data Persistence**: JSON file storage

## Dependencies

- **Newtonsoft.Json**: For JSON serialization/deserialization
- **ConsoleTables**: For formatted console table output

## Future Enhancements

- Export data to CSV/Excel
- Import transactions from bank files
- Multiple currency support
- Recurring transactions
- Investment tracking
- Web-based interface
- Mobile app companion

## Contributing

Feel free to submit issues, feature requests, or pull requests to improve the application.

## License

This project is open source and available under the MIT License.

## Building the Application

The application's executable is not included in the repository due to GitHub's file size limitations. You can build it yourself using the following steps:

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later

### Steps to Build a Self-Contained Executable

1. Clone the repository:
   ```
   git clone https://github.com/matthewtrefusis/BudgetManager.git
   cd BudgetManager
   ```

2. Build the self-contained executable:
   ```
   dotnet publish -r win-x64 -c Release --self-contained true /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true
   ```

3. The executable will be available at:
   ```
   bin\Release\net8.0-windows\win-x64\publish\BudgetManager.exe
   ```

4. Copy it to your desired location and run it directly.

### Alternative: Run from Source

If you prefer not to build the executable, you can run the application directly from source:

```
dotnet run
```
