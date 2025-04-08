using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using Data;

[Authorize]
public class CprModel : PageModel
{
    private readonly TodoDbContext _todoDb;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public CprModel(TodoDbContext todoDb, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _todoDb = todoDb;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    [BindProperty]
    [Required]
    [Display(Name = "CPR Number")]
    public string CprValue { get; set; }

    public string UserName { get; set; }
    public IList<string> Roles { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        UserName = user?.UserName;
        Roles = (await _userManager.GetRolesAsync(user)).ToList();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        var user = await _userManager.GetUserAsync(User);
        var existing = _todoDb.Cprs.FirstOrDefault(x => x.UserId == user.Id);

        if (existing == null)
        {
            _todoDb.Cprs.Add(new Cpr { UserId = user.Id, Value = CprValue });
            await _todoDb.SaveChangesAsync();
        }

        return RedirectToPage("/TodoList");
    }
}