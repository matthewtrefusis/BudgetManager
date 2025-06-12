using BudgetManager.Services;
using BudgetManager.Models;
using System;
using System.Windows.Forms;

namespace BudgetManager
{
    class DebugProgram
    {
        [STAThread]
        static void Main()
        {
            try
            {
                Console.WriteLine("Starting application debugging...");
                
                Console.WriteLine("1. Creating services...");
                var dataService = new JsonDataService();
                var budgetService = new BudgetService(dataService);
                var reportService = new ReportService(budgetService);
                
                Console.WriteLine("2. Setting up Windows Forms...");
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                
                Console.WriteLine("3. Creating MainForm...");
                var mainForm = new UI.MainForm(budgetService, reportService);
                
                Console.WriteLine("4. Running application...");
                Application.Run(mainForm);
                
                Console.WriteLine("Application completed successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                MessageBox.Show($"Error: {ex.Message}\n\nStack trace: {ex.StackTrace}", "Application Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
