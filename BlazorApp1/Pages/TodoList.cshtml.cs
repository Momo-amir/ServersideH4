using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorApp1.Pages;

[Authorize]
public class TodoList : PageModel
{
    private readonly TodoDbContext _db;
    private readonly UserManager<IdentityUser> _userManager;

    public TodoList(TodoDbContext db, UserManager<IdentityUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    [BindProperty]
    [Required]
    [Display(Name = "New Todo Item")]
    public string NewItemTitle { get; set; }

    public List<TodoItem> TodoItems { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        TodoItems = _db.TodoList.Where(t => t.UserId == user.Id).ToList();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (!ModelState.IsValid)
        {
            TodoItems = _db.TodoList.Where(t => t.UserId == user.Id).ToList();
            return Page();
        }

        var newItem = new TodoItem
        {
            UserId = user.Id,
            Title = NewItemTitle,
            IsDone = false
        };

        _db.TodoList.Add(newItem);
        await _db.SaveChangesAsync();

        return RedirectToPage();
    }
}