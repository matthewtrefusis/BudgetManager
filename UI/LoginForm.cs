// filepath: c:\Code\BudgetManager\UI\LoginForm.cs
using BudgetManager.Models;
using BudgetManager.Services;
using System;
using System.Windows.Forms;

namespace BudgetManager.UI
{
    public class LoginForm : Form
    {
        private TextBox usernameTextBox;
        private TextBox passwordTextBox;
        private Button loginButton;
        private Button registerButton;
        private Label titleLabel;
        private Label statusLabel;
        private UserService _userService;
        private User? _currentUser = null;

        public User? CurrentUser => _currentUser;

        public LoginForm(UserService userService)
        {
            _userService = userService;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            // Form settings
            this.Text = "Budget Manager - Login";
            this.Size = new Size(400, 350);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(236, 240, 241);

            // Create main container panel for proper layout
            var mainContainer = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(0)
            };

            // Set row styles
            mainContainer.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));
            mainContainer.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            // Title label at the top
            titleLabel = new Label
            {
                Text = "Budget Manager",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 73, 94),
                TextAlign = ContentAlignment.MiddleCenter,
                AutoSize = false,
                Dock = DockStyle.Fill
            };
            mainContainer.Controls.Add(titleLabel, 0, 0);

            // Form fields panel
            var formPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(30),
                ColumnCount = 2,
                RowCount = 4
            };

            // Add columns to formPanel
            formPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
            formPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70));

            // Username row
            formPanel.Controls.Add(new Label { 
                Text = "Username:", 
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill
            }, 0, 0);
            
            usernameTextBox = new TextBox { 
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White
            };
            formPanel.Controls.Add(usernameTextBox, 1, 0);

            // Password row
            formPanel.Controls.Add(new Label { 
                Text = "Password:", 
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill
            }, 0, 1);
            
            passwordTextBox = new TextBox { 
                Dock = DockStyle.Fill, 
                UseSystemPasswordChar = true,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White
            };
            formPanel.Controls.Add(passwordTextBox, 1, 1);

            // Buttons row
            var buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft
            };

            loginButton = new Button
            {
                Text = "Login",
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(100, 30),
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            loginButton.FlatAppearance.BorderSize = 0;
            loginButton.Click += LoginButton_Click;

            registerButton = new Button
            {
                Text = "Register",
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(100, 30),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Margin = new Padding(5, 0, 0, 0)
            };
            registerButton.FlatAppearance.BorderSize = 0;
            registerButton.Click += RegisterButton_Click;

            buttonPanel.Controls.Add(loginButton);
            buttonPanel.Controls.Add(registerButton);

            formPanel.Controls.Add(buttonPanel, 0, 2);
            formPanel.SetColumnSpan(buttonPanel, 2);

            // Status label row
            statusLabel = new Label
            {
                Text = "",
                ForeColor = Color.Red,
                TextAlign = ContentAlignment.MiddleCenter,
                AutoSize = false,
                Height = 30,
                Dock = DockStyle.Fill
            };
            formPanel.Controls.Add(statusLabel, 0, 3);
            formPanel.SetColumnSpan(statusLabel, 2);

            // Add the form panel to the main container
            mainContainer.Controls.Add(formPanel, 0, 1);

            // Add the main container to the form
            this.Controls.Add(mainContainer);
            this.AcceptButton = loginButton;
        }        private async void LoginButton_Click(object? sender, EventArgs e)
        {
            // Disable login button to prevent multiple clicks
            loginButton.Enabled = false;
            statusLabel.Text = "Processing...";
            
            try
            {
                if (string.IsNullOrWhiteSpace(usernameTextBox.Text) || string.IsNullOrWhiteSpace(passwordTextBox.Text))
                {
                    statusLabel.Text = "Please enter both username and password";
                    return;
                }

                var (user, errorMessage) = await _userService.Login(usernameTextBox.Text, passwordTextBox.Text);
                
                if (user != null)
                {
                    _currentUser = user;
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    statusLabel.Text = errorMessage ?? "Login failed";
                    
                    // Clear password field on failed login for security
                    passwordTextBox.Clear();
                }
            }
            catch (Exception ex)
            {
                // Generic error message to avoid leaking implementation details
                statusLabel.Text = "An error occurred during login";
                System.Diagnostics.Debug.WriteLine($"Login error: {ex.Message}");
                passwordTextBox.Clear();
            }
            finally
            {
                // Re-enable login button
                loginButton.Enabled = true;
            }
        }        private async void RegisterButton_Click(object? sender, EventArgs e)
        {
            // Clear previous status
            statusLabel.ForeColor = Color.Red;
            
            // Disable register button to prevent multiple clicks
            registerButton.Enabled = false;
            statusLabel.Text = "Processing...";
            
            try
            {
                // Debug: Show the current values for debugging
                if (usernameTextBox == null)
                {
                    statusLabel.Text = "Error: Username textbox is null";
                    return;
                }
                
                string username = usernameTextBox.Text?.Trim() ?? "";
                string password = passwordTextBox.Text?.Trim() ?? "";
                
                // Check if either field is empty
                if (string.IsNullOrWhiteSpace(username))
                {
                    statusLabel.Text = "Please enter a username";
                    return;
                }
                
                if (string.IsNullOrWhiteSpace(password))
                {
                    statusLabel.Text = "Please enter a password";
                    return;
                }

                bool result = await _userService.Register(username, password);
                if (result)
                {
                    statusLabel.ForeColor = Color.Green;
                    statusLabel.Text = "Registration successful! You can now login.";
                    passwordTextBox.Clear(); // Clear for security
                }
                else
                {
                    statusLabel.Text = "Username already exists";
                    passwordTextBox.Clear(); // Clear for security
                }
            }
            catch (ArgumentException ex) when (ex.Message.Contains("Password"))
            {
                // Show password policy requirements
                statusLabel.Text = ex.Message;
                passwordTextBox.Clear();
                
                MessageBox.Show(
                    "Password must:\n" +
                    "• Be at least 8 characters long\n" +
                    "• Contain at least one uppercase letter\n" +
                    "• Contain at least one lowercase letter\n" +
                    "• Contain at least one digit\n" +
                    "• Contain at least one special character",
                    "Password Requirements",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
            catch (Exception ex)
            {
                // Generic error message to avoid leaking implementation details
                statusLabel.Text = "Registration failed";
                System.Diagnostics.Debug.WriteLine($"Registration error: {ex.Message}");
                passwordTextBox.Clear(); // Clear for security
            }
            finally
            {
                // Re-enable register button
                registerButton.Enabled = true;
            }
        }
    }
}
