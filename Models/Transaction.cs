using BudgetManager.Models;

namespace BudgetManager.Models
{
    public class Transaction
    {
        public Guid Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public TransactionType Type { get; set; }
        public string Category { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string? Notes { get; set; }

        public Transaction()
        {
            Id = Guid.NewGuid();
            Date = DateTime.Now;
        }

        public Transaction(string description, decimal amount, TransactionType type, string category, string? notes = null)
        {
            Id = Guid.NewGuid();
            Description = description;
            Amount = amount;
            Type = type;
            Category = category;
            Date = DateTime.Now;
            Notes = notes;
        }
    }

    public enum TransactionType
    {
        Income,
        Expense
    }
}
