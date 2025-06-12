using BudgetManager.Services;
using BudgetManager.UI;

namespace BudgetManager
{
    class Program
    {
        [STAThread]
        static async Task Main(string[] args)
        {
            try
            {
                // Enable visual styles for Windows Forms
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                
                // Initialize services
                var dataService = new JsonDataService();
                var budgetService = new BudgetService(dataService);
                var reportService = new ReportService(budgetService);

                // Initialize budget service with data
                await budgetService.InitializeAsync();

                // Check if we need to create sample data
                var transactions = await budgetService.GetAllTransactionsAsync();
                if (!transactions.Any())
                {
                    var result = MessageBox.Show(
                        "No data found. Would you like to create sample data to explore the application?",
                        "Welcome to Budget Manager",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        await CreateSampleData.GenerateSampleDataAsync();
                        await budgetService.InitializeAsync(); // Reload data
                        MessageBox.Show("Sample data created successfully! You can now explore all features of the Budget Manager.", 
                                      "Sample Data Created", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }

                // Create and run the main form
                var mainForm = new MainForm(budgetService, reportService);
                Application.Run(mainForm);
            }
            catch (Exception ex)
            {                MessageBox.Show($"An unexpected error occurred: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
