using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlazorApp1.Services;

namespace BlazorApp1.Pages;

[Authorize]
public class TodoList : PageModel
{
    private readonly TodoDbContext _db;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IAsymmetricEncryptionService _asymmetricEncryptionService;


    public TodoList(TodoDbContext db, UserManager<IdentityUser> userManager,
        IAsymmetricEncryptionService asymmetricEncryptionService)
    {
        _db = db;
        _userManager = userManager;
        _asymmetricEncryptionService = asymmetricEncryptionService;

    }

    [BindProperty]
    [Required]
    [Display(Name = "New Todo Item")]
    public string NewItemTitle { get; set; }

    public List<TodoItem> TodoItems { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        var encryptedItems = _db.TodoList.Where(t => t.UserId == user.Id).ToList();
        TodoItems = new List<TodoItem>();
        foreach (var item in encryptedItems)
        {
            try
            {
                byte[] cipherBytes = Convert.FromBase64String(item.Title);
                byte[] plainBytes = _asymmetricEncryptionService.Decrypt(cipherBytes);
                item.Title = System.Text.Encoding.UTF8.GetString(plainBytes);
            }
            catch
            {
                item.Title = "Decryption Error";
            }

            TodoItems.Add(item);
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (!ModelState.IsValid)
        {
            var encryptedItems = _db.TodoList.Where(t => t.UserId == user.Id).ToList();
            TodoItems = new List<TodoItem>();
            foreach (var item in encryptedItems)
            {
                try
                {
                    byte[] cipherBytes = Convert.FromBase64String(item.Title);
                    byte[] plainBytes = _asymmetricEncryptionService.Decrypt(cipherBytes);
                    item.Title = Encoding.UTF8.GetString(plainBytes);
                }
                catch
                {
                    item.Title = "Decryption Error";
                }

                TodoItems.Add(item);
            }
            
            return Page();

        }

        // Encrypt NewItemTitle before saving
        byte[] encryptedBytes = await _asymmetricEncryptionService.EncryptAsync(NewItemTitle);
        string encryptedTitle = Convert.ToBase64String(encryptedBytes);

        var newItem = new TodoItem
        {
            UserId = user.Id,
            Title = encryptedTitle,
            IsDone = false
        };
        _db.TodoList.Add(newItem);
        await _db.SaveChangesAsync();

        return RedirectToPage();
    }
}