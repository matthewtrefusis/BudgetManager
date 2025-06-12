using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace BudgetManager.Services
{
    public class EncryptionService
    {
        private static readonly byte[] Salt = new byte[] { 0x43, 0x87, 0x23, 0x72, 0x45, 0x56, 0x68, 0x14, 0x62, 0x84 };
        private readonly string _passphrase;
        
        public EncryptionService()
        {
            // Use a machine-specific identifier combined with a fixed app key
            // In a production app, this would be more securely managed
            string machineId = Environment.MachineName;
            _passphrase = $"BudgetManagerApp-{machineId}-9AF26B";
        }
        
        public string EncryptString(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return string.Empty;
                  try
            {
                byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
                byte[] encryptedBytes;
                
                using (var aes = Aes.Create())
                {
                    // Use modern constructor with SHA256 hash algorithm
                    var key = new Rfc2898DeriveBytes(_passphrase, Salt, 10000, HashAlgorithmName.SHA256);
                    aes.Key = key.GetBytes(32); // 256-bit key
                    aes.IV = key.GetBytes(16); // 128-bit IV
                    
                    using (var ms = new MemoryStream())
                    {
                        using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                        {
                            cs.Write(plainBytes, 0, plainBytes.Length);
                            cs.FlushFinalBlock();
                        }
                        encryptedBytes = ms.ToArray();
                    }
                }
                
                return Convert.ToBase64String(encryptedBytes);
            }
            catch (Exception ex)
            {
                throw new Exception("Error encrypting data", ex);
            }
        }
        
        public string DecryptString(string encryptedText)
        {
            if (string.IsNullOrEmpty(encryptedText))
                return string.Empty;
                
            try
            {
                byte[] encryptedBytes = Convert.FromBase64String(encryptedText);
                byte[] decryptedBytes;
                  using (var aes = Aes.Create())
                {
                    // Use modern constructor with SHA256 hash algorithm
                    var key = new Rfc2898DeriveBytes(_passphrase, Salt, 10000, HashAlgorithmName.SHA256);
                    aes.Key = key.GetBytes(32); // 256-bit key
                    aes.IV = key.GetBytes(16); // 128-bit IV
                    
                    using (var ms = new MemoryStream())
                    {
                        using (var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write))
                        {
                            cs.Write(encryptedBytes, 0, encryptedBytes.Length);
                            cs.FlushFinalBlock();
                        }
                        decryptedBytes = ms.ToArray();
                    }
                }
                
                return Encoding.UTF8.GetString(decryptedBytes);
            }
            catch
            {
                // If decryption fails, return empty string instead of exception
                // This helps with backwards compatibility or corrupted data
                return string.Empty;
            }
        }
        
        public void EncryptFile(string sourceFilePath, string destinationFilePath)
        {
            if (!File.Exists(sourceFilePath))
                return;
                
            var plainText = File.ReadAllText(sourceFilePath);
            var encryptedText = EncryptString(plainText);
            File.WriteAllText(destinationFilePath, encryptedText);
        }
        
        public void DecryptFile(string sourceFilePath, string destinationFilePath)
        {
            if (!File.Exists(sourceFilePath))
                return;
                
            var encryptedText = File.ReadAllText(sourceFilePath);
            var plainText = DecryptString(encryptedText);
            File.WriteAllText(destinationFilePath, plainText);
        }
    }
}
