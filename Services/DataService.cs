using BudgetManager.Models;
using Newtonsoft.Json;

namespace BudgetManager.Services
{
    public interface IDataService
    {
        Task<List<Transaction>> LoadTransactionsAsync();
        Task SaveTransactionsAsync(List<Transaction> transactions);
        Task<List<Budget>> LoadBudgetsAsync();
        Task SaveBudgetsAsync(List<Budget> budgets);
        Task<List<FinancialGoal>> LoadGoalsAsync();
        Task SaveGoalsAsync(List<FinancialGoal> goals);
    }

    public class JsonDataService : IDataService
    {
        private readonly string _dataDirectory;
        private readonly string _transactionsFile;
        private readonly string _budgetsFile;
        private readonly string _goalsFile;

        public JsonDataService()
        {
            _dataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BudgetManager");
            _transactionsFile = Path.Combine(_dataDirectory, "transactions.json");
            _budgetsFile = Path.Combine(_dataDirectory, "budgets.json");
            _goalsFile = Path.Combine(_dataDirectory, "goals.json");

            // Ensure data directory exists
            Directory.CreateDirectory(_dataDirectory);
        }

        public async Task<List<Transaction>> LoadTransactionsAsync()
        {
            if (!File.Exists(_transactionsFile))
                return new List<Transaction>();

            try
            {
                var json = await File.ReadAllTextAsync(_transactionsFile);
                return JsonConvert.DeserializeObject<List<Transaction>>(json) ?? new List<Transaction>();
            }
            catch
            {
                return new List<Transaction>();
            }
        }

        public async Task SaveTransactionsAsync(List<Transaction> transactions)
        {
            var json = JsonConvert.SerializeObject(transactions, Formatting.Indented);
            await File.WriteAllTextAsync(_transactionsFile, json);
        }

        public async Task<List<Budget>> LoadBudgetsAsync()
        {
            if (!File.Exists(_budgetsFile))
                return new List<Budget>();

            try
            {
                var json = await File.ReadAllTextAsync(_budgetsFile);
                return JsonConvert.DeserializeObject<List<Budget>>(json) ?? new List<Budget>();
            }
            catch
            {
                return new List<Budget>();
            }
        }

        public async Task SaveBudgetsAsync(List<Budget> budgets)
        {
            var json = JsonConvert.SerializeObject(budgets, Formatting.Indented);
            await File.WriteAllTextAsync(_budgetsFile, json);
        }

        public async Task<List<FinancialGoal>> LoadGoalsAsync()
        {
            if (!File.Exists(_goalsFile))
                return new List<FinancialGoal>();

            try
            {
                var json = await File.ReadAllTextAsync(_goalsFile);
                return JsonConvert.DeserializeObject<List<FinancialGoal>>(json) ?? new List<FinancialGoal>();
            }
            catch
            {
                return new List<FinancialGoal>();
            }
        }

        public async Task SaveGoalsAsync(List<FinancialGoal> goals)
        {
            var json = JsonConvert.SerializeObject(goals, Formatting.Indented);
            await File.WriteAllTextAsync(_goalsFile, json);
        }
    }
}
