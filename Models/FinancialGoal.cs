namespace BudgetManager.Models
{
    public class FinancialGoal
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal TargetAmount { get; set; }
        public decimal CurrentAmount { get; set; }
        public DateTime TargetDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsCompleted { get; set; }

        public FinancialGoal()
        {
            Id = Guid.NewGuid();
            CreatedDate = DateTime.Now;
            IsCompleted = false;
            CurrentAmount = 0;
        }

        public FinancialGoal(string name, string description, decimal targetAmount, DateTime targetDate)
        {
            Id = Guid.NewGuid();
            Name = name;
            Description = description;
            TargetAmount = targetAmount;
            TargetDate = targetDate;
            CreatedDate = DateTime.Now;
            IsCompleted = false;
            CurrentAmount = 0;
        }

        public decimal RemainingAmount => TargetAmount - CurrentAmount;
        public decimal ProgressPercentage => TargetAmount > 0 ? (CurrentAmount / TargetAmount) * 100 : 0;
        public int DaysRemaining => (TargetDate - DateTime.Now).Days;
        public decimal RequiredMonthlySavings
        {
            get
            {
                var monthsRemaining = Math.Max(1, DaysRemaining / 30.0);
                return RemainingAmount / (decimal)monthsRemaining;
            }
        }
    }
}
