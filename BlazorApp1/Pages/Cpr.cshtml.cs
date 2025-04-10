using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BlazorApp1.Services;
using System.Security.Cryptography;
using System;
using System.Threading.Tasks;
using Data;

namespace BlazorApp1.Pages
{
    public class CprModel : PageModel
    {
        private readonly IHashingService _hashingService;
        private readonly TodoDbContext _todoDbContext;

        public CprModel(IHashingService hashingService, TodoDbContext todoDbContext)
        {
            _hashingService = hashingService;
            _todoDbContext = todoDbContext;
        }

        // Bind the CPR input to this property
        [BindProperty]
        public string CprValue { get; set; }
        public string UserName { get; set; }
        public string[] Roles { get; set; }

        public void OnGet()
        {
            // Initialize any page data here
            UserName = User?.Identity?.Name;
            // Initialize Roles to an empty array to avoid null reference issues
            Roles = new string[0];
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Generate a 16-byte random salt
            byte[] salt = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            int iterations = 10000;
            string hashAlgorithm = "SHA256";

            // Hash the CPR value using PBKDF2 with salt, iteration count, and algorithm
            string hashedCpr = _hashingService.HashPBKDF2(CprValue, salt, iterations, hashAlgorithm);
            // Combine hash with parameters so that they can be verified later
            string storedValue = $"{hashedCpr}:{Convert.ToBase64String(salt)}:{iterations}:{hashAlgorithm}";

            // Create a new CPR record; assuming your Cpr entity includes UserId and Value properties
            var cprRecord = new Cpr
            {
                UserId = User?.Identity?.Name,
                Value = storedValue
            };

            _todoDbContext.Cprs.Add(cprRecord);
            await _todoDbContext.SaveChangesAsync();

            return RedirectToPage("/TodoList");
        }
    }
}