using BudgetManager.Models;
using BudgetManager.Services;

namespace BudgetManager.UI
{
    public class ConsoleUI
    {
        private readonly IBudgetService _budgetService;
        private readonly ReportService _reportService;

        public ConsoleUI(IBudgetService budgetService, ReportService reportService)
        {
            _budgetService = budgetService;
            _reportService = reportService;
        }

        public async Task RunAsync()
        {
            Console.Clear();
            ShowWelcomeMessage();

            while (true)
            {
                try
                {
                    ShowMainMenu();
                    var choice = Console.ReadLine()?.Trim();

                    switch (choice)
                    {
                        case "1":
                            await ManageTransactionsAsync();
                            break;
                        case "2":
                            await ManageBudgetsAsync();
                            break;
                        case "3":
                            await ManageGoalsAsync();
                            break;
                        case "4":
                            await ViewReportsAsync();
                            break;
                        case "5":
                            await ShowDashboardAsync();
                            break;
                        case "6":
                            Console.WriteLine("\nThank you for using Budget Manager! 👋");
                            return;
                        default:
                            Console.WriteLine("\n❌ Invalid option. Please try again.");
                            break;
                    }

                    if (choice != "6")
                    {
                        Console.WriteLine("\nPress any key to continue...");
                        Console.ReadKey();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\n❌ An error occurred: {ex.Message}");
                    Console.WriteLine("Press any key to continue...");
                    Console.ReadKey();
                }
            }
        }

        private void ShowWelcomeMessage()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔════════════════════════════════════════╗");
            Console.WriteLine("║          💰 BUDGET MANAGER 💰          ║");
            Console.WriteLine("║     Your Personal Finance Assistant    ║");
            Console.WriteLine("╚════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();
        }

        private void ShowMainMenu()
        {
            Console.Clear();
            ShowWelcomeMessage();
            
            Console.WriteLine("📋 Main Menu:");
            Console.WriteLine("1. 💳 Manage Transactions");
            Console.WriteLine("2. 📝 Manage Budgets");
            Console.WriteLine("3. 🎯 Manage Financial Goals");
            Console.WriteLine("4. 📊 View Reports");
            Console.WriteLine("5. 🏠 Dashboard");
            Console.WriteLine("6. 🚪 Exit");
            Console.Write("\nSelect an option (1-6): ");
        }

        private async Task ManageTransactionsAsync()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("💳 Transaction Management\n");
                Console.WriteLine("1. Add Transaction");
                Console.WriteLine("2. View Recent Transactions");
                Console.WriteLine("3. Search Transactions");
                Console.WriteLine("4. Delete Transaction");
                Console.WriteLine("5. Back to Main Menu");
                Console.Write("\nSelect an option (1-5): ");

                var choice = Console.ReadLine()?.Trim();

                switch (choice)
                {
                    case "1":
                        await AddTransactionAsync();
                        break;
                    case "2":
                        await ViewRecentTransactionsAsync();
                        break;
                    case "3":
                        await SearchTransactionsAsync();
                        break;
                    case "4":
                        await DeleteTransactionAsync();
                        break;
                    case "5":
                        return;
                    default:
                        Console.WriteLine("❌ Invalid option.");
                        break;
                }

                if (choice != "5")
                {
                    Console.WriteLine("\nPress any key to continue...");
                    Console.ReadKey();
                }
            }
        }

        private async Task AddTransactionAsync()
        {
            Console.Clear();
            Console.WriteLine("➕ Add New Transaction\n");

            Console.Write("Description: ");
            var description = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(description))
            {
                Console.WriteLine("❌ Description cannot be empty.");
                return;
            }

            Console.Write("Amount: $");
            if (!decimal.TryParse(Console.ReadLine(), out var amount) || amount <= 0)
            {
                Console.WriteLine("❌ Please enter a valid positive amount.");
                return;
            }

            Console.WriteLine("Type: ");
            Console.WriteLine("1. Income");
            Console.WriteLine("2. Expense");
            Console.Write("Select (1-2): ");
            
            var typeChoice = Console.ReadLine()?.Trim();
            TransactionType type;
            
            if (typeChoice == "1")
                type = TransactionType.Income;
            else if (typeChoice == "2")
                type = TransactionType.Expense;
            else
            {
                Console.WriteLine("❌ Invalid transaction type.");
                return;
            }

            Console.Write("Category: ");
            var category = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(category))
            {
                Console.WriteLine("❌ Category cannot be empty.");
                return;
            }

            Console.Write("Notes (optional): ");
            var notes = Console.ReadLine()?.Trim();

            var transaction = new Transaction(description, amount, type, category, string.IsNullOrEmpty(notes) ? null : notes);
            await _budgetService.AddTransactionAsync(transaction);

            Console.WriteLine($"\n✅ Transaction added successfully!");
            Console.WriteLine($"   {type}: ${amount:N2} - {description} ({category})");
        }

        private async Task ViewRecentTransactionsAsync()
        {
            Console.Clear();
            Console.WriteLine("📋 Recent Transactions\n");

            var transactions = await _budgetService.GetAllTransactionsAsync();
            
            if (!transactions.Any())
            {
                Console.WriteLine("No transactions found.");
                return;
            }

            Console.WriteLine($"{"Date",-12} {"Type",-8} {"Category",-15} {"Description",-25} {"Amount",10}");
            Console.WriteLine(new string('-', 80));

            foreach (var transaction in transactions.Take(20))
            {
                var typeColor = transaction.Type == TransactionType.Income ? ConsoleColor.Green : ConsoleColor.Red;
                var typeSymbol = transaction.Type == TransactionType.Income ? "+" : "-";
                
                Console.Write($"{transaction.Date:MMM dd, yyyy} ");
                
                Console.ForegroundColor = typeColor;
                Console.Write($"{transaction.Type,-8}");
                Console.ResetColor();
                
                Console.Write($" {transaction.Category,-15} {transaction.Description,-25} ");
                
                Console.ForegroundColor = typeColor;
                Console.WriteLine($"{typeSymbol}${transaction.Amount,9:N2}");
                Console.ResetColor();
            }

            if (transactions.Count > 20)
            {
                Console.WriteLine($"\n... and {transactions.Count - 20} more transactions.");
            }
        }

        private async Task SearchTransactionsAsync()
        {
            Console.Clear();
            Console.WriteLine("🔍 Search Transactions\n");
            Console.WriteLine("1. Search by Category");
            Console.WriteLine("2. Search by Date Range");
            Console.WriteLine("3. Back");
            Console.Write("\nSelect option (1-3): ");

            var choice = Console.ReadLine()?.Trim();

            switch (choice)
            {
                case "1":
                    Console.Write("Enter category: ");
                    var category = Console.ReadLine()?.Trim();
                    if (!string.IsNullOrEmpty(category))
                    {
                        var transactions = await _budgetService.GetTransactionsByCategoryAsync(category);
                        DisplayTransactionResults(transactions, $"Transactions in category '{category}'");
                    }
                    break;
                case "2":
                    await SearchByDateRangeAsync();
                    break;
                case "3":
                    return;
            }
        }

        private async Task SearchByDateRangeAsync()
        {
            Console.Write("Start date (MM/dd/yyyy): ");
            if (!DateTime.TryParse(Console.ReadLine(), out var startDate))
            {
                Console.WriteLine("❌ Invalid start date format.");
                return;
            }

            Console.Write("End date (MM/dd/yyyy): ");
            if (!DateTime.TryParse(Console.ReadLine(), out var endDate))
            {
                Console.WriteLine("❌ Invalid end date format.");
                return;
            }

            var transactions = await _budgetService.GetTransactionsByDateRangeAsync(startDate, endDate);
            DisplayTransactionResults(transactions, $"Transactions from {startDate:MMM dd} to {endDate:MMM dd, yyyy}");
        }

        private void DisplayTransactionResults(List<Transaction> transactions, string title)
        {
            Console.Clear();
            Console.WriteLine($"📋 {title}\n");

            if (!transactions.Any())
            {
                Console.WriteLine("No transactions found.");
                return;
            }

            Console.WriteLine($"{"Date",-12} {"Type",-8} {"Category",-15} {"Description",-25} {"Amount",10}");
            Console.WriteLine(new string('-', 80));

            foreach (var transaction in transactions)
            {
                var typeColor = transaction.Type == TransactionType.Income ? ConsoleColor.Green : ConsoleColor.Red;
                var typeSymbol = transaction.Type == TransactionType.Income ? "+" : "-";
                
                Console.Write($"{transaction.Date:MMM dd, yyyy} ");
                
                Console.ForegroundColor = typeColor;
                Console.Write($"{transaction.Type,-8}");
                Console.ResetColor();
                
                Console.Write($" {transaction.Category,-15} {transaction.Description,-25} ");
                
                Console.ForegroundColor = typeColor;
                Console.WriteLine($"{typeSymbol}${transaction.Amount,9:N2}");
                Console.ResetColor();
            }

            var totalIncome = transactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
            var totalExpenses = transactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);

            Console.WriteLine(new string('-', 80));
            Console.WriteLine($"Total Income: ${totalIncome:N2}  |  Total Expenses: ${totalExpenses:N2}  |  Net: ${totalIncome - totalExpenses:N2}");
        }

        private async Task DeleteTransactionAsync()
        {
            Console.Clear();
            Console.WriteLine("🗑️ Delete Transaction\n");

            var transactions = await _budgetService.GetAllTransactionsAsync();
            
            if (!transactions.Any())
            {
                Console.WriteLine("No transactions found.");
                return;
            }

            Console.WriteLine("Recent transactions:");
            for (int i = 0; i < Math.Min(10, transactions.Count); i++)
            {
                var t = transactions[i];
                Console.WriteLine($"{i + 1}. {t.Date:MMM dd} - {t.Description} (${t.Amount:N2})");
            }

            Console.Write("\nEnter transaction number to delete (1-10), or 0 to cancel: ");
            if (int.TryParse(Console.ReadLine(), out var index) && index > 0 && index <= Math.Min(10, transactions.Count))
            {
                var transaction = transactions[index - 1];
                Console.Write($"Are you sure you want to delete '{transaction.Description}' (${transaction.Amount:N2})? (y/N): ");
                
                if (Console.ReadLine()?.ToLower() == "y")
                {
                    await _budgetService.DeleteTransactionAsync(transaction.Id);
                    Console.WriteLine("✅ Transaction deleted successfully!");
                }
                else
                {
                    Console.WriteLine("❌ Deletion cancelled.");
                }
            }
            else if (index != 0)
            {
                Console.WriteLine("❌ Invalid selection.");
            }
        }

        private async Task ManageBudgetsAsync()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("📝 Budget Management\n");
                Console.WriteLine("1. View Current Budgets");
                Console.WriteLine("2. Add New Budget");
                Console.WriteLine("3. Update Budget");
                Console.WriteLine("4. Delete Budget");
                Console.WriteLine("5. Back to Main Menu");
                Console.Write("\nSelect an option (1-5): ");

                var choice = Console.ReadLine()?.Trim();

                switch (choice)
                {
                    case "1":
                        await ViewBudgetsAsync();
                        break;
                    case "2":
                        await AddBudgetAsync();
                        break;
                    case "3":
                        await UpdateBudgetAsync();
                        break;
                    case "4":
                        await DeleteBudgetAsync();
                        break;
                    case "5":
                        return;
                    default:
                        Console.WriteLine("❌ Invalid option.");
                        break;
                }

                if (choice != "5")
                {
                    Console.WriteLine("\nPress any key to continue...");
                    Console.ReadKey();
                }
            }
        }

        private async Task ViewBudgetsAsync()
        {
            Console.Clear();
            Console.WriteLine("📝 Current Budgets\n");

            var budgets = await _budgetService.GetAllBudgetsAsync();
            
            if (!budgets.Any())
            {
                Console.WriteLine("No budgets configured.");
                return;
            }

            Console.WriteLine($"{"Category",-20} {"Budget",-12} {"Spent",-12} {"Remaining",-12} {"Usage %",-10} {"Status",-10}");
            Console.WriteLine(new string('-', 90));

            foreach (var budget in budgets)
            {
                var usageColor = budget.IsOverBudget ? ConsoleColor.Red : 
                               budget.BudgetUtilizationPercentage > 80 ? ConsoleColor.Yellow : 
                               ConsoleColor.Green;

                var status = budget.IsOverBudget ? "⚠ Over" : 
                           budget.BudgetUtilizationPercentage > 80 ? "⚡ High" : 
                           "✓ Good";

                Console.Write($"{budget.Category,-20} ${budget.MonthlyLimit,-11:N2} ${budget.CurrentSpent,-11:N2} ");
                
                Console.ForegroundColor = usageColor;
                Console.Write($"${budget.RemainingBudget,-11:N2} {budget.BudgetUtilizationPercentage,-9:F1}% {status,-10}");
                Console.ResetColor();
                Console.WriteLine();
            }
        }

        private async Task AddBudgetAsync()
        {
            Console.Clear();
            Console.WriteLine("➕ Add New Budget\n");

            Console.Write("Budget Name: ");
            var name = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(name))
            {
                Console.WriteLine("❌ Budget name cannot be empty.");
                return;
            }

            Console.Write("Category: ");
            var category = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(category))
            {
                Console.WriteLine("❌ Category cannot be empty.");
                return;
            }

            Console.Write("Monthly Limit: $");
            if (!decimal.TryParse(Console.ReadLine(), out var limit) || limit <= 0)
            {
                Console.WriteLine("❌ Please enter a valid positive amount.");
                return;
            }

            var budget = new Budget(name, category, limit);
            await _budgetService.AddBudgetAsync(budget);

            Console.WriteLine($"\n✅ Budget '{name}' created successfully!");
            Console.WriteLine($"   Category: {category}");
            Console.WriteLine($"   Monthly Limit: ${limit:N2}");
        }

        private async Task UpdateBudgetAsync()
        {
            Console.Clear();
            Console.WriteLine("✏️ Update Budget\n");

            var budgets = await _budgetService.GetAllBudgetsAsync();
            
            if (!budgets.Any())
            {
                Console.WriteLine("No budgets found.");
                return;
            }

            Console.WriteLine("Select a budget to update:");
            for (int i = 0; i < budgets.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {budgets[i].Name} - {budgets[i].Category} (${budgets[i].MonthlyLimit:N2})");
            }

            Console.Write($"\nEnter budget number (1-{budgets.Count}): ");
            if (int.TryParse(Console.ReadLine(), out var index) && index > 0 && index <= budgets.Count)
            {
                var budget = budgets[index - 1];
                
                Console.WriteLine($"\nUpdating budget: {budget.Name}");
                Console.Write($"New monthly limit (current: ${budget.MonthlyLimit:N2}): $");
                
                if (decimal.TryParse(Console.ReadLine(), out var newLimit) && newLimit > 0)
                {
                    budget.MonthlyLimit = newLimit;
                    await _budgetService.UpdateBudgetAsync(budget);
                    Console.WriteLine("✅ Budget updated successfully!");
                }
                else
                {
                    Console.WriteLine("❌ Invalid amount entered.");
                }
            }
            else
            {
                Console.WriteLine("❌ Invalid selection.");
            }
        }

        private async Task DeleteBudgetAsync()
        {
            Console.Clear();
            Console.WriteLine("🗑️ Delete Budget\n");

            var budgets = await _budgetService.GetAllBudgetsAsync();
            
            if (!budgets.Any())
            {
                Console.WriteLine("No budgets found.");
                return;
            }

            Console.WriteLine("Select a budget to delete:");
            for (int i = 0; i < budgets.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {budgets[i].Name} - {budgets[i].Category} (${budgets[i].MonthlyLimit:N2})");
            }

            Console.Write($"\nEnter budget number (1-{budgets.Count}): ");
            if (int.TryParse(Console.ReadLine(), out var index) && index > 0 && index <= budgets.Count)
            {
                var budget = budgets[index - 1];
                Console.Write($"Are you sure you want to delete budget '{budget.Name}'? (y/N): ");
                
                if (Console.ReadLine()?.ToLower() == "y")
                {
                    await _budgetService.DeleteBudgetAsync(budget.Id);
                    Console.WriteLine("✅ Budget deleted successfully!");
                }
                else
                {
                    Console.WriteLine("❌ Deletion cancelled.");
                }
            }
            else
            {
                Console.WriteLine("❌ Invalid selection.");
            }
        }

        private async Task ManageGoalsAsync()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("🎯 Financial Goals Management\n");
                Console.WriteLine("1. View Current Goals");
                Console.WriteLine("2. Add New Goal");
                Console.WriteLine("3. Update Goal Progress");
                Console.WriteLine("4. Delete Goal");
                Console.WriteLine("5. Back to Main Menu");
                Console.Write("\nSelect an option (1-5): ");

                var choice = Console.ReadLine()?.Trim();

                switch (choice)
                {
                    case "1":
                        await ViewGoalsAsync();
                        break;
                    case "2":
                        await AddGoalAsync();
                        break;
                    case "3":
                        await UpdateGoalProgressAsync();
                        break;
                    case "4":
                        await DeleteGoalAsync();
                        break;
                    case "5":
                        return;
                    default:
                        Console.WriteLine("❌ Invalid option.");
                        break;
                }

                if (choice != "5")
                {
                    Console.WriteLine("\nPress any key to continue...");
                    Console.ReadKey();
                }
            }
        }

        private async Task ViewGoalsAsync()
        {
            Console.Clear();
            Console.WriteLine("🎯 Current Financial Goals\n");

            var goals = await _budgetService.GetAllGoalsAsync();
            
            if (!goals.Any())
            {
                Console.WriteLine("No financial goals set.");
                return;
            }

            foreach (var goal in goals)
            {
                Console.WriteLine($"📌 {goal.Name}");
                Console.WriteLine($"   Description: {goal.Description}");
                Console.WriteLine($"   Target: ${goal.TargetAmount:N2}");
                Console.WriteLine($"   Current: ${goal.CurrentAmount:N2}");
                Console.WriteLine($"   Progress: {goal.ProgressPercentage:F1}%");
                Console.WriteLine($"   Target Date: {goal.TargetDate:MMM dd, yyyy}");
                Console.WriteLine($"   Days Remaining: {goal.DaysRemaining}");
                Console.WriteLine($"   Required Monthly Savings: ${goal.RequiredMonthlySavings:N2}");
                
                // Progress bar
                var progressBar = CreateProgressBar(goal.ProgressPercentage);
                Console.WriteLine($"   {progressBar}");
                Console.WriteLine();
            }
        }

        private string CreateProgressBar(decimal percentage)
        {
            const int barLength = 30;
            var filledLength = (int)((percentage / 100) * barLength);
            var bar = new string('█', filledLength) + new string('░', barLength - filledLength);
            return $"[{bar}] {percentage:F1}%";
        }

        private async Task AddGoalAsync()
        {
            Console.Clear();
            Console.WriteLine("➕ Add New Financial Goal\n");

            Console.Write("Goal Name: ");
            var name = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(name))
            {
                Console.WriteLine("❌ Goal name cannot be empty.");
                return;
            }

            Console.Write("Description: ");
            var description = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(description))
            {
                Console.WriteLine("❌ Description cannot be empty.");
                return;
            }

            Console.Write("Target Amount: $");
            if (!decimal.TryParse(Console.ReadLine(), out var targetAmount) || targetAmount <= 0)
            {
                Console.WriteLine("❌ Please enter a valid positive amount.");
                return;
            }

            Console.Write("Target Date (MM/dd/yyyy): ");
            if (!DateTime.TryParse(Console.ReadLine(), out var targetDate) || targetDate <= DateTime.Now.Date)
            {
                Console.WriteLine("❌ Please enter a valid future date.");
                return;
            }

            var goal = new FinancialGoal(name, description, targetAmount, targetDate);
            await _budgetService.AddGoalAsync(goal);

            Console.WriteLine($"\n✅ Financial goal '{name}' created successfully!");
            Console.WriteLine($"   Target: ${targetAmount:N2} by {targetDate:MMM dd, yyyy}");
            Console.WriteLine($"   Required monthly savings: ${goal.RequiredMonthlySavings:N2}");
        }

        private async Task UpdateGoalProgressAsync()
        {
            Console.Clear();
            Console.WriteLine("📈 Update Goal Progress\n");

            var goals = await _budgetService.GetAllGoalsAsync();
            
            if (!goals.Any())
            {
                Console.WriteLine("No goals found.");
                return;
            }

            Console.WriteLine("Select a goal to update:");
            for (int i = 0; i < goals.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {goals[i].Name} - ${goals[i].CurrentAmount:N2}/${goals[i].TargetAmount:N2}");
            }

            Console.Write($"\nEnter goal number (1-{goals.Count}): ");
            if (int.TryParse(Console.ReadLine(), out var index) && index > 0 && index <= goals.Count)
            {
                var goal = goals[index - 1];
                
                Console.WriteLine($"\nUpdating progress for: {goal.Name}");
                Console.WriteLine($"Current amount: ${goal.CurrentAmount:N2}");
                Console.Write("Amount to add: $");
                
                if (decimal.TryParse(Console.ReadLine(), out var amount) && amount > 0)
                {
                    await _budgetService.UpdateGoalProgressAsync(goal.Id, amount);
                    
                    var updatedGoal = (await _budgetService.GetAllGoalsAsync()).FirstOrDefault(g => g.Id == goal.Id);
                    if (updatedGoal != null)
                    {
                        Console.WriteLine("✅ Goal progress updated successfully!");
                        Console.WriteLine($"   New total: ${updatedGoal.CurrentAmount:N2}");
                        Console.WriteLine($"   Progress: {updatedGoal.ProgressPercentage:F1}%");
                        
                        if (updatedGoal.IsCompleted)
                        {
                            Console.WriteLine("🎉 Congratulations! You've reached your goal!");
                        }
                    }
                }
                else
                {
                    Console.WriteLine("❌ Invalid amount entered.");
                }
            }
            else
            {
                Console.WriteLine("❌ Invalid selection.");
            }
        }

        private async Task DeleteGoalAsync()
        {
            Console.Clear();
            Console.WriteLine("🗑️ Delete Financial Goal\n");

            var goals = await _budgetService.GetAllGoalsAsync();
            
            if (!goals.Any())
            {
                Console.WriteLine("No goals found.");
                return;
            }

            Console.WriteLine("Select a goal to delete:");
            for (int i = 0; i < goals.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {goals[i].Name} - ${goals[i].CurrentAmount:N2}/${goals[i].TargetAmount:N2}");
            }

            Console.Write($"\nEnter goal number (1-{goals.Count}): ");
            if (int.TryParse(Console.ReadLine(), out var index) && index > 0 && index <= goals.Count)
            {
                var goal = goals[index - 1];
                Console.Write($"Are you sure you want to delete goal '{goal.Name}'? (y/N): ");
                
                if (Console.ReadLine()?.ToLower() == "y")
                {
                    await _budgetService.DeleteGoalAsync(goal.Id);
                    Console.WriteLine("✅ Goal deleted successfully!");
                }
                else
                {
                    Console.WriteLine("❌ Deletion cancelled.");
                }
            }
            else
            {
                Console.WriteLine("❌ Invalid selection.");
            }
        }

        private async Task ViewReportsAsync()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("📊 Reports\n");
                Console.WriteLine("1. Monthly Report");
                Console.WriteLine("2. Budget Performance");
                Console.WriteLine("3. Goal Progress");
                Console.WriteLine("4. Yearly Report");
                Console.WriteLine("5. Transaction Report");
                Console.WriteLine("6. Back to Main Menu");
                Console.Write("\nSelect an option (1-6): ");

                var choice = Console.ReadLine()?.Trim();

                switch (choice)
                {
                    case "1":
                        await _reportService.GenerateMonthlyReportAsync();
                        break;
                    case "2":
                        await _reportService.GenerateBudgetPerformanceReportAsync();
                        break;
                    case "3":
                        await _reportService.GenerateGoalProgressReportAsync();
                        break;
                    case "4":
                        await _reportService.GenerateYearlyReportAsync();
                        break;
                    case "5":
                        await GenerateCustomTransactionReportAsync();
                        break;
                    case "6":
                        return;
                    default:
                        Console.WriteLine("❌ Invalid option.");
                        break;
                }

                if (choice != "6")
                {
                    Console.WriteLine("\nPress any key to continue...");
                    Console.ReadKey();
                }
            }
        }

        private async Task GenerateCustomTransactionReportAsync()
        {
            Console.Clear();
            Console.WriteLine("📋 Transaction Report\n");
            Console.WriteLine("1. Last 30 days");
            Console.WriteLine("2. This month");
            Console.WriteLine("3. Custom date range");
            Console.Write("\nSelect option (1-3): ");

            var choice = Console.ReadLine()?.Trim();
            DateTime? startDate = null;
            DateTime? endDate = null;

            switch (choice)
            {
                case "1":
                    startDate = DateTime.Now.AddDays(-30);
                    endDate = DateTime.Now;
                    break;
                case "2":
                    startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                    endDate = DateTime.Now;
                    break;
                case "3":
                    Console.Write("Start date (MM/dd/yyyy): ");
                    if (!DateTime.TryParse(Console.ReadLine(), out var start))
                    {
                        Console.WriteLine("❌ Invalid start date format.");
                        return;
                    }
                    Console.Write("End date (MM/dd/yyyy): ");
                    if (!DateTime.TryParse(Console.ReadLine(), out var end))
                    {
                        Console.WriteLine("❌ Invalid end date format.");
                        return;
                    }
                    startDate = start;
                    endDate = end;
                    break;
                default:
                    Console.WriteLine("❌ Invalid option.");
                    return;
            }

            await _reportService.GenerateTransactionReportAsync(startDate, endDate);
        }

        private async Task ShowDashboardAsync()
        {
            Console.Clear();
            Console.WriteLine("🏠 Dashboard\n");

            // Quick financial summary
            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;
            var startDate = new DateTime(currentYear, currentMonth, 1);
            var endDate = DateTime.Now;

            var income = await _budgetService.GetTotalIncomeAsync(startDate, endDate);
            var expenses = await _budgetService.GetTotalExpensesAsync(startDate, endDate);
            var netIncome = income - expenses;

            Console.WriteLine($"💰 This Month's Summary ({DateTime.Now:MMMM yyyy}):");
            Console.WriteLine($"   Income:     ${income:N2}");
            Console.WriteLine($"   Expenses:   ${expenses:N2}");
            Console.WriteLine($"   Net Income: ${netIncome:N2}");

            if (netIncome >= 0)
                Console.ForegroundColor = ConsoleColor.Green;
            else
                Console.ForegroundColor = ConsoleColor.Red;
            
            Console.WriteLine($"   Status:     {(netIncome >= 0 ? "✓ Positive" : "⚠ Negative")}");
            Console.ResetColor();

            // Budget alerts
            var budgets = await _budgetService.GetAllBudgetsAsync();
            var alertBudgets = budgets.Where(b => b.BudgetUtilizationPercentage > 80).ToList();
            
            if (alertBudgets.Any())
            {
                Console.WriteLine($"\n⚠️ Budget Alerts:");
                foreach (var budget in alertBudgets)
                {
                    var status = budget.IsOverBudget ? "OVER BUDGET" : "HIGH USAGE";
                    Console.ForegroundColor = budget.IsOverBudget ? ConsoleColor.Red : ConsoleColor.Yellow;
                    Console.WriteLine($"   {budget.Category}: {status} ({budget.BudgetUtilizationPercentage:F1}%)");
                    Console.ResetColor();
                }
            }

            // Upcoming goal deadlines
            var goals = await _budgetService.GetAllGoalsAsync();
            var urgentGoals = goals.Where(g => g.DaysRemaining <= 30 && g.DaysRemaining > 0).ToList();
            
            if (urgentGoals.Any())
            {
                Console.WriteLine($"\n⏰ Upcoming Goal Deadlines:");
                foreach (var goal in urgentGoals.OrderBy(g => g.DaysRemaining))
                {
                    Console.WriteLine($"   {goal.Name}: {goal.DaysRemaining} days left ({goal.ProgressPercentage:F1}% complete)");
                }
            }

            // Recent transactions
            var recentTransactions = await _budgetService.GetAllTransactionsAsync();
            if (recentTransactions.Any())
            {
                Console.WriteLine($"\n📋 Recent Transactions:");
                foreach (var transaction in recentTransactions.Take(5))
                {
                    var typeSymbol = transaction.Type == TransactionType.Income ? "+" : "-";
                    var typeColor = transaction.Type == TransactionType.Income ? ConsoleColor.Green : ConsoleColor.Red;
                    
                    Console.Write($"   {transaction.Date:MMM dd} - {transaction.Description} ");
                    Console.ForegroundColor = typeColor;
                    Console.WriteLine($"({typeSymbol}${transaction.Amount:N2})");
                    Console.ResetColor();
                }
            }
        }
    }
}
