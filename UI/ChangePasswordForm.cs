using BudgetManager.Models;
using BudgetManager.Services;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Threading.Tasks;

namespace BudgetManager.UI
{
    public class ChangePasswordForm : Form
    {
        private TextBox currentPasswordBox;
        private TextBox newPasswordBox;
        private TextBox confirmPasswordBox;
        private Button changeButton;
        private Button cancelButton;
        private Label statusLabel;
        private Label passwordStrengthLabel;
        private ProgressBar passwordStrengthBar;
        
        private readonly UserService _userService;
        private readonly User _currentUser;
        private readonly SecurityAuditService _securityAuditService;
        
        public ChangePasswordForm(UserService userService, User currentUser, SecurityAuditService securityAuditService)
        {
            _userService = userService;
            _currentUser = currentUser;
            _securityAuditService = securityAuditService;
            
            InitializeComponent();
        }
        
        private void InitializeComponent()
        {
            // Form settings
            this.Text = "Change Password";
            this.Size = new Size(400, 400);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(236, 240, 241);
            this.Font = new Font("Segoe UI", 9F);
            
            // Layout
            var tableLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20),
                ColumnCount = 2,
                RowCount = 7
            };
            
            // Add column and row styles
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));
            
            // Add controls
            var titleLabel = new Label
            {
                Text = "Change Password",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };
            tableLayout.Controls.Add(titleLabel, 0, 0);
            tableLayout.SetColumnSpan(titleLabel, 2);
            
            // Current password
            tableLayout.Controls.Add(new Label { Text = "Current Password:", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight }, 0, 1);
            currentPasswordBox = new TextBox { Dock = DockStyle.Fill, UseSystemPasswordChar = true };
            tableLayout.Controls.Add(currentPasswordBox, 1, 1);
            
            // New password
            tableLayout.Controls.Add(new Label { Text = "New Password:", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight }, 0, 2);
            newPasswordBox = new TextBox { Dock = DockStyle.Fill, UseSystemPasswordChar = true };
            newPasswordBox.TextChanged += NewPasswordBox_TextChanged;
            tableLayout.Controls.Add(newPasswordBox, 1, 2);
            
            // Password strength
            tableLayout.Controls.Add(new Label { Text = "Password Strength:", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight }, 0, 3);
            var strengthPanel = new Panel { Dock = DockStyle.Fill };
            passwordStrengthBar = new ProgressBar { Width = 150, Height = 15, Maximum = 100, Minimum = 0, Value = 0 };
            passwordStrengthLabel = new Label { Text = "Enter a new password", Top = passwordStrengthBar.Bottom + 5 };
            strengthPanel.Controls.Add(passwordStrengthBar);
            strengthPanel.Controls.Add(passwordStrengthLabel);
            tableLayout.Controls.Add(strengthPanel, 1, 3);
            
            // Confirm password
            tableLayout.Controls.Add(new Label { Text = "Confirm Password:", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight }, 0, 4);
            confirmPasswordBox = new TextBox { Dock = DockStyle.Fill, UseSystemPasswordChar = true };
            tableLayout.Controls.Add(confirmPasswordBox, 1, 4);
            
            // Status label
            statusLabel = new Label
            {
                Text = "Enter your current and new passwords",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.FromArgb(44, 62, 80)
            };
            tableLayout.Controls.Add(statusLabel, 0, 5);
            tableLayout.SetColumnSpan(statusLabel, 2);
            
            // Buttons
            var buttonPanel = new Panel { Dock = DockStyle.Fill };
            changeButton = new Button
            {
                Text = "Change Password",
                Width = 150,
                Height = 35,
                Left = 0,
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            changeButton.FlatAppearance.BorderSize = 0;
            changeButton.Click += ChangeButton_Click;
            
            cancelButton = new Button
            {
                Text = "Cancel",
                Width = 100,
                Height = 35,
                Left = changeButton.Right + 10,
                BackColor = Color.FromArgb(149, 165, 166),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                DialogResult = DialogResult.Cancel
            };
            cancelButton.FlatAppearance.BorderSize = 0;
            
            buttonPanel.Controls.Add(changeButton);
            buttonPanel.Controls.Add(cancelButton);
            tableLayout.Controls.Add(buttonPanel, 0, 6);
            tableLayout.SetColumnSpan(buttonPanel, 2);
            
            this.Controls.Add(tableLayout);
            this.AcceptButton = changeButton;
            this.CancelButton = cancelButton;
        }
        
        private void NewPasswordBox_TextChanged(object? sender, EventArgs e)
        {
            var password = newPasswordBox.Text;
            
            // Calculate password strength
            int strength = CalculatePasswordStrength(password);
            passwordStrengthBar.Value = strength;
            
            if (strength < 40)
            {
                passwordStrengthBar.ForeColor = Color.Red;
                passwordStrengthLabel.Text = "Weak";
                passwordStrengthLabel.ForeColor = Color.Red;
            }
            else if (strength < 75)
            {
                passwordStrengthBar.ForeColor = Color.Yellow;
                passwordStrengthLabel.Text = "Moderate";
                passwordStrengthLabel.ForeColor = Color.Orange;
            }
            else
            {
                passwordStrengthBar.ForeColor = Color.Green;
                passwordStrengthLabel.Text = "Strong";
                passwordStrengthLabel.ForeColor = Color.Green;
            }
        }
        
        private int CalculatePasswordStrength(string password)
        {
            if (string.IsNullOrEmpty(password))
                return 0;
                
            int score = 0;
            
            // Length
            if (password.Length >= 12)
                score += 25;
            else if (password.Length >= 8)
                score += 15;
            else if (password.Length >= 6)
                score += 10;
            
            // Character variety
            if (password.Any(char.IsUpper))
                score += 15;
            if (password.Any(char.IsLower))
                score += 15;
            if (password.Any(char.IsDigit))
                score += 15;
            if (password.Any(c => !char.IsLetterOrDigit(c)))
                score += 20;
                
            // Variety bonus (more than 8 chars with all types)
            if (password.Length >= 8 && 
                password.Any(char.IsUpper) && 
                password.Any(char.IsLower) && 
                password.Any(char.IsDigit) && 
                password.Any(c => !char.IsLetterOrDigit(c)))
            {
                score += 10;
            }
            
            return Math.Min(score, 100);
        }
        
        private async void ChangeButton_Click(object? sender, EventArgs e)
        {
            // Disable button to prevent multiple clicks
            changeButton.Enabled = false;
            statusLabel.Text = "Processing...";
            
            try
            {
                // Validate form
                if (string.IsNullOrWhiteSpace(currentPasswordBox.Text))
                {
                    statusLabel.Text = "Please enter your current password";
                    statusLabel.ForeColor = Color.Red;
                    return;
                }
                
                if (string.IsNullOrWhiteSpace(newPasswordBox.Text))
                {
                    statusLabel.Text = "Please enter a new password";
                    statusLabel.ForeColor = Color.Red;
                    return;
                }
                
                if (newPasswordBox.Text != confirmPasswordBox.Text)
                {
                    statusLabel.Text = "New password and confirmation don't match";
                    statusLabel.ForeColor = Color.Red;
                    return;
                }
                
                // Check if new password is the same as old
                if (newPasswordBox.Text == currentPasswordBox.Text)
                {
                    statusLabel.Text = "New password must be different from current password";
                    statusLabel.ForeColor = Color.Red;
                    return;
                }
                
                // Call service to change password
                var result = await _userService.ChangePassword(
                    _currentUser.Username,
                    currentPasswordBox.Text,
                    newPasswordBox.Text);
                
                if (result.Success)
                {
                    statusLabel.Text = "Password changed successfully";
                    statusLabel.ForeColor = Color.Green;
                    await Task.Delay(1500);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    statusLabel.Text = result.Message;
                    statusLabel.ForeColor = Color.Red;
                    
                    // Clear password fields on failure
                    currentPasswordBox.Text = string.Empty;
                    newPasswordBox.Text = string.Empty;
                    confirmPasswordBox.Text = string.Empty;
                }
            }
            finally
            {
                changeButton.Enabled = true;
            }
        }
    }
}
