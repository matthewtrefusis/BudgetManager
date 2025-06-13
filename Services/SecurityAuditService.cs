using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BudgetManager.Services
{
    public class SecurityEvent
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string EventType { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string IPAddress { get; set; } = string.Empty;
        public bool Success { get; set; }
    }

    public class SecurityAuditService
    {
        private readonly string _auditLogFile;
        private readonly EncryptionService _encryptionService;
        private readonly object _fileLock = new object();
        
        public SecurityAuditService()
        {
            var dataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BudgetManager");
            Directory.CreateDirectory(dataDirectory);
            _auditLogFile = Path.Combine(dataDirectory, "security_audit.log");
            _encryptionService = new EncryptionService();
        }
        
        public async Task LogEventAsync(string eventType, string username, string message, bool success = true)
        {
            var securityEvent = new SecurityEvent
            {
                EventType = eventType,
                Username = username,
                Message = message,
                Success = success,
                IPAddress = GetLocalIPAddress()
            };
            
            await AppendToLogAsync(securityEvent);
        }
          private Task AppendToLogAsync(SecurityEvent securityEvent)
        {
            try
            {
                var eventJson = JsonConvert.SerializeObject(securityEvent) + Environment.NewLine;
                
                // Encrypt the event before saving
                var encryptedEvent = _encryptionService.EncryptString(eventJson);
                
                // Using a lock to prevent file access conflicts
                lock (_fileLock)
                {
                    // Append to the file
                    File.AppendAllText(_auditLogFile, encryptedEvent + Environment.NewLine);
                }
            }
            catch (Exception ex)
            {
                // Log to the application's debug log, but don't crash the app
                System.Diagnostics.Debug.WriteLine($"Failed to log security event: {ex.Message}");
            }
            
            return Task.CompletedTask;
        }
        
        public async Task<List<SecurityEvent>> GetRecentEventsAsync(int count = 100)
        {
            var events = new List<SecurityEvent>();
            
            try
            {
                if (!File.Exists(_auditLogFile))
                    return events;
                
                var encryptedLines = await File.ReadAllLinesAsync(_auditLogFile);
                
                // Process the most recent events first
                for (int i = encryptedLines.Length - 1; i >= 0 && events.Count < count; i--)
                {
                    if (!string.IsNullOrEmpty(encryptedLines[i]))
                    {
                        var decryptedLine = _encryptionService.DecryptString(encryptedLines[i]);
                        try
                        {
                            var securityEvent = JsonConvert.DeserializeObject<SecurityEvent>(decryptedLine);
                            if (securityEvent != null)
                            {
                                events.Add(securityEvent);
                            }
                        }
                        catch
                        {
                            // Skip corrupted entries
                            continue;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to read security events: {ex.Message}");
            }
            
            return events;
        }
        
        private string GetLocalIPAddress()
        {
            try
            {
                // Get machine name for local app
                return Environment.MachineName;
            }
            catch
            {
                return "Unknown";
            }
        }
    }
}
