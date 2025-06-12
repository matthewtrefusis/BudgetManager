## 🛠️ Development Guide

This guide explains the architecture and how to extend the Budget Manager application.

### Architecture Overview

The application follows a clean architecture pattern with clear separation of concerns:

```
BudgetManager/
├── Models/           # Domain entities
├── Services/         # Business logic and data access
├── UI/              # User interface layer
└── Program.cs       # Application entry point
```

### Core Components

#### Models Layer
- **Transaction.cs**: Represents financial transactions (income/expense)
- **Budget.cs**: Represents monthly spending budgets with tracking
- **FinancialGoal.cs**: Represents savings goals with progress tracking

#### Services Layer
- **IDataService/JsonDataService**: Handles data persistence using JSON files
- **IBudgetService/BudgetService**: Core business logic for financial operations
- **ReportService**: Generates various financial reports

#### UI Layer
- **ConsoleUI**: Console-based user interface with menu navigation

### Key Design Patterns

1. **Repository Pattern**: DataService abstracts data storage
2. **Service Layer Pattern**: Business logic separated from UI
3. **Dependency Injection**: Services injected via constructor
4. **Async/Await**: Non-blocking operations for file I/O

### Extending the Application

#### Adding New Transaction Types
1. Extend the `TransactionType` enum in `Transaction.cs`
2. Update UI logic in `ConsoleUI.cs` to handle new types
3. Modify reporting logic if needed

#### Adding New Report Types
1. Add new methods to `ReportService.cs`
2. Create corresponding UI menu items in `ViewReportsAsync()`
3. Use ConsoleTables for consistent formatting

#### Adding Data Validation
```csharp
public class TransactionValidator
{
    public static bool ValidateTransaction(Transaction transaction)
    {
        return !string.IsNullOrEmpty(transaction.Description) && 
               transaction.Amount > 0;
    }
}
```

#### Adding New Data Storage Options
1. Implement `IDataService` interface
2. Choose storage method (SQL, XML, Cloud API, etc.)
3. Update dependency injection in `Program.cs`

### Future Enhancement Ideas

#### Database Integration
Replace JSON files with a proper database:
```csharp
public class SqlDataService : IDataService
{
    private readonly string _connectionString;
    
    public SqlDataService(string connectionString)
    {
        _connectionString = connectionString;
    }
    
    // Implement interface methods using Entity Framework or ADO.NET
}
```

#### Web API Layer
Add a REST API for web/mobile clients:
```csharp
[ApiController]
[Route("api/[controller]")]
public class TransactionsController : ControllerBase
{
    private readonly IBudgetService _budgetService;
    
    [HttpGet]
    public async Task<ActionResult<List<Transaction>>> GetTransactions()
    {
        return await _budgetService.GetAllTransactionsAsync();
    }
    
    [HttpPost]
    public async Task<ActionResult> AddTransaction(Transaction transaction)
    {
        await _budgetService.AddTransactionAsync(transaction);
        return Ok();
    }
}
```

#### Rich Console UI
Enhance the console interface:
```csharp
// Using Spectre.Console for rich formatting
var table = new Table();
table.AddColumn("Date");
table.AddColumn("Description");
table.AddColumn(new TableColumn("Amount").RightAligned());

foreach (var transaction in transactions)
{
    table.AddRow(
        transaction.Date.ToString("MMM dd"),
        transaction.Description,
        $"${transaction.Amount:N2}"
    );
}

AnsiConsole.Write(table);
```

#### Configuration System
Add application settings:
```csharp
public class AppSettings
{
    public string DataDirectory { get; set; }
    public string DateFormat { get; set; }
    public string Currency { get; set; }
    public bool EnableNotifications { get; set; }
}
```

#### Import/Export Features
Add data import/export capabilities:
```csharp
public interface IImportExportService
{
    Task<List<Transaction>> ImportFromCsvAsync(string filePath);
    Task ExportToCsvAsync(List<Transaction> transactions, string filePath);
    Task<List<Transaction>> ImportFromBankFileAsync(string filePath);
}
```

#### Notification System
Add budget and goal notifications:
```csharp
public interface INotificationService
{
    Task SendBudgetAlertAsync(Budget budget);
    Task SendGoalReminderAsync(FinancialGoal goal);
    Task SendMonthlyReportAsync();
}
```

### Testing Strategy

#### Unit Tests Example
```csharp
[Test]
public void Budget_ShouldCalculateRemainingCorrectly()
{
    // Arrange
    var budget = new Budget("Test", "Food", 500);
    budget.CurrentSpent = 300;
    
    // Act
    var remaining = budget.RemainingBudget;
    
    // Assert
    Assert.AreEqual(200, remaining);
}
```

#### Integration Tests Example
```csharp
[Test]
public async Task BudgetService_ShouldUpdateBudgetSpending()
{
    // Arrange
    var dataService = new MockDataService();
    var budgetService = new BudgetService(dataService);
    await budgetService.InitializeAsync();
    
    // Act
    await budgetService.AddTransactionAsync(
        new Transaction("Groceries", 50, TransactionType.Expense, "Food"));
    
    // Assert
    var budgets = await budgetService.GetAllBudgetsAsync();
    var foodBudget = budgets.FirstOrDefault(b => b.Category == "Food");
    Assert.AreEqual(50, foodBudget?.CurrentSpent);
}
```

### Performance Considerations

1. **Lazy Loading**: Load data only when needed
2. **Caching**: Cache frequently accessed data
3. **Pagination**: For large transaction lists
4. **Indexing**: When using a database, index frequently queried fields

### Security Considerations

1. **Data Encryption**: Encrypt sensitive financial data
2. **Input Validation**: Validate all user inputs
3. **Access Control**: Implement user authentication if multi-user
4. **Audit Trail**: Log all financial operations

### Deployment Options

#### Console Application
- Package as a self-contained executable
- Use `dotnet publish -c Release --self-contained`

#### Windows Service
- Convert to run as a background service
- Use Microsoft.Extensions.Hosting.WindowsServices

#### Web Application
- Convert to ASP.NET Core with Blazor UI
- Deploy to Azure App Service or IIS

### Contributing Guidelines

1. Follow existing code patterns and naming conventions
2. Add unit tests for new features
3. Update documentation for new functionality
4. Use meaningful commit messages
5. Create feature branches for development

### Useful NuGet Packages

- **Entity Framework Core**: Database ORM
- **AutoMapper**: Object-to-object mapping
- **FluentValidation**: Input validation
- **Serilog**: Structured logging
- **Spectre.Console**: Rich console UI
- **CsvHelper**: CSV file processing
- **Newtonsoft.Json**: JSON serialization (already included)

This architecture provides a solid foundation for building a comprehensive personal finance management system while maintaining clean, testable, and extensible code.
