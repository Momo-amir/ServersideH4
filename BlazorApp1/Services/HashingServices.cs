using System;
using System.Security.Cryptography;
using System.Text;

namespace BlazorApp1.Services
{
    public interface IHashingService
    {
         // Fast hash – not intended for password or sensitive data validation
         string HashSHA2(string input);
         
         // HMAC: combines a key and an input to produce a keyed-hash
         string HashHMAC(string input, string key);
         
         // PBKDF2: recommended for secure hashing with salt, iterations and algorithm choice.
         string HashPBKDF2(string input, byte[] salt, int iterations, string hashAlgorithmName = "SHA256");
         
         // Optional verification method for PBKDF2
         bool VerifyPBKDF2(string input, string hashed, byte[] salt, int iterations, string hashAlgorithmName = "SHA256");
         
         // BCrypt: a popular algorithm designed for passwords.
         string HashBcrypt(string input, int workFactor = 10);
         
         // Optional verification for BCrypt.
         bool VerifyBcrypt(string input, string hashed);
    }

    public class HashingService : IHashingService
    {
         public string HashSHA2(string input)
         {
             using (var sha256 = SHA256.Create())
             {
                  var inputBytes = Encoding.UTF8.GetBytes(input);
                  var hashBytes = sha256.ComputeHash(inputBytes);
                  return Convert.ToBase64String(hashBytes);
             }
         }
         
         public string HashHMAC(string input, string key)
         {
             using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key)))
             {
                  var inputBytes = Encoding.UTF8.GetBytes(input);
                  var hashBytes = hmac.ComputeHash(inputBytes);
                  return Convert.ToBase64String(hashBytes);
             }
         }
         
         public string HashPBKDF2(string input, byte[] salt, int iterations, string hashAlgorithmName = "SHA256")
         {
              using (var pbkdf2 = new Rfc2898DeriveBytes(input, salt, iterations, new HashAlgorithmName(hashAlgorithmName)))
              {
                  // Here we get a 256-bit hash (32 bytes)
                  byte[] hash = pbkdf2.GetBytes(32);
                  return Convert.ToBase64String(hash);
              }
         }
         
         public bool VerifyPBKDF2(string input, string hashed, byte[] salt, int iterations, string hashAlgorithmName = "SHA256")
         {
              var computedHash = HashPBKDF2(input, salt, iterations, hashAlgorithmName);
              return computedHash == hashed;
         }
         
         public string HashBcrypt(string input, int workFactor = 10)
         {
              return BCrypt.Net.BCrypt.HashPassword(input, workFactor);
         }
         
         public bool VerifyBcrypt(string input, string hashed)
         {
              return BCrypt.Net.BCrypt.Verify(input, hashed);
         }
    }
}