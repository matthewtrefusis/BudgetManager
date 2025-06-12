using BudgetManager.Services;
using BudgetManager.Models;
using Microsoft.VisualBasic;

namespace BudgetManager.UI
{
    public partial class BudgetControl : UserControl
    {
        private readonly IBudgetService _budgetService;
        private DataGridView budgetsGrid = null!;
        private Button addBtn = null!;
        private Button editBtn = null!;
        private Button deleteBtn = null!;

        public BudgetControl(IBudgetService budgetService)
        {
            _budgetService = budgetService;
            InitializeComponent();
            LoadBudgets();
        }

        private void InitializeComponent()
        {
            this.BackColor = Color.White;
            this.Dock = DockStyle.Fill;

            // Create buttons panel
            var buttonPanel = new Panel
            {
                Height = 50,
                Dock = DockStyle.Top,
                BackColor = Color.FromArgb(236, 240, 241)
            };

            addBtn = new Button { Text = "Add Budget", Location = new Point(10, 10), Size = new Size(100, 30) };
            editBtn = new Button { Text = "Edit", Location = new Point(120, 10), Size = new Size(60, 30), Enabled = false };
            deleteBtn = new Button { Text = "Delete", Location = new Point(190, 10), Size = new Size(60, 30), Enabled = false };

            addBtn.Click += AddBudget_Click;
            editBtn.Click += EditBudget_Click;
            deleteBtn.Click += DeleteBudget_Click;

            buttonPanel.Controls.AddRange(new[] { addBtn, editBtn, deleteBtn });

            // Create data grid
            budgetsGrid = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false
            };

            budgetsGrid.Columns.AddRange(new[]
            {
                new DataGridViewTextBoxColumn { Name = "Category", HeaderText = "Category", DataPropertyName = "Category" },
                new DataGridViewTextBoxColumn { Name = "MonthlyLimit", HeaderText = "Monthly Limit", DataPropertyName = "MonthlyLimit", DefaultCellStyle = new DataGridViewCellStyle { Format = "C2" } },
                new DataGridViewTextBoxColumn { Name = "CurrentSpent", HeaderText = "Current Spent", DataPropertyName = "CurrentSpent", DefaultCellStyle = new DataGridViewCellStyle { Format = "C2" } },
                new DataGridViewTextBoxColumn { Name = "RemainingBudget", HeaderText = "Remaining", DataPropertyName = "RemainingBudget", DefaultCellStyle = new DataGridViewCellStyle { Format = "C2" } },
                new DataGridViewTextBoxColumn { Name = "BudgetUtilizationPercentage", HeaderText = "Usage %", DataPropertyName = "BudgetUtilizationPercentage", DefaultCellStyle = new DataGridViewCellStyle { Format = "F1" } }
            });

            budgetsGrid.SelectionChanged += (s, e) =>
            {
                bool hasSelection = budgetsGrid.SelectedRows.Count > 0;
                editBtn.Enabled = hasSelection;
                deleteBtn.Enabled = hasSelection;
            };

            this.Controls.Add(budgetsGrid);
            this.Controls.Add(buttonPanel);
        }

        private async void LoadBudgets()
        {
            try
            {
                var budgets = await _budgetService.GetAllBudgetsAsync();
                budgetsGrid.DataSource = budgets;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading budgets: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void AddBudget_Click(object? sender, EventArgs e)
        {
            try
            {
                var category = Interaction.InputBox("Enter budget category:", "Add Budget", "");
                if (string.IsNullOrEmpty(category)) return;

                var limitStr = Interaction.InputBox("Enter monthly limit:", "Add Budget", "0");
                if (!decimal.TryParse(limitStr, out var limit) || limit <= 0)
                {
                    MessageBox.Show("Please enter a valid positive amount.", "Invalid Amount", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var budget = new Budget(category, category, limit);
                await _budgetService.AddBudgetAsync(budget);
                LoadBudgets();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding budget: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void EditBudget_Click(object? sender, EventArgs e)
        {
            if (budgetsGrid.SelectedRows.Count > 0)
            {
                try
                {
                    var budget = (Budget)budgetsGrid.SelectedRows[0].DataBoundItem;
                    
                    var limitStr = Interaction.InputBox("Enter new monthly limit:", "Edit Budget", budget.MonthlyLimit.ToString());
                    if (!decimal.TryParse(limitStr, out var limit) || limit <= 0)
                    {
                        MessageBox.Show("Please enter a valid positive amount.", "Invalid Amount", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    budget.MonthlyLimit = limit;
                    await _budgetService.UpdateBudgetAsync(budget);
                    LoadBudgets();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error updating budget: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private async void DeleteBudget_Click(object? sender, EventArgs e)
        {
            if (budgetsGrid.SelectedRows.Count > 0)
            {
                var budget = (Budget)budgetsGrid.SelectedRows[0].DataBoundItem;
                var result = MessageBox.Show($"Delete budget for '{budget.Category}'?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                
                if (result == DialogResult.Yes)
                {
                    try
                    {
                        await _budgetService.DeleteBudgetAsync(budget.Id);
                        LoadBudgets();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error deleting budget: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
    }
}
