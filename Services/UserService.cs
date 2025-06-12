using BudgetManager.Models;
using BudgetManager.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BudgetManager.Services
{    public class UserService
    {
    private readonly string _usersFile;
        private readonly string _loginAttemptsFile;
        private readonly string _tempDirectory;
        private readonly string _securityLogFile;
        private List<User> _users;
        private Dictionary<string, LoginAttemptInfo> _loginAttempts;
        private readonly EncryptionService _encryptionService;
        private readonly SecurityAuditService _securityAuditService;
        
        // Security settings
        private const int MaxFailedAttempts = 5;
        private const int LockoutMinutes = 15;
        private const int MinPasswordLength = 8;        public UserService()
        {
            var dataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BudgetManager");
            _tempDirectory = Path.Combine(dataDirectory, "temp");
            Directory.CreateDirectory(dataDirectory);
            Directory.CreateDirectory(_tempDirectory);
            _usersFile = Path.Combine(dataDirectory, "users.json");
            _loginAttemptsFile = Path.Combine(dataDirectory, "login_attempts.json");
            _securityLogFile = Path.Combine(dataDirectory, "security_audit.log");
            _encryptionService = new EncryptionService();
            _securityAuditService = new SecurityAuditService();
            _users = LoadUsers();
            _loginAttempts = LoadLoginAttempts();
        }
          // Security audit logging using the SecurityAuditService
        private async Task LogSecurityEvent(string eventType, string username, string message, bool success)
        {
            try
            {
                await _securityAuditService.LogEventAsync(eventType, username, message, success);
            }
            catch
            {
                // Log silently fails rather than interfering with the main application flow
                System.Diagnostics.Debug.WriteLine("Failed to log security event");
            }
        }private bool IsEncryptedFile(string filePath)
        {
            if (!File.Exists(filePath))
                return false;
                
            try
            {
                var firstFewBytes = File.ReadAllText(filePath, Encoding.UTF8).Take(10).ToArray();
                string start = new string(firstFewBytes);
                return !start.Contains("{") && !start.Contains("[");
            }
            catch
            {
                return false;
            }
        }
        
        private List<User> LoadUsers()
        {
            if (!File.Exists(_usersFile))
                return new List<User>();
            try
            {
                string json;
                if (IsEncryptedFile(_usersFile))
                {
                    var encryptedData = File.ReadAllText(_usersFile);
                    json = _encryptionService.DecryptString(encryptedData);
                }
                else
                {
                    // Handle existing unencrypted files
                    json = File.ReadAllText(_usersFile);
                    // Re-save as encrypted for future use
                    var users = JsonConvert.DeserializeObject<List<User>>(json) ?? new List<User>();
                    SaveUsersEncrypted(users);
                    return users;
                }
                
                return JsonConvert.DeserializeObject<List<User>>(json) ?? new List<User>();
            }
            catch
            {
                return new List<User>();
            }
        }

        private void SaveUsers()
        {
            SaveUsersEncrypted(_users);
        }
        
        private void SaveUsersEncrypted(List<User> users)
        {
            var json = JsonConvert.SerializeObject(users, Formatting.Indented);
            var encryptedData = _encryptionService.EncryptString(json);
            File.WriteAllText(_usersFile, encryptedData);
        }private bool ValidatePasswordStrength(string password)
        {
            if (password.Length < MinPasswordLength)
                return false;
                
            // Check for at least one uppercase letter
            if (!password.Any(char.IsUpper))
                return false;
                
            // Check for at least one lowercase letter
            if (!password.Any(char.IsLower))
                return false;
                
            // Check for at least one digit
            if (!password.Any(char.IsDigit))
                return false;
                
            // Check for at least one special character
            if (!password.Any(c => !char.IsLetterOrDigit(c)))
                return false;
                
            return true;
        }
          public async Task<bool> Register(string username, string password)
        {
            // Input validation
            if (string.IsNullOrWhiteSpace(username))
            {
                await _securityAuditService.LogEventAsync("REGISTRATION", username, "Registration failed: Empty username", false);
                throw new ArgumentException("Username cannot be empty");
            }
                
            if (string.IsNullOrWhiteSpace(password))
            {
                await _securityAuditService.LogEventAsync("REGISTRATION", username, "Registration failed: Empty password", false);
                throw new ArgumentException("Password cannot be empty");
            }
            
            // Validate password strength
            if (!ValidatePasswordStrength(password))
            {
                await _securityAuditService.LogEventAsync("REGISTRATION", username, "Registration failed: Weak password", false);
                throw new ArgumentException("Password must be at least 8 characters long and contain uppercase, lowercase, digit, and special characters");
            }
                
            // Check if username already exists
            if (_users.Any(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase)))
            {
                await _securityAuditService.LogEventAsync("REGISTRATION", username, "Registration failed: Username already exists", false);
                return false;
            }
                
            try
            {
                // Create new user with hashed password
                var user = new User
                {
                    Username = username,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                    CreatedAt = DateTime.UtcNow,
                    LastLogin = DateTime.UtcNow
                };
                _users.Add(user);
                SaveUsers();
                
                await _securityAuditService.LogEventAsync("REGISTRATION", username, "User registered successfully", true);
                return true;
            }
            catch (Exception ex)
            {
                await _securityAuditService.LogEventAsync("REGISTRATION", username, $"Registration error: {ex.Message}", false);
                throw new Exception($"Registration failed: {ex.Message}", ex);
            }
        }        private Dictionary<string, LoginAttemptInfo> LoadLoginAttempts()
        {
            if (!File.Exists(_loginAttemptsFile))
                return new Dictionary<string, LoginAttemptInfo>();
                
            try
            {
                string json;
                if (IsEncryptedFile(_loginAttemptsFile))
                {
                    var encryptedData = File.ReadAllText(_loginAttemptsFile);
                    json = _encryptionService.DecryptString(encryptedData);
                }
                else
                {
                    // For backward compatibility
                    json = File.ReadAllText(_loginAttemptsFile);
                }
                
                var attempts = JsonConvert.DeserializeObject<List<LoginAttemptInfo>>(json) ?? new List<LoginAttemptInfo>();
                // Convert the list to dictionary
                var attemptDict = new Dictionary<string, LoginAttemptInfo>();
                foreach (var attempt in attempts)
                {
                    if (!string.IsNullOrEmpty(attempt.Username))
                        attemptDict[attempt.Username.ToLower()] = attempt;
                }
                return attemptDict;
            }
            catch
            {
                return new Dictionary<string, LoginAttemptInfo>();
            }
        }
        
        private void SaveLoginAttempts()
        {
            var json = JsonConvert.SerializeObject(_loginAttempts.Values.ToList(), Formatting.Indented);
            var encryptedData = _encryptionService.EncryptString(json);
            File.WriteAllText(_loginAttemptsFile, encryptedData);
        }
        
        private void RecordLoginAttempt(string username, bool success)
        {
            if (!_loginAttempts.TryGetValue(username.ToLower(), out var attemptInfo))
            {
                attemptInfo = new LoginAttemptInfo { Username = username.ToLower() };
                _loginAttempts[username.ToLower()] = attemptInfo;
            }
            
            attemptInfo.LastAttemptTime = DateTime.UtcNow;
            
            if (success)
            {
                // Reset failed attempts on successful login
                attemptInfo.FailedAttempts = 0;
                attemptInfo.LockoutEnd = null;
            }
            else
            {
                // Increment failed attempts
                attemptInfo.FailedAttempts++;
                
                // Lock account if too many failed attempts
                if (attemptInfo.FailedAttempts >= MaxFailedAttempts)
                {
                    attemptInfo.LockoutEnd = DateTime.UtcNow.AddMinutes(LockoutMinutes);
                    
                    // Update user's locked status
                    var user = _users.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
                    if (user != null)
                    {
                        user.IsLocked = true;
                        user.LockedUntil = attemptInfo.LockoutEnd;
                        SaveUsers();
                    }
                }
            }
            
            SaveLoginAttempts();
        }
          public async Task<(User? user, string? errorMessage)> Login(string username, string password)
        {
            // Check if account is locked
            if (_loginAttempts.TryGetValue(username.ToLower(), out var attemptInfo) && 
                attemptInfo.LockoutEnd.HasValue && 
                attemptInfo.LockoutEnd.Value > DateTime.UtcNow)
            {
                var remainingMinutes = Math.Ceiling((attemptInfo.LockoutEnd.Value - DateTime.UtcNow).TotalMinutes);
                await _securityAuditService.LogEventAsync("LOGIN_ATTEMPT", username, $"Attempt on locked account", false);
                return (null, $"Account is locked. Try again in {remainingMinutes} minute(s).");
            }
            
            var user = _users.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
            
            if (user == null)
            {
                // Record failed attempt but with generic message
                RecordLoginAttempt(username, false);
                await _securityAuditService.LogEventAsync("LOGIN_ATTEMPT", username, "Invalid username", false);
                return (null, "Invalid username or password");
            }
            
            // Check if the account is locked
            if (user.IsLocked && user.LockedUntil.HasValue && user.LockedUntil.Value > DateTime.UtcNow)
            {
                var remainingMinutes = Math.Ceiling((user.LockedUntil.Value - DateTime.UtcNow).TotalMinutes);
                await _securityAuditService.LogEventAsync("LOGIN_ATTEMPT", username, $"Attempt on locked account", false);
                return (null, $"Account is locked. Try again in {remainingMinutes} minute(s).");
            }
            
            // Verify password
            if (BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                // Update last login time
                user.LastLogin = DateTime.UtcNow;
                
                // If account was locked but lockout period expired, unlock it
                if (user.IsLocked)
                {
                    user.IsLocked = false;
                    user.LockedUntil = null;
                    await _securityAuditService.LogEventAsync("ACCOUNT_UNLOCK", username, "Account automatically unlocked at login", true);
                }
                
                SaveUsers();
                
                // Record successful login
                RecordLoginAttempt(username, true);
                await _securityAuditService.LogEventAsync("LOGIN", username, "Successful login", true);
                
                return (user, null);
            }
            
            // Record failed attempt
            RecordLoginAttempt(username, false);
            await _securityAuditService.LogEventAsync("LOGIN_ATTEMPT", username, "Invalid password", false);
            return (null, "Invalid username or password");
        }

        public async Task<(bool Success, string Message)> ChangePassword(string username, string currentPassword, string newPassword)
        {
            // Validate inputs
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(currentPassword) || string.IsNullOrWhiteSpace(newPassword))
            {
                await LogSecurityEvent("PasswordChange", username, "Password change failed - missing data", false);
                return (false, "Username and passwords are required.");
            }

            // Check if the user exists
            var user = _users.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
            if (user == null)
            {
                await LogSecurityEvent("PasswordChange", username, "Password change failed - user not found", false);
                return (false, "Invalid username or password.");
            }

            // Verify current password
            if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
            {
                await LogSecurityEvent("PasswordChange", username, "Password change failed - incorrect current password", false);
                return (false, "Current password is incorrect.");
            }

            // Validate new password strength
            if (!ValidatePasswordStrength(newPassword))
            {
                await LogSecurityEvent("PasswordChange", username, "Password change failed - new password doesn't meet requirements", false);
                return (false, "New password doesn't meet the strength requirements.\n" +
                              "Password must be at least 8 characters and include uppercase, lowercase, digit, and special character.");
            }

            // Change password
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            SaveUsers();
            await LogSecurityEvent("PasswordChange", username, "Password changed successfully", true);

            return (true, "Password changed successfully.");
        }
    }
}
