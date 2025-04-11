using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BlazorApp1.Services;
using System.Security.Cryptography;

using Data;
using System.Globalization; // Added for culture-specific parsing


 namespace BlazorApp1.Pages
 {
         using Microsoft.AspNetCore.Identity;
         public class CprModel : PageModel
     {
             private readonly IHashingService _hashingService;
         private readonly TodoDbContext _todoDbContext;
         private readonly UserManager<IdentityUser> _userManager;
         private readonly SignInManager<IdentityUser> _signInManager;
 
             public CprModel(IHashingService hashingService, TodoDbContext todoDbContext, UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
         {
                 _hashingService = hashingService;
                 _todoDbContext = todoDbContext;
                 _userManager = userManager;
                 _signInManager = signInManager;
             }
 
                 // Bind the CPR input to this property
                     [BindProperty]
             public string CprValue { get; set; }
         public string UserName { get; set; }
         public string[] Roles { get; set; }

         public async Task OnGetAsync()
         {
             if (_signInManager.IsSignedIn(User))
             {
                 var currentUser = await _userManager.GetUserAsync(User);
                 if (currentUser != null)
                 {
                     // Retrieve the stored full name token and roles
                     UserName = await _userManager.GetAuthenticationTokenAsync(currentUser, "Default", "FullName") ??
                                "";
                     var roles = await _userManager.GetRolesAsync(currentUser);
                     Roles = roles.ToArray();
                 }
                 else
                 {
                     UserName = "";
                     Roles = new string[0];
                 }
             }
             else
             {
                 UserName = "";
                 Roles = new string[0];
             }
         }

         public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                UserName = User?.Identity?.Name;
                Roles = new string[0];
                return Page();
            }

            // Retrieve existing CPR record if it exists
            var existingCpr = _todoDbContext.Cprs.FirstOrDefault(c => User.Identity != null && c.UserId == User.Identity.Name);

            if (existingCpr != null)
            {
                // Validate the input against the stored record.
                // Expected stored value format: hashedCpr:base64Salt:iterations:hashAlgorithm
                var parts = existingCpr.Value.Split(':');
                if (parts.Length != 4)
                {
                    ModelState.AddModelError(string.Empty, "Stored CPR record is invalid.");
                    UserName = User?.Identity?.Name;
                    Roles = new string[0];
                    return Page();
                }
                var storedHash = parts[0];
                var saltBase64 = parts[1];
                var iterationsStr = parts[2];
                var hashAlgorithm = parts[3];

                // Use the culture-specific overload to disambiguate TryParse
                if (!int.TryParse(iterationsStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out int iterations))
                {
                    ModelState.AddModelError(string.Empty, "Stored CPR iterations is invalid.");
                    UserName = User?.Identity?.Name;
                    Roles = new string[0];
                    return Page();
                }

                var salt = Convert.FromBase64String(saltBase64);
                bool isValid = _hashingService.VerifyPBKDF2(CprValue, storedHash, salt, iterations, hashAlgorithm);
                if (!isValid)
                {
                    ModelState.AddModelError(string.Empty, "Invalid CPR number.");
                    UserName = User?.Identity?.Name;
                    Roles = new string[0];
                    return Page();
                }
                // Valid CPR number found, mark the session as verified and navigate to TodoList page.
                HttpContext.Session.SetString("CPRVerified", "true");
                return RedirectToPage("/TodoList");
            }
            else
            {
                // No CPR record exists, so create one.
                byte[] salt = new byte[16];
                using (var rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(salt);
                }

                int iterations = 10000;
                string hashAlgorithm = "SHA256";
                string hashedCpr = _hashingService.HashPBKDF2(CprValue, salt, iterations, hashAlgorithm);
                string storedValue = $"{hashedCpr}:{Convert.ToBase64String(salt)}:{iterations}:{hashAlgorithm}";

                var cprRecord = new Cpr
                {
                    UserId = User?.Identity?.Name,
                    Value = storedValue
                };
                _todoDbContext.Cprs.Add(cprRecord);
                await _todoDbContext.SaveChangesAsync();

                // Mark the session as verified.
                HttpContext.Session.SetString("CPRVerified", "true");
                return RedirectToPage("/TodoList");
            }
        }
    }
}