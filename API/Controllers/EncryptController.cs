using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class EncryptController : ControllerBase
{
    [HttpPost]
    public ActionResult<EncryptionResponse> Post([FromBody] EncryptionRequest request)
    {
        try
        {
            // Convert the incoming public key from Base64 to byte array.
            var publicKeyBytes = Convert.FromBase64String(request.PublicKey);

            using (RSA rsa = RSA.Create())
            {
                // Import the public key.
                rsa.ImportSubjectPublicKeyInfo(publicKeyBytes, out _);
                var plaintextBytes = System.Text.Encoding.UTF8.GetBytes(request.PlainText);
                // Encrypt the plaintext using RSA and PKCS#1 padding.
                var encryptedBytes = rsa.Encrypt(plaintextBytes, RSAEncryptionPadding.Pkcs1);
                return Ok(new EncryptionResponse { EncryptedText = Convert.ToBase64String(encryptedBytes) });
            }
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}

public class EncryptionRequest
{
    public string PublicKey { get; set; }
    public string PlainText { get; set; }
}

public class EncryptionResponse
{
    public string EncryptedText { get; set; }
}