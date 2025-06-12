using BudgetManager.Services;
using BudgetManager.Models;

namespace BudgetManager.UI
{
    public partial class TransactionForm : Form
    {
        private readonly IBudgetService _budgetService;
        private readonly Transaction? _existingTransaction;
        
        private TextBox descriptionTextBox = null!;
        private NumericUpDown amountNumeric = null!;
        private ComboBox typeComboBox = null!;
        private ComboBox categoryComboBox = null!;
        private TextBox notesTextBox = null!;
        private DateTimePicker datePicker = null!;
        private Button saveBtn = null!;
        private Button cancelBtn = null!;

        public TransactionForm(IBudgetService budgetService, Transaction? existingTransaction = null)
        {
            _budgetService = budgetService;
            _existingTransaction = existingTransaction;
            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = _existingTransaction == null ? "Add Transaction" : "Edit Transaction";
            this.Size = new Size(400, 350);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 7,
                Padding = new Padding(20)
            };

            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            // Description
            layout.Controls.Add(new Label { Text = "Description:", TextAlign = ContentAlignment.MiddleLeft }, 0, 0);
            descriptionTextBox = new TextBox { Dock = DockStyle.Fill };
            layout.Controls.Add(descriptionTextBox, 1, 0);

            // Amount
            layout.Controls.Add(new Label { Text = "Amount:", TextAlign = ContentAlignment.MiddleLeft }, 0, 1);
            amountNumeric = new NumericUpDown { Dock = DockStyle.Fill, DecimalPlaces = 2, Maximum = 999999, Minimum = 0.01m };
            layout.Controls.Add(amountNumeric, 1, 1);

            // Type
            layout.Controls.Add(new Label { Text = "Type:", TextAlign = ContentAlignment.MiddleLeft }, 0, 2);
            typeComboBox = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            typeComboBox.Items.AddRange(new[] { "Income", "Expense" });
            layout.Controls.Add(typeComboBox, 1, 2);

            // Category
            layout.Controls.Add(new Label { Text = "Category:", TextAlign = ContentAlignment.MiddleLeft }, 0, 3);
            categoryComboBox = new ComboBox { Dock = DockStyle.Fill };
            layout.Controls.Add(categoryComboBox, 1, 3);

            // Date
            layout.Controls.Add(new Label { Text = "Date:", TextAlign = ContentAlignment.MiddleLeft }, 0, 4);
            datePicker = new DateTimePicker { Dock = DockStyle.Fill, Format = DateTimePickerFormat.Short };
            layout.Controls.Add(datePicker, 1, 4);

            // Notes
            layout.Controls.Add(new Label { Text = "Notes:", TextAlign = ContentAlignment.MiddleLeft }, 0, 5);
            notesTextBox = new TextBox { Dock = DockStyle.Fill, Multiline = true };
            layout.Controls.Add(notesTextBox, 1, 5);

            // Buttons
            var buttonPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.RightToLeft,
                Dock = DockStyle.Fill
            };

            cancelBtn = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel };
            saveBtn = new Button { Text = "Save", DialogResult = DialogResult.None };
            saveBtn.Click += SaveBtn_Click;

            buttonPanel.Controls.Add(cancelBtn);
            buttonPanel.Controls.Add(saveBtn);
            
            layout.Controls.Add(buttonPanel, 0, 6);
            layout.SetColumnSpan(buttonPanel, 2);

            this.Controls.Add(layout);
            this.AcceptButton = saveBtn;
            this.CancelButton = cancelBtn;
        }

        private async void LoadData()
        {
            try
            {
                // Load categories
                var transactions = await _budgetService.GetAllTransactionsAsync();
                var categories = transactions.Select(t => t.Category).Distinct().OrderBy(c => c).ToList();
                
                if (!categories.Any())
                {
                    categories.AddRange(new[] { "Salary", "Freelance", "Investment", "Bonus", "Groceries", "Transportation", "Utilities", "Entertainment", "Healthcare", "Shopping", "Dining", "Education" });
                }

                categoryComboBox.Items.AddRange(categories.ToArray());

                // Populate form if editing
                if (_existingTransaction != null)
                {
                    descriptionTextBox.Text = _existingTransaction.Description;
                    amountNumeric.Value = _existingTransaction.Amount;
                    typeComboBox.SelectedItem = _existingTransaction.Type.ToString();
                    categoryComboBox.Text = _existingTransaction.Category;
                    datePicker.Value = _existingTransaction.Date;
                    notesTextBox.Text = _existingTransaction.Notes ?? "";
                }
                else
                {
                    typeComboBox.SelectedIndex = 1; // Default to Expense
                    datePicker.Value = DateTime.Now;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void SaveBtn_Click(object? sender, EventArgs e)
        {
            if (!ValidateForm()) return;

            try
            {
                var transactionType = typeComboBox.SelectedItem?.ToString() == "Income" ? TransactionType.Income : TransactionType.Expense;

                if (_existingTransaction == null)
                {
                    // Create new transaction
                    var transaction = new Transaction(
                        descriptionTextBox.Text.Trim(),
                        amountNumeric.Value,
                        transactionType,
                        categoryComboBox.Text.Trim(),
                        string.IsNullOrWhiteSpace(notesTextBox.Text) ? null : notesTextBox.Text.Trim()
                    );
                    transaction.Date = datePicker.Value;

                    await _budgetService.AddTransactionAsync(transaction);
                }
                else
                {
                    // Update existing transaction
                    _existingTransaction.Description = descriptionTextBox.Text.Trim();
                    _existingTransaction.Amount = amountNumeric.Value;
                    _existingTransaction.Type = transactionType;
                    _existingTransaction.Category = categoryComboBox.Text.Trim();
                    _existingTransaction.Date = datePicker.Value;
                    _existingTransaction.Notes = string.IsNullOrWhiteSpace(notesTextBox.Text) ? null : notesTextBox.Text.Trim();

                    await _budgetService.UpdateTransactionAsync(_existingTransaction);
                }

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving transaction: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ValidateForm()
        {
            if (string.IsNullOrWhiteSpace(descriptionTextBox.Text))
            {
                MessageBox.Show("Description is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                descriptionTextBox.Focus();
                return false;
            }

            if (amountNumeric.Value <= 0)
            {
                MessageBox.Show("Amount must be greater than zero.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                amountNumeric.Focus();
                return false;
            }

            if (typeComboBox.SelectedItem == null)
            {
                MessageBox.Show("Please select a transaction type.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                typeComboBox.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(categoryComboBox.Text))
            {
                MessageBox.Show("Category is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                categoryComboBox.Focus();
                return false;
            }

            return true;
        }
    }
}
