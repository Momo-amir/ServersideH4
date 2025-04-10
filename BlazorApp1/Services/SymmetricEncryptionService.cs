using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace BlazorApp1.Services
{
    public interface ISymmetricEncryptionService
    {
        byte[] Encrypt(string plaintext);
        string Decrypt(byte[] cipherText);
    }
    
    public class SymmetricEncryptionService : ISymmetricEncryptionService
    {
        // For symmetric encryption we use AES.
        // In production you should persist the key and IV so that encryption remains consistent across restarts.
        // Here, for demonstration, we generate them on instantiation.
        private readonly byte[] _key;
        private readonly byte[] _iv;
        
        public SymmetricEncryptionService()
        {
            using (var aes = Aes.Create())
            {
                _key = aes.Key;
                _iv = aes.IV;
            }
        }
        
        public byte[] Encrypt(string plaintext)
        {
            using (var aes = Aes.Create())
            {
                aes.Key = _key;
                aes.IV = _iv;
                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                using (var ms = new MemoryStream())
                using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                using (var writer = new StreamWriter(cs))
                {
                    writer.Write(plaintext);
                    writer.Flush();
                    cs.FlushFinalBlock();
                    return ms.ToArray();
                }
            }
        }
        
        public string Decrypt(byte[] cipherText)
        {
            using (var aes = Aes.Create())
            {
                aes.Key = _key;
                aes.IV = _iv;
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                using (var ms = new MemoryStream(cipherText))
                using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                using (var reader = new StreamReader(cs))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}