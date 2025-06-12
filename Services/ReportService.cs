using BudgetManager.Models;

namespace BudgetManager.Services
{
    public class ReportService
    {
        private readonly IBudgetService _budgetService;

        public ReportService(IBudgetService budgetService)
        {
            _budgetService = budgetService;
        }

        public async Task<string> GenerateMonthlyReportAsync()
        {
            var startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var report = $"=== MONTHLY BUDGET REPORT ({startDate:MMMM yyyy}) ===\n\n";

            // Financial Summary
            var income = await _budgetService.GetTotalIncomeAsync(startDate, endDate);
            var expenses = await _budgetService.GetTotalExpensesAsync(startDate, endDate);
            var netIncome = income - expenses;

            report += "📊 Financial Summary:\n";
            report += $"   Total Income:  ${income:N2}\n";
            report += $"   Total Expenses: ${Math.Abs(expenses):N2}\n";
            report += $"   Net Income:    ${netIncome:N2}\n";
            report += $"   Status: {(netIncome >= 0 ? "✓ Positive" : "⚠ Negative")}\n\n";

            // Expenses by Category
            var expensesByCategory = await _budgetService.GetExpensesByCategoryAsync(startDate, endDate);
            if (expensesByCategory.Any())
            {
                report += "💰 Expenses by Category:\n";
                foreach (var category in expensesByCategory.OrderByDescending(x => x.Value))
                {
                    var percentage = expenses > 0 ? (Math.Abs(category.Value) / Math.Abs(expenses)) * 100 : 0;
                    report += $"   {category.Key}: ${Math.Abs(category.Value):N2} ({percentage:F1}%)\n";
                }
                report += "\n";
            }

            // Budget Performance
            var budgetReport = await GenerateBudgetPerformanceReportAsync();
            report += budgetReport + "\n";

            // Financial Goals Progress
            var goalsReport = await GenerateGoalProgressReportAsync();
            report += goalsReport + "\n";

            return report;
        }

        public async Task<string> GenerateBudgetPerformanceReportAsync()
        {
            var budgets = await _budgetService.GetAllBudgetsAsync();
            
            if (!budgets.Any())
            {
                return "📝 No budgets configured.\n";
            }

            var report = "📝 Budget Performance:\n\n";

            foreach (var budget in budgets.OrderBy(b => b.Category))
            {
                var usagePercentage = budget.BudgetUtilizationPercentage;
                var status = budget.IsOverBudget ? "⚠ Over" : 
                           usagePercentage > 80 ? "⚡ High" : 
                           "✓ Good";

                report += $"Category: {budget.Category}\n";
                report += $"  Budget: ${budget.MonthlyLimit:N2}\n";
                report += $"  Spent: ${budget.CurrentSpent:N2}\n";
                report += $"  Remaining: ${budget.RemainingBudget:N2}\n";
                report += $"  Usage: {usagePercentage:F1}%\n";
                report += $"  Status: {status}\n\n";
            }

            return report;
        }

        public async Task<string> GenerateGoalProgressReportAsync()
        {
            var goals = await _budgetService.GetAllGoalsAsync();
            
            if (!goals.Any())
            {
                return "🎯 No financial goals set.\n";
            }

            var report = "🎯 Financial Goals Progress:\n\n";

            foreach (var goal in goals.OrderBy(g => g.TargetDate))
            {
                var progressPercentage = (goal.CurrentAmount / goal.TargetAmount) * 100;
                var daysRemaining = (goal.TargetDate - DateTime.Now).Days;
                var monthsRemaining = Math.Max(1, daysRemaining / 30.0);
                var requiredMonthlySavings = (goal.TargetAmount - goal.CurrentAmount) / (decimal)monthsRemaining;

                report += $"Goal: {goal.Name}\n";
                report += $"  Target: ${goal.TargetAmount:N2}\n";
                report += $"  Current: ${goal.CurrentAmount:N2}\n";
                report += $"  Progress: {progressPercentage:F1}%\n";
                report += $"  Days Left: {(daysRemaining > 0 ? daysRemaining.ToString() : "Overdue")}\n";
                report += $"  Monthly Required: ${requiredMonthlySavings:N2}\n";
                report += $"  Target Date: {goal.TargetDate:MMM dd, yyyy}\n\n";
            }

            return report;
        }

        public async Task<string> GenerateYearlyReportAsync()
        {
            var startDate = new DateTime(DateTime.Now.Year, 1, 1);
            var endDate = new DateTime(DateTime.Now.Year, 12, 31);

            var report = $"=== YEARLY BUDGET REPORT ({DateTime.Now.Year}) ===\n\n";

            var income = await _budgetService.GetTotalIncomeAsync(startDate, endDate);
            var expenses = await _budgetService.GetTotalExpensesAsync(startDate, endDate);
            var netIncome = income - expenses;

            report += "📊 Yearly Financial Summary:\n";
            report += $"   Total Income:  ${income:N2}\n";
            report += $"   Total Expenses: ${Math.Abs(expenses):N2}\n";
            report += $"   Net Income:    ${netIncome:N2}\n";
            report += $"   Savings Rate:  {(income > 0 ? (netIncome / income) * 100 : 0):F1}%\n\n";

            // Monthly breakdown
            report += "📅 Monthly Breakdown:\n";
            for (int month = 1; month <= 12; month++)
            {
                var monthStart = new DateTime(DateTime.Now.Year, month, 1);
                var monthEnd = monthStart.AddMonths(1).AddDays(-1);

                var monthlyIncome = await _budgetService.GetTotalIncomeAsync(monthStart, monthEnd);
                var monthlyExpenses = await _budgetService.GetTotalExpensesAsync(monthStart, monthEnd);
                var monthlyNet = monthlyIncome - monthlyExpenses;

                report += $"   {monthStart:MMM}: Income ${monthlyIncome:N0}, Expenses ${Math.Abs(monthlyExpenses):N0}, Net ${monthlyNet:N0}\n";
            }

            return report;
        }

        public async Task<string> GenerateTransactionReportAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            startDate ??= DateTime.Now.AddMonths(-1);
            endDate ??= DateTime.Now;

            var transactions = await _budgetService.GetTransactionsByDateRangeAsync(startDate.Value, endDate.Value);

            var report = $"=== TRANSACTION REPORT ({startDate:MMM dd} - {endDate:MMM dd, yyyy}) ===\n\n";

            if (!transactions.Any())
            {
                return report + "No transactions found in the specified date range.\n";
            }

            report += $"{"Date",-12} {"Description",-25} {"Category",-15} {"Type",-8} {"Amount",10}\n";
            report += new string('-', 80) + "\n";

            foreach (var transaction in transactions.Take(50)) // Limit to 50 most recent
            {
                var description = transaction.Description.Length > 25 ? 
                    transaction.Description.Substring(0, 22) + "..." : 
                    transaction.Description;

                report += $"{transaction.Date:MMM dd, yyyy} {description,-25} {transaction.Category,-15} {transaction.Type,-8} ${transaction.Amount,7:N2}\n";
            }

            if (transactions.Count > 50)
            {
                report += $"\n... and {transactions.Count - 50} more transactions.\n";
            }

            return report;
        }
    }
}