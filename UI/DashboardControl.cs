using BudgetManager.Services;
using BudgetManager.Models;

namespace BudgetManager.UI
{
    public partial class DashboardControl : UserControl
    {
        private readonly IBudgetService _budgetService;
          // UI Components
        private Panel summaryPanel = null!;
        private Panel budgetPanel = null!;
        private Panel goalsPanel = null!;
        private Panel transactionsPanel = null!;

        public DashboardControl(IBudgetService budgetService)
        {
            _budgetService = budgetService;
            InitializeComponent();
            LoadDashboardData();
        }

        private void InitializeComponent()
        {
            this.BackColor = Color.FromArgb(236, 240, 241);
            this.Dock = DockStyle.Fill;

            CreateDashboardPanels();
        }

        private void CreateDashboardPanels()
        {
            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 2,
                Padding = new Padding(10)
            };

            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50));

            // Create panels
            summaryPanel = CreatePanel("💰 Monthly Summary", 0, 0);
            budgetPanel = CreatePanel("📊 Budget Status", 1, 0);
            goalsPanel = CreatePanel("🎯 Goals Progress", 0, 1);
            transactionsPanel = CreatePanel("📝 Recent Transactions", 1, 1);

            mainLayout.Controls.Add(summaryPanel, 0, 0);
            mainLayout.Controls.Add(budgetPanel, 1, 0);
            mainLayout.Controls.Add(goalsPanel, 0, 1);
            mainLayout.Controls.Add(transactionsPanel, 1, 1);

            this.Controls.Add(mainLayout);
        }

        private Panel CreatePanel(string title, int col, int row)
        {
            var panel = new Panel
            {
                BackColor = Color.White,
                Margin = new Padding(5),
                Dock = DockStyle.Fill
            };

            var titleLabel = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 73, 94),
                Location = new Point(15, 15),
                AutoSize = true
            };

            panel.Controls.Add(titleLabel);
            return panel;
        }

        private async void LoadDashboardData()
        {
            await LoadMonthlySummary();
            await LoadBudgetStatus();
            await LoadGoalsProgress();
            await LoadRecentTransactions();
        }

        private async Task LoadMonthlySummary()
        {
            var startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var income = await _budgetService.GetTotalIncomeAsync(startDate, endDate);
            var expenses = await _budgetService.GetTotalExpensesAsync(startDate, endDate);
            var netIncome = income - expenses;

            var yPos = 50;
            
            AddSummaryItem(summaryPanel, "Total Income:", $"${income:N2}", Color.FromArgb(46, 204, 113), yPos);
            AddSummaryItem(summaryPanel, "Total Expenses:", $"${Math.Abs(expenses):N2}", Color.FromArgb(231, 76, 60), yPos + 40);
            AddSummaryItem(summaryPanel, "Net Income:", $"${netIncome:N2}", 
                netIncome >= 0 ? Color.FromArgb(46, 204, 113) : Color.FromArgb(231, 76, 60), yPos + 80);
        }

        private void AddSummaryItem(Panel panel, string label, string value, Color valueColor, int yPos)
        {
            var labelControl = new Label
            {
                Text = label,
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(127, 140, 141),
                Location = new Point(15, yPos),
                AutoSize = true
            };

            var valueControl = new Label
            {
                Text = value,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = valueColor,
                Location = new Point(150, yPos),
                AutoSize = true
            };

            panel.Controls.Add(labelControl);
            panel.Controls.Add(valueControl);
        }

        private async Task LoadBudgetStatus()
        {
            var titleLabel = budgetPanel.Controls[0];
            budgetPanel.Controls.Clear();
            budgetPanel.Controls.Add(titleLabel);

            var budgets = await _budgetService.GetAllBudgetsAsync();
            
            if (!budgets.Any())
            {
                var noBudgetsLabel = new Label
                {
                    Text = "No budgets configured",
                    Font = new Font("Segoe UI", 9),
                    ForeColor = Color.Gray,
                    Location = new Point(15, 50),
                    AutoSize = true
                };
                budgetPanel.Controls.Add(noBudgetsLabel);
                return;
            }

            var yPos = 50;
            foreach (var budget in budgets.Take(4))
            {
                var usagePercentage = budget.BudgetUtilizationPercentage;
                var statusColor = budget.IsOverBudget ? Color.FromArgb(231, 76, 60) :
                                usagePercentage > 80 ? Color.FromArgb(243, 156, 18) :
                                Color.FromArgb(46, 204, 113);

                var budgetLabel = new Label
                {
                    Text = $"{budget.Category}: ${budget.CurrentSpent:N0}/${budget.MonthlyLimit:N0}",
                    Font = new Font("Segoe UI", 9),
                    ForeColor = Color.FromArgb(52, 73, 94),
                    Location = new Point(15, yPos),
                    AutoSize = true
                };

                var progressBar = new ProgressBar
                {
                    Location = new Point(15, yPos + 20),
                    Size = new Size(200, 10),
                    Value = Math.Min((int)usagePercentage, 100),
                    ForeColor = statusColor
                };

                budgetPanel.Controls.Add(budgetLabel);
                budgetPanel.Controls.Add(progressBar);
                yPos += 45;
            }
        }

        private async Task LoadGoalsProgress()
        {
            var titleLabel = goalsPanel.Controls[0];
            goalsPanel.Controls.Clear();
            goalsPanel.Controls.Add(titleLabel);

            var goals = await _budgetService.GetAllGoalsAsync();
            
            if (!goals.Any())
            {
                var noGoalsLabel = new Label
                {
                    Text = "No goals configured",
                    Font = new Font("Segoe UI", 9),
                    ForeColor = Color.Gray,
                    Location = new Point(15, 50),
                    AutoSize = true
                };
                goalsPanel.Controls.Add(noGoalsLabel);
                return;
            }

            var yPos = 50;
            foreach (var goal in goals.Take(4))
            {
                var progressPercentage = (int)((goal.CurrentAmount / goal.TargetAmount) * 100);
                
                var goalLabel = new Label
                {
                    Text = $"{goal.Name}: ${goal.CurrentAmount:N0}/${goal.TargetAmount:N0}",
                    Font = new Font("Segoe UI", 9),
                    ForeColor = Color.FromArgb(52, 73, 94),
                    Location = new Point(15, yPos),
                    AutoSize = true
                };

                var progressBar = new ProgressBar
                {
                    Location = new Point(15, yPos + 20),
                    Size = new Size(200, 10),
                    Value = Math.Min(progressPercentage, 100)
                };

                goalsPanel.Controls.Add(goalLabel);
                goalsPanel.Controls.Add(progressBar);
                yPos += 45;
            }
        }

        private async Task LoadRecentTransactions()
        {
            var titleLabel = transactionsPanel.Controls[0];
            transactionsPanel.Controls.Clear();
            transactionsPanel.Controls.Add(titleLabel);

            var transactions = await _budgetService.GetAllTransactionsAsync();
            
            if (!transactions.Any())
            {
                var noTransactionsLabel = new Label
                {
                    Text = "No transactions recorded",
                    Font = new Font("Segoe UI", 9),
                    ForeColor = Color.Gray,
                    Location = new Point(15, 50),
                    AutoSize = true
                };
                transactionsPanel.Controls.Add(noTransactionsLabel);
                return;
            }

            var yPos = 50;
            foreach (var transaction in transactions.Take(6))
            {
                var transactionRow = CreateTransactionRow(transaction, yPos);
                transactionsPanel.Controls.Add(transactionRow);
                yPos += 30;
            }
        }

        private Panel CreateTransactionRow(Transaction transaction, int yPos)
        {
            var rowPanel = new Panel
            {
                Location = new Point(15, yPos),
                Size = new Size(250, 25)
            };

            var descLabel = new Label
            {
                Text = transaction.Description,
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(52, 73, 94),
                Location = new Point(0, 0),
                Size = new Size(120, 20)
            };

            var amountLabel = new Label
            {
                Text = $"${Math.Abs(transaction.Amount):N0}",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = transaction.Type == TransactionType.Income ? 
                    Color.FromArgb(46, 204, 113) : Color.FromArgb(231, 76, 60),
                Location = new Point(125, 0),
                Size = new Size(80, 20),
                TextAlign = ContentAlignment.MiddleRight
            };

            var dateLabel = new Label
            {
                Text = transaction.Date.ToString("MM/dd"),
                Font = new Font("Segoe UI", 8),
                ForeColor = Color.Gray,
                Location = new Point(210, 0),
                Size = new Size(40, 20)
            };

            rowPanel.Controls.Add(descLabel);
            rowPanel.Controls.Add(amountLabel);
            rowPanel.Controls.Add(dateLabel);

            return rowPanel;
        }
    }
}
