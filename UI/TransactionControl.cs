using BudgetManager.Services;
using BudgetManager.Models;

namespace BudgetManager.UI
{
    public partial class TransactionControl : UserControl
    {
        private readonly IBudgetService _budgetService;
        private DataGridView transactionsGrid;
        private Button addBtn, editBtn, deleteBtn, refreshBtn;

        public TransactionControl(IBudgetService budgetService)
        {
            _budgetService = budgetService;
            InitializeComponent();
            LoadTransactions();
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

            addBtn = new Button { Text = "Add Transaction", Location = new Point(10, 10), Size = new Size(120, 30) };
            editBtn = new Button { Text = "Edit", Location = new Point(140, 10), Size = new Size(60, 30), Enabled = false };
            deleteBtn = new Button { Text = "Delete", Location = new Point(210, 10), Size = new Size(60, 30), Enabled = false };
            refreshBtn = new Button { Text = "Refresh", Location = new Point(280, 10), Size = new Size(60, 30) };

            addBtn.Click += AddTransaction_Click;
            editBtn.Click += EditTransaction_Click;
            deleteBtn.Click += DeleteTransaction_Click;
            refreshBtn.Click += (s, e) => LoadTransactions();

            buttonPanel.Controls.AddRange(new[] { addBtn, editBtn, deleteBtn, refreshBtn });

            // Create data grid
            transactionsGrid = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false
            };

            transactionsGrid.Columns.AddRange(new[]
            {
                new DataGridViewTextBoxColumn { Name = "Date", HeaderText = "Date", DataPropertyName = "Date", DefaultCellStyle = new DataGridViewCellStyle { Format = "MM/dd/yyyy" } },
                new DataGridViewTextBoxColumn { Name = "Description", HeaderText = "Description", DataPropertyName = "Description" },
                new DataGridViewTextBoxColumn { Name = "Category", HeaderText = "Category", DataPropertyName = "Category" },
                new DataGridViewTextBoxColumn { Name = "Type", HeaderText = "Type", DataPropertyName = "Type" },
                new DataGridViewTextBoxColumn { Name = "Amount", HeaderText = "Amount", DataPropertyName = "Amount", DefaultCellStyle = new DataGridViewCellStyle { Format = "C2" } }
            });

            transactionsGrid.SelectionChanged += (s, e) =>
            {
                bool hasSelection = transactionsGrid.SelectedRows.Count > 0;
                editBtn.Enabled = hasSelection;
                deleteBtn.Enabled = hasSelection;
            };

            this.Controls.Add(transactionsGrid);
            this.Controls.Add(buttonPanel);
        }

        private async void LoadTransactions()
        {
            try
            {
                var transactions = await _budgetService.GetAllTransactionsAsync();
                transactionsGrid.DataSource = transactions;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading transactions: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AddTransaction_Click(object? sender, EventArgs e)
        {
            var form = new TransactionForm(_budgetService);
            if (form.ShowDialog() == DialogResult.OK)
            {
                LoadTransactions();
            }
        }

        private void EditTransaction_Click(object? sender, EventArgs e)
        {
            if (transactionsGrid.SelectedRows.Count > 0)
            {
                var transaction = (Transaction)transactionsGrid.SelectedRows[0].DataBoundItem;
                var form = new TransactionForm(_budgetService, transaction);
                if (form.ShowDialog() == DialogResult.OK)
                {
                    LoadTransactions();
                }
            }
        }

        private async void DeleteTransaction_Click(object? sender, EventArgs e)
        {
            if (transactionsGrid.SelectedRows.Count > 0)
            {
                var transaction = (Transaction)transactionsGrid.SelectedRows[0].DataBoundItem;
                var result = MessageBox.Show($"Delete transaction '{transaction.Description}'?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                
                if (result == DialogResult.Yes)
                {
                    try
                    {
                        await _budgetService.DeleteTransactionAsync(transaction.Id);
                        LoadTransactions();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error deleting transaction: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
    }
}
