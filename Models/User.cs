using System;
using System.Collections.Generic;

namespace BudgetManager.Models
{
    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastLogin { get; set; }
        public bool IsLocked { get; set; } = false;
        public DateTime? LockedUntil { get; set; }
        public List<Transaction> Transactions { get; set; } = new();
        public List<Budget> Budgets { get; set; } = new();
        public List<FinancialGoal> Goals { get; set; } = new();
    }
    
    public class LoginAttemptInfo
    {
        public string Username { get; set; } = string.Empty;
        public int FailedAttempts { get; set; }
        public DateTime LastAttemptTime { get; set; }
        public DateTime? LockoutEnd { get; set; }
    }
}
