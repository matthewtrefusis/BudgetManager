using BudgetManager.Models;
using ConsoleTables;

namespace BudgetManager.Services
{
    public class ReportService
    {
        private readonly IBudgetService _budgetService;

        public ReportService(IBudgetService budgetService)
        {
            _budgetService = budgetService;
        }

        public async Task GenerateMonthlyReportAsync()
        {
            var startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            Console.WriteLine($"\n=== MONTHLY BUDGET REPORT ({startDate:MMMM yyyy}) ===");

            // Financial Summary
            var income = await _budgetService.GetTotalIncomeAsync(startDate, endDate);
            var expenses = await _budgetService.GetTotalExpensesAsync(startDate, endDate);
            var netIncome = income - expenses;

            Console.WriteLine($"\n📊 Financial Summary:");
            Console.WriteLine($"   Total Income:  ${income:N2}");
            Console.WriteLine($"   Total Expenses: ${expenses:N2}");
            Console.WriteLine($"   Net Income:    ${netIncome:N2}");
            
            if (netIncome >= 0)
                Console.ForegroundColor = ConsoleColor.Green;
            else
                Console.ForegroundColor = ConsoleColor.Red;
            
            Console.WriteLine($"   Status: {(netIncome >= 0 ? "✓ Positive" : "⚠ Negative")}");
            Console.ResetColor();

            // Expenses by Category
            var expensesByCategory = await _budgetService.GetExpensesByCategoryAsync(startDate, endDate);
            if (expensesByCategory.Any())
            {
                Console.WriteLine($"\n💰 Expenses by Category:");
                var table = new ConsoleTable("Category", "Amount", "Percentage");
                
                foreach (var category in expensesByCategory.OrderByDescending(x => x.Value))
                {
                    var percentage = expenses > 0 ? (category.Value / expenses) * 100 : 0;
                    table.AddRow(category.Key, $"${category.Value:N2}", $"{percentage:F1}%");
                }
                
                table.Write(Format.Alternative);
            }

            // Budget Performance
            await GenerateBudgetPerformanceReportAsync();

            // Financial Goals Progress
            await GenerateGoalProgressReportAsync();
        }

        public async Task GenerateBudgetPerformanceReportAsync()
        {
            var budgets = await _budgetService.GetAllBudgetsAsync();
            
            if (!budgets.Any())
            {
                Console.WriteLine("\n📝 No budgets configured.");
                return;
            }

            Console.WriteLine($"\n📝 Budget Performance:");
            var table = new ConsoleTable("Category", "Budget", "Spent", "Remaining", "Usage %", "Status");

            foreach (var budget in budgets.OrderBy(b => b.Category))
            {
                var usagePercentage = budget.BudgetUtilizationPercentage;
                var status = budget.IsOverBudget ? "⚠ Over" : 
                           usagePercentage > 80 ? "⚡ High" : 
                           "✓ Good";

                table.AddRow(
                    budget.Category,
                    $"${budget.MonthlyLimit:N2}",
                    $"${budget.CurrentSpent:N2}",
                    $"${budget.RemainingBudget:N2}",
                    $"{usagePercentage:F1}%",
                    status
                );
            }

            table.Write(Format.Alternative);
        }

        public async Task GenerateGoalProgressReportAsync()
        {
            var goals = await _budgetService.GetAllGoalsAsync();
            
            if (!goals.Any())
            {
                Console.WriteLine("\n🎯 No financial goals set.");
                return;
            }

            Console.WriteLine($"\n🎯 Financial Goals Progress:");
            var table = new ConsoleTable("Goal", "Target", "Current", "Progress %", "Days Left", "Monthly Req.");

            foreach (var goal in goals.OrderBy(g => g.TargetDate))
            {
                table.AddRow(
                    goal.Name,
                    $"${goal.TargetAmount:N2}",
                    $"${goal.CurrentAmount:N2}",
                    $"{goal.ProgressPercentage:F1}%",
                    goal.DaysRemaining > 0 ? goal.DaysRemaining.ToString() : "Overdue",
                    $"${goal.RequiredMonthlySavings:N2}"
                );
            }

            table.Write(Format.Alternative);
        }

        public async Task GenerateYearlyReportAsync()
        {
            var startDate = new DateTime(DateTime.Now.Year, 1, 1);
            var endDate = new DateTime(DateTime.Now.Year, 12, 31);

            Console.WriteLine($"\n=== YEARLY BUDGET REPORT ({DateTime.Now.Year}) ===");

            var income = await _budgetService.GetTotalIncomeAsync(startDate, endDate);
            var expenses = await _budgetService.GetTotalExpensesAsync(startDate, endDate);
            var netIncome = income - expenses;

            Console.WriteLine($"\n📊 Yearly Financial Summary:");
            Console.WriteLine($"   Total Income:  ${income:N2}");
            Console.WriteLine($"   Total Expenses: ${expenses:N2}");
            Console.WriteLine($"   Net Income:    ${netIncome:N2}");
            Console.WriteLine($"   Savings Rate:  {(income > 0 ? (netIncome / income) * 100 : 0):F1}%");

            // Monthly breakdown
            Console.WriteLine($"\n📅 Monthly Breakdown:");
            var monthlyTable = new ConsoleTable("Month", "Income", "Expenses", "Net Income");

            for (int month = 1; month <= 12; month++)
            {
                var monthStart = new DateTime(DateTime.Now.Year, month, 1);
                var monthEnd = monthStart.AddMonths(1).AddDays(-1);

                var monthlyIncome = await _budgetService.GetTotalIncomeAsync(monthStart, monthEnd);
                var monthlyExpenses = await _budgetService.GetTotalExpensesAsync(monthStart, monthEnd);
                var monthlyNet = monthlyIncome - monthlyExpenses;

                monthlyTable.AddRow(
                    monthStart.ToString("MMM"),
                    $"${monthlyIncome:N2}",
                    $"${monthlyExpenses:N2}",
                    $"${monthlyNet:N2}"
                );
            }

            monthlyTable.Write(Format.Alternative);
        }

        public async Task GenerateTransactionReportAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            startDate ??= DateTime.Now.AddMonths(-1);
            endDate ??= DateTime.Now;

            var transactions = await _budgetService.GetTransactionsByDateRangeAsync(startDate.Value, endDate.Value);

            Console.WriteLine($"\n=== TRANSACTION REPORT ({startDate:MMM dd} - {endDate:MMM dd, yyyy}) ===");

            if (!transactions.Any())
            {
                Console.WriteLine("No transactions found in the specified date range.");
                return;
            }

            var table = new ConsoleTable("Date", "Description", "Category", "Type", "Amount");

            foreach (var transaction in transactions.Take(50)) // Limit to 50 most recent
            {
                table.AddRow(
                    transaction.Date.ToString("MMM dd"),
                    transaction.Description.Length > 25 ? transaction.Description.Substring(0, 22) + "..." : transaction.Description,
                    transaction.Category,
                    transaction.Type.ToString(),
                    $"${transaction.Amount:N2}"
                );
            }

            table.Write(Format.Alternative);

            if (transactions.Count > 50)
            {
                Console.WriteLine($"\n... and {transactions.Count - 50} more transactions.");
            }
        }
    }
}
