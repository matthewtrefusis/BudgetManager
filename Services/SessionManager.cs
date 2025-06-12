using BudgetManager.Models;
using System;
using System.Timers;

namespace BudgetManager.Services
{
    public class SessionManager : IDisposable
    {
        // Session timeout in minutes
        private const int DefaultSessionTimeout = 30;
        
        private readonly System.Timers.Timer _sessionTimer;
        private readonly SecurityAuditService _securityAuditService;
        private User? _currentUser;
        
        public event EventHandler? SessionExpired;
        public int SessionTimeoutMinutes { get; set; } = DefaultSessionTimeout;
        public bool IsSessionActive { get; private set; }
        public User? CurrentUser => _currentUser;
        
        public SessionManager(SecurityAuditService securityAuditService)
        {
            _securityAuditService = securityAuditService;
            
            // Create and configure the session timer
            _sessionTimer = new System.Timers.Timer();
            _sessionTimer.Interval = 60 * 1000; // Check every minute
            _sessionTimer.AutoReset = true;
            _sessionTimer.Elapsed += SessionTimerElapsed;
        }
        
        public void StartSession(User user)
        {
            _currentUser = user;
            IsSessionActive = true;
            ResetSessionTimer();
            _sessionTimer.Start();
            _ = _securityAuditService.LogEventAsync("Session", user.Username, "Session started", true);
        }
        
        public void EndSession()
        {
            if (_currentUser != null)
            {
                _ = _securityAuditService.LogEventAsync("Session", _currentUser.Username, "Session ended", true);
            }
            
            _sessionTimer.Stop();
            IsSessionActive = false;
            _currentUser = null;
        }
        
        public void ExtendSession()
        {
            if (IsSessionActive)
            {
                ResetSessionTimer();
            }
        }
        
        private void ResetSessionTimer()
        {
            _lastActivity = DateTime.UtcNow;
        }
        
        private DateTime _lastActivity;
        
        private void SessionTimerElapsed(object? sender, ElapsedEventArgs e)
        {
            if (!IsSessionActive)
                return;
                
            // Check if session has timed out
            if ((DateTime.UtcNow - _lastActivity).TotalMinutes >= SessionTimeoutMinutes)
            {
                _sessionTimer.Stop();
                IsSessionActive = false;
                
                if (_currentUser != null)
                {
                    _ = _securityAuditService.LogEventAsync("Session", _currentUser.Username, 
                        $"Session expired after {SessionTimeoutMinutes} minutes of inactivity", true);
                }
                
                // Trigger the event on a separate thread to avoid timer thread issues
                System.Threading.Tasks.Task.Run(() => SessionExpired?.Invoke(this, EventArgs.Empty));
            }
        }
        
        public void Dispose()
        {
            _sessionTimer.Stop();
            _sessionTimer.Dispose();
        }
    }
}
