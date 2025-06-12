using BudgetManager.Services;
using BudgetManager.UI;

namespace BudgetManager
{
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                Console.Title = "Budget Manager - Personal Finance Assistant";
                
                // Initialize services
                var dataService = new JsonDataService();
                var budgetService = new BudgetService(dataService);
                var reportService = new ReportService(budgetService);
                var consoleUI = new ConsoleUI(budgetService, reportService);

                // Initialize budget service with data
                await budgetService.InitializeAsync();

                // Start the application
                await consoleUI.RunAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An unexpected error occurred: {ex.Message}");
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
        }
    }
}
