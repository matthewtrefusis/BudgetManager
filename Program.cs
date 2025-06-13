using BudgetManager.Services;
using BudgetManager.Models;
using BudgetManager.UI;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BudgetManager
{
    class Program
    {
        [STAThread]
        static async Task Main(string[] args)
        {
            // Enable visual styles for Windows Forms
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            User? currentUser = null;
              try
            {
                // Show a simple message box to verify Windows Forms functionality
                MessageBox.Show("Budget Manager is starting up...", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);

                var securityAuditService = new SecurityAuditService();
                var sessionManager = new SessionManager(securityAuditService);
                var userService = new UserService();

                // Show login form instead of console prompt
                using (var loginForm = new LoginForm(userService))
                {
                    if (loginForm.ShowDialog() == DialogResult.OK)
                    {
                        currentUser = loginForm.CurrentUser;
                    }
                    else
                    {
                        // User cancelled login
                        return;
                    }
                }

                if (currentUser == null)
                {
                    MessageBox.Show("Authentication failed. Application will now exit.", "Authentication Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                
                // Start session tracking
                sessionManager.StartSession(currentUser);
                  var dataService = new JsonDataService(currentUser);
                var budgetService = new BudgetService(dataService, currentUser);
                var reportService = new ReportService(budgetService);
                
                // Initialize budget service with data
                await budgetService.InitializeAsync();

                // Handle session expiration
                sessionManager.SessionExpired += (sender, e) => 
                {
                    // Use the thread-safe way to update UI from a different thread
                    var mainForm = Application.OpenForms.OfType<Form>().FirstOrDefault();
                    if (mainForm != null && !mainForm.IsDisposed)
                    {
                        mainForm.Invoke(new Action(() => {
                            MessageBox.Show("Your session has expired due to inactivity.", "Session Expired", 
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            Application.Exit();
                        }));
                    }
                    else
                    {
                        Application.Exit();
                    }
                };
                
                // Create and run the main form
                var mainForm = new BudgetManager.UI.MainForm(budgetService, reportService, currentUser, sessionManager);
                Application.Run(mainForm);
                
                // End the session when application closes
                sessionManager.EndSession();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An unexpected error occurred: {ex.Message}\n\n{ex.StackTrace}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
