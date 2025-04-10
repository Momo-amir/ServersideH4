using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace BlazorApp1.Services
{
    public interface IAsymmetricEncryptionService
    {
        // Returns the public key as a byte array
        byte[] GetPublicKey();

        // Calls an external API to encrypt the given plaintext using our public key.
        Task<byte[]> EncryptAsync(string plaintext);

        // Decrypts the provided encrypted data using our private key.
        byte[] Decrypt(byte[] encryptedData);
    }

    public class AsymmetricEncryptionService : IAsymmetricEncryptionService
    {
        private readonly RSA _rsa;
        private readonly string _privateKeyFile = "asymmetric_private.key";
        private readonly HttpClient _httpClient;
        private byte[] _publicKey;

        public AsymmetricEncryptionService(HttpClient httpClient)
        {
            _httpClient = httpClient;

            // Check if the private key file exists so we have persistent keys across restarts.
            if (File.Exists(_privateKeyFile))
            {
                var privateKeyBytes = File.ReadAllBytes(_privateKeyFile);
                _rsa = RSA.Create();
                _rsa.ImportRSAPrivateKey(privateKeyBytes, out _);
            }
            else
            {
                _rsa = RSA.Create(2048); // Generate a new RSA key pair
                var privateKeyBytes = _rsa.ExportRSAPrivateKey();
                File.WriteAllBytes(_privateKeyFile, privateKeyBytes);
            }
            // Export the public key using ExportSubjectPublicKeyInfo()
            _publicKey = _rsa.ExportSubjectPublicKeyInfo();
        }

        public byte[] GetPublicKey()
        {
            return _publicKey;
        }

        // Calls an external WebAPI to encrypt the plaintext using our public key.
        public async Task<byte[]> EncryptAsync(string plaintext)
        {
            // Set the API endpoint URL (update this URL as appropriate)
            var apiUrl = "https://localhost:5001/api/encrypt";

            // Create the request payload containing the Base64-encoded public key and the plaintext.
            var requestModel = new EncryptionRequest
            {
                PublicKey = Convert.ToBase64String(_publicKey),
                PlainText = plaintext
            };

            // Call the external API using HttpClient.
            var response = await _httpClient.PostAsJsonAsync(apiUrl, requestModel);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<EncryptionResponse>();

            // Convert the returned Base64 string back to a byte array.
            return Convert.FromBase64String(result.EncryptedText);
        }

        // Decrypts data using the stored RSA private key.
        public byte[] Decrypt(byte[] encryptedData)
        {
            return _rsa.Decrypt(encryptedData, RSAEncryptionPadding.Pkcs1);
        }
    }

    // Models used for communicating with the external encryption API.
    public class EncryptionRequest
    {
        public string PublicKey { get; set; }
        public string PlainText { get; set; }
    }

    public class EncryptionResponse
    {
        public string EncryptedText { get; set; }
    }
}