using BudgetManager.Services;
using BudgetManager.Models;
using BudgetManager.UI;
using System;
using System.Windows.Forms;

namespace BudgetManager.Debug
{
    static class ProgramTest
    {
        [STAThread]
        static void Main()
        {
            try
            {
                // Create minimal services
                var dataService = new JsonDataService();
                var budgetService = new BudgetService(dataService);
                var reportService = new ReportService(budgetService);

                // Initialize Windows Forms
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                // Create and run form
                MessageBox.Show("About to create main form", "Debug");
                var form = new MainForm(budgetService, reportService);
                MessageBox.Show("Form created, about to run application", "Debug");
                Application.Run(form);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}\n\nStack Trace:\n{ex.StackTrace}", "Application Error");
            }
        }
    }
}
