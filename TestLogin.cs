using System;
using System.Windows.Forms;

namespace BudgetManager.Debug
{
    public class TestLogin
    {
        [STAThread]
        public static void TestMain() // Changed from Main to TestMain to avoid entry point conflict
        {
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                
                MessageBox.Show("Test application started successfully!", "Test", MessageBoxButtons.OK, MessageBoxIcon.Information);
                
                var form = new Form
                {
                    Text = "Test Form",
                    Size = new System.Drawing.Size(400, 300)
                };
                
                var button = new Button
                {
                    Text = "Close",
                    Location = new System.Drawing.Point(150, 120),
                    Size = new System.Drawing.Size(100, 30)
                };
                button.Click += (s, e) => form.Close();
                
                form.Controls.Add(button);
                Application.Run(form);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}\n\n{ex.StackTrace}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
