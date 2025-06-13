using BudgetManager.Models;
using BudgetManager.Services;

namespace BudgetManager.Services
{
    public interface IBudgetService
    {
        Task<List<Transaction>> GetAllTransactionsAsync();
        Task<List<Transaction>> GetTransactionsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<List<Transaction>> GetTransactionsByCategoryAsync(string category);
        Task AddTransactionAsync(Transaction transaction);
        Task UpdateTransactionAsync(Transaction transaction);
        Task DeleteTransactionAsync(Guid transactionId);
        Task<decimal> GetTotalIncomeAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<decimal> GetTotalExpensesAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<decimal> GetNetIncomeAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<Dictionary<string, decimal>> GetExpensesByCategoryAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<List<Budget>> GetAllBudgetsAsync();
        Task AddBudgetAsync(Budget budget);
        Task UpdateBudgetAsync(Budget budget);
        Task DeleteBudgetAsync(Guid budgetId);
        Task UpdateBudgetSpendingAsync();
        Task<List<FinancialGoal>> GetAllGoalsAsync();
        Task AddGoalAsync(FinancialGoal goal);
        Task UpdateGoalAsync(FinancialGoal goal);
        Task DeleteGoalAsync(Guid goalId);
        Task UpdateGoalProgressAsync(Guid goalId, decimal amount);
    }

    public class BudgetService : IBudgetService
    {        private readonly IDataService _dataService;
        private List<Transaction> _transactions = new List<Transaction>();
        private List<Budget> _budgets = new List<Budget>();
        private List<FinancialGoal> _goals = new List<FinancialGoal>();
        private User? _currentUser;

        public BudgetService(IDataService dataService, User? currentUser = null)
        {
            _dataService = dataService;
            _currentUser = currentUser;
        }

        public void SetCurrentUser(User user)
        {
            _currentUser = user;
            if (_dataService is JsonDataService jsonDataService)
                jsonDataService.SetCurrentUser(user);
        }

        public async Task InitializeAsync()
        {
            _transactions = await _dataService.LoadTransactionsAsync();
            _budgets = await _dataService.LoadBudgetsAsync();
            _goals = await _dataService.LoadGoalsAsync();
        }        // Transaction methods
        public Task<List<Transaction>> GetAllTransactionsAsync()
        {
            return Task.FromResult(_transactions.OrderByDescending(t => t.Date).ToList());
        }

        public Task<List<Transaction>> GetTransactionsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return Task.FromResult(_transactions
                .Where(t => t.Date.Date >= startDate.Date && t.Date.Date <= endDate.Date)
                .OrderByDescending(t => t.Date)
                .ToList());
        }

        public Task<List<Transaction>> GetTransactionsByCategoryAsync(string category)
        {
            return Task.FromResult(_transactions
                .Where(t => t.Category.Equals(category, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(t => t.Date)
                .ToList());
        }

        public async Task AddTransactionAsync(Transaction transaction)
        {
            _transactions.Add(transaction);
            await _dataService.SaveTransactionsAsync(_transactions);
            await UpdateBudgetSpendingAsync();
        }

        public async Task UpdateTransactionAsync(Transaction transaction)
        {
            var index = _transactions.FindIndex(t => t.Id == transaction.Id);
            if (index >= 0)
            {
                _transactions[index] = transaction;
                await _dataService.SaveTransactionsAsync(_transactions);
                await UpdateBudgetSpendingAsync();
            }
        }

        public async Task DeleteTransactionAsync(Guid transactionId)
        {
            _transactions.RemoveAll(t => t.Id == transactionId);
            await _dataService.SaveTransactionsAsync(_transactions);
            await UpdateBudgetSpendingAsync();
        }        public Task<decimal> GetTotalIncomeAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var transactions = _transactions.Where(t => t.Type == TransactionType.Income);
            
            if (startDate.HasValue)
                transactions = transactions.Where(t => t.Date.Date >= startDate.Value.Date);
            
            if (endDate.HasValue)
                transactions = transactions.Where(t => t.Date.Date <= endDate.Value.Date);

            return Task.FromResult(transactions.Sum(t => t.Amount));
        }

        public Task<decimal> GetTotalExpensesAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var transactions = _transactions.Where(t => t.Type == TransactionType.Expense);
            
            if (startDate.HasValue)
                transactions = transactions.Where(t => t.Date.Date >= startDate.Value.Date);
            
            if (endDate.HasValue)
                transactions = transactions.Where(t => t.Date.Date <= endDate.Value.Date);

            return Task.FromResult(transactions.Sum(t => t.Amount));
        }

        public async Task<decimal> GetNetIncomeAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var income = await GetTotalIncomeAsync(startDate, endDate);
            var expenses = await GetTotalExpensesAsync(startDate, endDate);
            return income - expenses;
        }        public Task<Dictionary<string, decimal>> GetExpensesByCategoryAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var transactions = _transactions.Where(t => t.Type == TransactionType.Expense);
            
            if (startDate.HasValue)
                transactions = transactions.Where(t => t.Date.Date >= startDate.Value.Date);
            
            if (endDate.HasValue)
                transactions = transactions.Where(t => t.Date.Date <= endDate.Value.Date);

            return Task.FromResult(transactions
                .GroupBy(t => t.Category)
                .ToDictionary(g => g.Key, g => g.Sum(t => t.Amount)));
        }

        // Budget methods
        public Task<List<Budget>> GetAllBudgetsAsync()
        {
            return Task.FromResult(_budgets.Where(b => b.IsActive).ToList());
        }

        public async Task AddBudgetAsync(Budget budget)
        {
            _budgets.Add(budget);
            await _dataService.SaveBudgetsAsync(_budgets);
            await UpdateBudgetSpendingAsync();
        }

        public async Task UpdateBudgetAsync(Budget budget)
        {
            var index = _budgets.FindIndex(b => b.Id == budget.Id);
            if (index >= 0)
            {
                _budgets[index] = budget;
                await _dataService.SaveBudgetsAsync(_budgets);
            }
        }

        public async Task DeleteBudgetAsync(Guid budgetId)
        {
            var budget = _budgets.FirstOrDefault(b => b.Id == budgetId);
            if (budget != null)
            {
                budget.IsActive = false;
                await _dataService.SaveBudgetsAsync(_budgets);
            }
        }

        public async Task UpdateBudgetSpendingAsync()
        {
            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;

            foreach (var budget in _budgets.Where(b => b.IsActive))
            {
                var monthlyExpenses = _transactions
                    .Where(t => t.Type == TransactionType.Expense &&
                               t.Category.Equals(budget.Category, StringComparison.OrdinalIgnoreCase) &&
                               t.Date.Month == currentMonth &&
                               t.Date.Year == currentYear)
                    .Sum(t => t.Amount);

                budget.CurrentSpent = monthlyExpenses;
            }

            await _dataService.SaveBudgetsAsync(_budgets);
        }        // Goal methods
        public Task<List<FinancialGoal>> GetAllGoalsAsync()
        {
            return Task.FromResult(_goals.Where(g => !g.IsCompleted).ToList());
        }

        public async Task AddGoalAsync(FinancialGoal goal)
        {
            _goals.Add(goal);
            await _dataService.SaveGoalsAsync(_goals);
        }

        public async Task UpdateGoalAsync(FinancialGoal goal)
        {
            var index = _goals.FindIndex(g => g.Id == goal.Id);
            if (index >= 0)
            {
                _goals[index] = goal;
                await _dataService.SaveGoalsAsync(_goals);
            }
        }

        public async Task DeleteGoalAsync(Guid goalId)
        {
            _goals.RemoveAll(g => g.Id == goalId);
            await _dataService.SaveGoalsAsync(_goals);
        }

        public async Task UpdateGoalProgressAsync(Guid goalId, decimal amount)
        {
            var goal = _goals.FirstOrDefault(g => g.Id == goalId);
            if (goal != null)
            {
                goal.CurrentAmount += amount;
                if (goal.CurrentAmount >= goal.TargetAmount)
                {
                    goal.IsCompleted = true;
                    goal.CurrentAmount = goal.TargetAmount;
                }
                await _dataService.SaveGoalsAsync(_goals);
            }
        }
    }
}
