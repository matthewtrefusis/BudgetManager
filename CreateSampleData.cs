using BudgetManager.Models;
using BudgetManager.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BudgetManager
{
    public class CreateSampleData
    {        public static async Task GenerateSampleDataAsync()
        {
            var user = await GetOrCreateDemoUser();
            var dataService = new JsonDataService(user);  // Create sample transactions with user context
            var transactions = new List<Transaction>
            {
                new Transaction("Salary", 5000, TransactionType.Income, "Income") { Date = DateTime.Now.AddDays(-30) },
                new Transaction("Rent", 1200, TransactionType.Expense, "Housing") { Date = DateTime.Now.AddDays(-28) },
                new Transaction("Groceries", 150, TransactionType.Expense, "Food") { Date = DateTime.Now.AddDays(-25) },
                new Transaction("Gas", 60, TransactionType.Expense, "Transportation") { Date = DateTime.Now.AddDays(-24) },
                new Transaction("Coffee", 15, TransactionType.Expense, "Food") { Date = DateTime.Now.AddDays(-22) },
                new Transaction("Freelance Work", 800, TransactionType.Income, "Income") { Date = DateTime.Now.AddDays(-20) },
                new Transaction("Utilities", 200, TransactionType.Expense, "Housing") { Date = DateTime.Now.AddDays(-18) },
                new Transaction("Restaurant", 85, TransactionType.Expense, "Food") { Date = DateTime.Now.AddDays(-15) },
                new Transaction("Movie Tickets", 25, TransactionType.Expense, "Entertainment") { Date = DateTime.Now.AddDays(-12) },
                new Transaction("Internet", 80, TransactionType.Expense, "Housing") { Date = DateTime.Now.AddDays(-10) },
                new Transaction("Groceries", 120, TransactionType.Expense, "Food") { Date = DateTime.Now.AddDays(-8) },
                new Transaction("Bus Pass", 75, TransactionType.Expense, "Transportation") { Date = DateTime.Now.AddDays(-7) },
                new Transaction("Book Purchase", 30, TransactionType.Expense, "Education") { Date = DateTime.Now.AddDays(-5) },
                new Transaction("Bonus", 1000, TransactionType.Income, "Income") { Date = DateTime.Now.AddDays(-3) },
                new Transaction("Phone Bill", 75, TransactionType.Expense, "Housing") { Date = DateTime.Now.AddDays(-2) }
            };

            // Create sample budgets
            var budgets = new List<Budget>
            {
                new Budget { Category = "Food", MonthlyLimit = 500, CurrentSpent = 370 },
                new Budget { Category = "Housing", MonthlyLimit = 1500, CurrentSpent = 1555 },
                new Budget { Category = "Transportation", MonthlyLimit = 200, CurrentSpent = 60 },
                new Budget { Category = "Entertainment", MonthlyLimit = 100, CurrentSpent = 25 },
                new Budget { Category = "Education", MonthlyLimit = 150, CurrentSpent = 30 }
            };

            // Create sample financial goals
            var goals = new List<FinancialGoal>
            {
                new FinancialGoal 
                { 
                    Name = "Emergency Fund", 
                    TargetAmount = 10000, 
                    CurrentAmount = 3500, 
                    TargetDate = DateTime.Now.AddMonths(12),
                    Description = "Build emergency fund covering 6 months of expenses"
                },
                new FinancialGoal 
                { 
                    Name = "Vacation Fund", 
                    TargetAmount = 3000, 
                    CurrentAmount = 1200, 
                    TargetDate = DateTime.Now.AddMonths(6),
                    Description = "Save for a summer vacation to Europe"
                },
                new FinancialGoal 
                { 
                    Name = "New Laptop", 
                    TargetAmount = 1500, 
                    CurrentAmount = 800, 
                    TargetDate = DateTime.Now.AddMonths(3),
                    Description = "Save for a new gaming laptop"
                },
                new FinancialGoal 
                { 
                    Name = "Investment Portfolio", 
                    TargetAmount = 25000, 
                    CurrentAmount = 8500, 
                    TargetDate = DateTime.Now.AddMonths(24),
                    Description = "Build a diversified investment portfolio"
                }
            };            // Save all sample data
            await dataService.SaveTransactionsAsync(transactions);
            await dataService.SaveBudgetsAsync(budgets);
            await dataService.SaveGoalsAsync(goals);
        }        private static async Task<User> GetOrCreateDemoUser()
        {
            var userService = new UserService();
            const string demoUsername = "demo";
            const string demoPassword = "Demo123!@#"; // Using a stronger password that meets requirements// Try to log in as demo user
            var (user, errorMessage) = await userService.Login(demoUsername, demoPassword);
            
            // If demo user doesn't exist, create it
            if (user == null)
            {
                await userService.Register(demoUsername, demoPassword);
                (user, errorMessage) = await userService.Login(demoUsername, demoPassword);
            }

            return user ?? throw new InvalidOperationException($"Failed to create or get demo user: {errorMessage}");
        }
    }
}
