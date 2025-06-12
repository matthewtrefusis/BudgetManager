using BudgetManager.Models;
using Newtonsoft.Json;
using System.Text;

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
    }    public class JsonDataService : IDataService
    {
        private readonly string _dataDirectory;
        private readonly string _transactionsFile;
        private readonly string _budgetsFile;
        private readonly string _goalsFile;
        private readonly string _usersFile;
        private readonly string _tempDirectory;
        private readonly EncryptionService _encryptionService;
        private User? _currentUser;

        public JsonDataService(User? currentUser = null)
        {
            _dataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BudgetManager");
            _tempDirectory = Path.Combine(_dataDirectory, "temp");
            _transactionsFile = Path.Combine(_dataDirectory, "transactions.json");
            _budgetsFile = Path.Combine(_dataDirectory, "budgets.json");
            _goalsFile = Path.Combine(_dataDirectory, "goals.json");
            _usersFile = Path.Combine(_dataDirectory, "users.json");
            Directory.CreateDirectory(_dataDirectory);
            Directory.CreateDirectory(_tempDirectory);
            _currentUser = currentUser;
            _encryptionService = new EncryptionService();
        }

        public void SetCurrentUser(User user)
        {
            _currentUser = user;
        }

        private string GetUserDataFile(string baseFile)
        {
            if (_currentUser == null)
                return baseFile;
            var userFile = Path.Combine(_dataDirectory, $"{_currentUser.Username}_{Path.GetFileName(baseFile)}");
            return userFile;
        }        public async Task<List<Transaction>> LoadTransactionsAsync()
        {
            var file = GetUserDataFile(_transactionsFile);
            if (!File.Exists(file))
                return new List<Transaction>();
            try
            {
                var json = await ReadAndDecryptFileAsync(file);
                return DeserializeWithValidation<List<Transaction>>(json) ?? new List<Transaction>();
            }
            catch
            {
                return new List<Transaction>();
            }
        }

        public async Task SaveTransactionsAsync(List<Transaction> transactions)
        {
            var file = GetUserDataFile(_transactionsFile);
            var json = JsonConvert.SerializeObject(transactions, Formatting.Indented);
            await EncryptAndSaveFileAsync(file, json);
        }        public async Task<List<Budget>> LoadBudgetsAsync()
        {
            var file = GetUserDataFile(_budgetsFile);
            if (!File.Exists(file))
                return new List<Budget>();
            try
            {
                var json = await ReadAndDecryptFileAsync(file);
                return DeserializeWithValidation<List<Budget>>(json) ?? new List<Budget>();
            }
            catch
            {
                return new List<Budget>();
            }
        }

        public async Task SaveBudgetsAsync(List<Budget> budgets)
        {
            var file = GetUserDataFile(_budgetsFile);
            var json = JsonConvert.SerializeObject(budgets, Formatting.Indented);
            await EncryptAndSaveFileAsync(file, json);
        }        public async Task<List<FinancialGoal>> LoadGoalsAsync()
        {
            var file = GetUserDataFile(_goalsFile);
            if (!File.Exists(file))
                return new List<FinancialGoal>();
            try
            {
                var json = await ReadAndDecryptFileAsync(file);
                return DeserializeWithValidation<List<FinancialGoal>>(json) ?? new List<FinancialGoal>();
            }
            catch
            {
                return new List<FinancialGoal>();
            }
        }

        public async Task SaveGoalsAsync(List<FinancialGoal> goals)
        {
            var file = GetUserDataFile(_goalsFile);
            var json = JsonConvert.SerializeObject(goals, Formatting.Indented);
            await EncryptAndSaveFileAsync(file, json);
        }

        private bool IsEncryptedFile(string filePath)
        {
            if (!File.Exists(filePath))
                return false;
                
            try
            {
                // Check if the file starts with a base64 character
                var firstFewBytes = File.ReadAllText(filePath, Encoding.UTF8).Take(10).ToArray();
                string start = new string(firstFewBytes);
                
                // Check if it could be base64 (encrypted)
                return !start.Contains("{") && !start.Contains("[");
            }
            catch
            {
                return false;
            }
        }
        
        private async Task<string> ReadAndDecryptFileAsync(string filePath)
        {
            if (!File.Exists(filePath))
                return "[]";
                
            try
            {
                string fileContent = await File.ReadAllTextAsync(filePath);
                
                // Check if the file is already encrypted
                if (IsEncryptedFile(filePath))
                {
                    return _encryptionService.DecryptString(fileContent);
                }
                
                // For backwards compatibility with unencrypted files
                return fileContent;
            }
            catch
            {
                return "[]";
            }
        }
        
        private async Task EncryptAndSaveFileAsync(string filePath, string content)
        {
            string encryptedContent = _encryptionService.EncryptString(content);
            await File.WriteAllTextAsync(filePath, encryptedContent);
        }

        private T? DeserializeWithValidation<T>(string json) where T : class
        {
            try
            {
                // Use a more secure deserialization approach with explicit settings
                var settings = new JsonSerializerSettings
                {
                    // Prevent loading types not intended to be deserialized
                    TypeNameHandling = TypeNameHandling.None,
                    
                    // Be strict about JSON schema, no automatic type conversion
                    MetadataPropertyHandling = MetadataPropertyHandling.Default,
                    
                    // Don't allow loading objects from references
                    PreserveReferencesHandling = PreserveReferencesHandling.None,
                    
                    // Don't deserialize error states
                    Error = (sender, args) => 
                    {
                        System.Diagnostics.Debug.WriteLine($"JSON Deserialization Error: {args.ErrorContext.Error}");
                        args.ErrorContext.Handled = true;
                    }
                };
                
                // Check for unexpected tokens before deserializing
                using (var stringReader = new StringReader(json))
                using (var jsonReader = new JsonTextReader(stringReader))
                {
                    var serializer = JsonSerializer.Create(settings);
                    return serializer.Deserialize<T>(jsonReader);
                }
            }
            catch (JsonReaderException ex)
            {
                // Log format errors
                System.Diagnostics.Debug.WriteLine($"Invalid JSON format: {ex.Message}");
                return null;
            }
            catch (JsonSerializationException ex)
            {
                // Log serialization issues
                System.Diagnostics.Debug.WriteLine($"JSON Serialization error: {ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                // Log any other issues
                System.Diagnostics.Debug.WriteLine($"Unexpected error deserializing JSON: {ex.Message}");
                return null;
            }
        }
    }
}
