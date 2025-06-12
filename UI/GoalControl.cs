using BudgetManager.Services;
using BudgetManager.Models;
using Microsoft.VisualBasic;

namespace BudgetManager.UI
{
    public partial class GoalControl : UserControl
    {
        private readonly IBudgetService _budgetService;
        private FlowLayoutPanel goalsPanel = null!;
        private Button addBtn = null!;

        public GoalControl(IBudgetService budgetService)
        {
            _budgetService = budgetService;
            InitializeComponent();
            LoadGoals();
        }

        private void InitializeComponent()
        {
            this.BackColor = Color.FromArgb(236, 240, 241);
            this.Dock = DockStyle.Fill;

            // Create header panel
            var headerPanel = new Panel
            {
                Height = 50,
                Dock = DockStyle.Top,
                BackColor = Color.White
            };

            addBtn = new Button
            {
                Text = "Add Goal",
                Location = new Point(20, 10),
                Size = new Size(100, 30)
            };
            addBtn.Click += AddGoal_Click;

            headerPanel.Controls.Add(addBtn);

            // Create scrollable goals panel
            goalsPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                Padding = new Padding(20)
            };

            this.Controls.Add(goalsPanel);
            this.Controls.Add(headerPanel);
        }

        private async void LoadGoals()
        {
            try
            {
                goalsPanel.Controls.Clear();
                var goals = await _budgetService.GetAllGoalsAsync();

                if (!goals.Any())
                {
                    var noGoalsLabel = new Label
                    {
                        Text = "No financial goals configured. Click 'Add Goal' to create your first goal.",
                        Font = new Font("Segoe UI", 10),
                        ForeColor = Color.Gray,
                        AutoSize = true
                    };
                    goalsPanel.Controls.Add(noGoalsLabel);
                    return;
                }

                foreach (var goal in goals)
                {
                    var goalCard = CreateGoalCard(goal);
                    goalsPanel.Controls.Add(goalCard);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading goals: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private Panel CreateGoalCard(FinancialGoal goal)
        {
            var card = new Panel
            {
                Size = new Size(goalsPanel.Width - 60, 150),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(0, 0, 0, 10)
            };

            var nameLabel = new Label
            {
                Text = goal.Name,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Location = new Point(15, 15),
                AutoSize = true
            };

            var progressPercentage = (int)((goal.CurrentAmount / goal.TargetAmount) * 100);
            var progressLabel = new Label
            {
                Text = $"${goal.CurrentAmount:N0} / ${goal.TargetAmount:N0} ({progressPercentage}%)",
                Font = new Font("Segoe UI", 10),
                Location = new Point(15, 45),
                AutoSize = true
            };

            var progressBar = new ProgressBar
            {
                Location = new Point(15, 70),
                Size = new Size(card.Width - 150, 20),
                Value = Math.Min(progressPercentage, 100)
            };

            var targetDateLabel = new Label
            {
                Text = $"Target: {goal.TargetDate:MMM dd, yyyy}",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray,
                Location = new Point(15, 100),
                AutoSize = true
            };

            var updateBtn = new Button
            {
                Text = "Update Progress",
                Location = new Point(card.Width - 135, 15),
                Size = new Size(120, 30)
            };
            updateBtn.Click += (s, e) => UpdateGoalProgress(goal);

            card.Controls.AddRange(new Control[] { nameLabel, progressLabel, progressBar, targetDateLabel, updateBtn });
            return card;
        }

        private async void AddGoal_Click(object? sender, EventArgs e)
        {
            try
            {
                var name = Interaction.InputBox("Enter goal name:", "Add Goal", "");
                if (string.IsNullOrEmpty(name)) return;

                var targetStr = Interaction.InputBox("Enter target amount:", "Add Goal", "");
                if (!decimal.TryParse(targetStr, out var target) || target <= 0)
                {
                    MessageBox.Show("Please enter a valid positive amount.", "Invalid Amount", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var goal = new FinancialGoal
                {
                    Name = name,
                    TargetAmount = target,
                    CurrentAmount = 0,
                    TargetDate = DateTime.Now.AddMonths(12),
                    Description = "",
                    IsCompleted = false
                };

                await _budgetService.AddGoalAsync(goal);
                LoadGoals();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding goal: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void UpdateGoalProgress(FinancialGoal goal)
        {
            try
            {
                var amountStr = Interaction.InputBox($"Enter amount to add to '{goal.Name}':", "Update Progress", "");
                if (!decimal.TryParse(amountStr, out var amount) || amount <= 0)
                {
                    MessageBox.Show("Please enter a valid positive amount.", "Invalid Amount", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                await _budgetService.UpdateGoalProgressAsync(goal.Id, amount);
                LoadGoals();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating goal: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
