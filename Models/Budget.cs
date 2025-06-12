namespace BudgetManager.Models
{
    public class Budget
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal MonthlyLimit { get; set; }
        public decimal CurrentSpent { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsActive { get; set; }

        public Budget()
        {
            Id = Guid.NewGuid();
            CreatedDate = DateTime.Now;
            IsActive = true;
            CurrentSpent = 0;
        }

        public Budget(string name, string category, decimal monthlyLimit)
        {
            Id = Guid.NewGuid();
            Name = name;
            Category = category;
            MonthlyLimit = monthlyLimit;
            CreatedDate = DateTime.Now;
            IsActive = true;
            CurrentSpent = 0;
        }

        public decimal RemainingBudget => MonthlyLimit - CurrentSpent;
        public decimal BudgetUtilizationPercentage => MonthlyLimit > 0 ? (CurrentSpent / MonthlyLimit) * 100 : 0;
        public bool IsOverBudget => CurrentSpent > MonthlyLimit;
    }
}
