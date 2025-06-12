using BudgetManager.Services;

namespace BudgetManager.UI
{
    public partial class ReportControl : UserControl
    {
        private readonly ReportService _reportService;
        private Panel buttonPanel = null!;
        private RichTextBox reportTextBox = null!;

        public ReportControl(ReportService reportService)
        {
            _reportService = reportService;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.BackColor = Color.White;
            this.Dock = DockStyle.Fill;

            // Create button panel
            buttonPanel = new Panel
            {
                Height = 60,
                Dock = DockStyle.Top,
                BackColor = Color.FromArgb(236, 240, 241),
                Padding = new Padding(10)
            };

            var monthlyBtn = new Button { Text = "Monthly Report", Location = new Point(10, 15), Size = new Size(120, 30) };
            var budgetBtn = new Button { Text = "Budget Performance", Location = new Point(140, 15), Size = new Size(140, 30) };
            var goalsBtn = new Button { Text = "Goals Progress", Location = new Point(290, 15), Size = new Size(120, 30) };
            var yearlyBtn = new Button { Text = "Yearly Report", Location = new Point(420, 15), Size = new Size(120, 30) };

            monthlyBtn.Click += async (s, e) => await ShowReport(_reportService.GenerateMonthlyReportAsync);
            budgetBtn.Click += async (s, e) => await ShowReport(_reportService.GenerateBudgetPerformanceReportAsync);
            goalsBtn.Click += async (s, e) => await ShowReport(_reportService.GenerateGoalProgressReportAsync);
            yearlyBtn.Click += async (s, e) => await ShowReport(_reportService.GenerateYearlyReportAsync);

            buttonPanel.Controls.AddRange(new[] { monthlyBtn, budgetBtn, goalsBtn, yearlyBtn });

            // Create report display area
            reportTextBox = new RichTextBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Consolas", 10),
                ReadOnly = true,
                BackColor = Color.White,
                BorderStyle = BorderStyle.None,
                Padding = new Padding(20)
            };

            this.Controls.Add(reportTextBox);
            this.Controls.Add(buttonPanel);

            // Show welcome message
            reportTextBox.Text = "Welcome to Reports!\n\nSelect a report type from the buttons above to view detailed financial information.";
        }

        private async Task ShowReport(Func<Task<string>> reportGenerator)
        {
            try
            {
                reportTextBox.Text = "Generating report...";
                reportTextBox.Refresh();

                var report = await reportGenerator();
                reportTextBox.Text = report;
            }
            catch (Exception ex)
            {
                reportTextBox.Text = $"Error generating report:\n{ex.Message}";
            }
        }
    }
}
