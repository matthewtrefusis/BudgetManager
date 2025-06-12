using BudgetManager.Services;
using BudgetManager.Models;
using System;
using System.Windows.Forms;
using System.Drawing;

namespace BudgetManager.UI
{    public partial class MainForm : Form
    {        private readonly IBudgetService _budgetService;
        private readonly ReportService _reportService;
        private readonly BudgetManager.Services.SessionManager? _sessionManager;
        
        // UI Components
        private TableLayoutPanel mainLayout;
        private Panel headerPanel;
        private Panel contentPanel;
        private Panel sidebarPanel;
        private Label titleLabel;
        
        // Navigation buttons
        private Button transactionsBtn;
        private Button budgetsBtn;
        private Button goalsBtn;
        private Button reportsBtn;
        private Button dashboardBtn;
        private Button exitBtn;
        
        // Current active control
        private UserControl? currentControl;        public MainForm(IBudgetService budgetService, ReportService reportService, User? currentUser = null, BudgetManager.Services.SessionManager? sessionManager = null)
        {
            _budgetService = budgetService;
            _reportService = reportService;
            _sessionManager = sessionManager;
            if (currentUser != null && _budgetService is BudgetService bs)
                bs.SetCurrentUser(currentUser);
                
            InitializeComponent();
            
            // Set up session activity monitoring
            if (_sessionManager != null)
            {
                this.MouseMove += (s, e) => _sessionManager.ExtendSession();
                this.KeyDown += (s, e) => _sessionManager.ExtendSession();
                this.Click += (s, e) => _sessionManager.ExtendSession();
            }
            
            ShowDashboard(); // Start with dashboard
        }        private void InitializeComponent()
        {
            this.Text = "Budget Manager - Personal Finance Assistant";
            this.Size = new Size(1200, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(1000, 600);

            // Create menu
            CreateMainMenu();
            
            CreateMainLayout();
            CreateHeader();
            CreateSidebar();
            CreateContentArea();
        }
        
        private void CreateMainMenu()
        {
            var mainMenu = new MenuStrip();
            this.MainMenuStrip = mainMenu;
            
            // File menu
            var fileMenu = new ToolStripMenuItem("File");
            var exitMenuItem = new ToolStripMenuItem("Exit", null, (s, e) => Application.Exit());
            fileMenu.DropDownItems.Add(exitMenuItem);
            
            // Account menu
            var accountMenu = new ToolStripMenuItem("Account");
            var changePasswordMenuItem = new ToolStripMenuItem("Change Password", null, ChangePassword_Click);
            accountMenu.DropDownItems.Add(changePasswordMenuItem);
            
            // Help menu
            var helpMenu = new ToolStripMenuItem("Help");
            var aboutMenuItem = new ToolStripMenuItem("About", null, (s, e) => 
                MessageBox.Show("Budget Manager - Personal Finance Application\nVersion 1.0", "About", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information));
            helpMenu.DropDownItems.Add(aboutMenuItem);
            
            mainMenu.Items.Add(fileMenu);
            mainMenu.Items.Add(accountMenu);
            mainMenu.Items.Add(helpMenu);
            
            this.Controls.Add(mainMenu);
        }
          private void ChangePassword_Click(object? sender, EventArgs e)
        {
            // Get the current user from the session manager
            var currentUser = _sessionManager?.CurrentUser;
            if (currentUser == null)
                return;
                
            var userService = new UserService();
            var securityAuditService = new SecurityAuditService();
            
            using (var changePasswordForm = new ChangePasswordForm(userService, currentUser, securityAuditService))
            {
                if (changePasswordForm.ShowDialog(this) == DialogResult.OK)
                {
                    MessageBox.Show("Your password has been changed successfully.", "Password Changed", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void CreateMainLayout()
        {
            mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 2
            };

            // Configure columns: sidebar (200px), content (fill)
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 200));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            // Configure rows: header (60px), content (fill)
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            this.Controls.Add(mainLayout);
        }

        private void CreateHeader()
        {
            headerPanel = new Panel
            {
                BackColor = Color.FromArgb(52, 73, 94),
                Dock = DockStyle.Fill
            };

            titleLabel = new Label
            {
                Text = "Budget Manager",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(20, 15)
            };

            headerPanel.Controls.Add(titleLabel);
            mainLayout.Controls.Add(headerPanel, 0, 0);
            mainLayout.SetColumnSpan(headerPanel, 2);
        }

        private void CreateSidebar()
        {
            sidebarPanel = new Panel
            {
                BackColor = Color.FromArgb(44, 62, 80),
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };

            CreateNavigationButtons();
            mainLayout.Controls.Add(sidebarPanel, 0, 1);
        }

        private void CreateNavigationButtons()
        {
            var buttonStyle = new Action<Button>((btn) =>
            {
                btn.Size = new Size(180, 45);
                btn.FlatStyle = FlatStyle.Flat;
                btn.FlatAppearance.BorderSize = 0;
                btn.Font = new Font("Segoe UI", 10, FontStyle.Bold);
                btn.ForeColor = Color.White;
                btn.BackColor = Color.FromArgb(52, 73, 94);
                btn.TextAlign = ContentAlignment.MiddleLeft;
                btn.Padding = new Padding(20, 0, 0, 0);
                btn.Cursor = Cursors.Hand;

                btn.MouseEnter += (s, e) => btn.BackColor = Color.FromArgb(41, 128, 185);
                btn.MouseLeave += (s, e) => 
                {
                    if (btn != GetActiveButton())
                        btn.BackColor = Color.FromArgb(52, 73, 94);
                };
            });

            dashboardBtn = new Button { Text = "📊 Dashboard", Location = new Point(10, 20) };
            buttonStyle(dashboardBtn);
            dashboardBtn.Click += (s, e) => ShowDashboard();

            transactionsBtn = new Button { Text = "💳 Transactions", Location = new Point(10, 75) };
            buttonStyle(transactionsBtn);
            transactionsBtn.Click += (s, e) => ShowTransactions();

            budgetsBtn = new Button { Text = "💰 Budgets", Location = new Point(10, 130) };
            buttonStyle(budgetsBtn);
            budgetsBtn.Click += (s, e) => ShowBudgets();

            goalsBtn = new Button { Text = "🎯 Goals", Location = new Point(10, 185) };
            buttonStyle(goalsBtn);
            goalsBtn.Click += (s, e) => ShowGoals();

            reportsBtn = new Button { Text = "📈 Reports", Location = new Point(10, 240) };
            buttonStyle(reportsBtn);
            reportsBtn.Click += (s, e) => ShowReports();

            exitBtn = new Button { Text = "🚪 Exit", Location = new Point(10, 350) };
            buttonStyle(exitBtn);
            exitBtn.Click += (s, e) => this.Close();

            sidebarPanel.Controls.AddRange(new[] { dashboardBtn, transactionsBtn, budgetsBtn, goalsBtn, reportsBtn, exitBtn });
        }

        private void CreateContentArea()
        {
            contentPanel = new Panel
            {
                BackColor = Color.FromArgb(236, 240, 241),
                Dock = DockStyle.Fill,
                Padding = new Padding(20)
            };

            mainLayout.Controls.Add(contentPanel, 1, 1);
        }

        private Button? GetActiveButton()
        {
            return sidebarPanel.Controls.OfType<Button>().FirstOrDefault(b => b.BackColor == Color.FromArgb(41, 128, 185));
        }

        private void SetActiveButton(Button button)
        {
            // Reset all buttons
            foreach (Button btn in sidebarPanel.Controls.OfType<Button>())
            {
                btn.BackColor = Color.FromArgb(52, 73, 94);
            }
            // Set active button
            button.BackColor = Color.FromArgb(41, 128, 185);
        }

        private void ShowControl(UserControl control, Button activeButton)
        {
            currentControl?.Dispose();
            contentPanel.Controls.Clear();
            
            control.Dock = DockStyle.Fill;
            contentPanel.Controls.Add(control);
            currentControl = control;
            
            SetActiveButton(activeButton);
        }

        private void ShowDashboard()
        {
            var dashboardControl = new DashboardControl(_budgetService);
            ShowControl(dashboardControl, dashboardBtn);
        }

        private void ShowTransactions()
        {
            var transactionControl = new TransactionControl(_budgetService);
            ShowControl(transactionControl, transactionsBtn);
        }

        private void ShowBudgets()
        {
            var budgetControl = new BudgetControl(_budgetService);
            ShowControl(budgetControl, budgetsBtn);
        }

        private void ShowGoals()
        {
            var goalControl = new GoalControl(_budgetService);
            ShowControl(goalControl, goalsBtn);
        }

        private void ShowReports()
        {
            var reportControl = new ReportControl(_reportService);
            ShowControl(reportControl, reportsBtn);
        }
    }
}
