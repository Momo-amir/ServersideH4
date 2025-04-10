using BlazorApp1.Components;
using BlazorApp1.Services;
using Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=identity.db";
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

var todoDbConnectionString = builder.Configuration.GetConnectionString("TodoDbContextConnection") ?? "Data Source=tododb.db";
builder.Services.AddDbContext<TodoDbContext>(options =>
    options.UseSqlite(todoDbConnectionString));

builder.Services
    .AddDefaultIdentity<IdentityUser>(options =>
    {
        // Your password config here
        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
    })
    .AddRoles<IdentityRole>()                     
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();


builder.Services.AddAuthentication()
    .AddGoogle(options =>
    {
        options.ClientId = "704519639950-07rhrpmpqovklt4alqjcefv1om8okboi.apps.googleusercontent.com";
        options.ClientSecret = "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";
    });

// Add services to the container.
builder.Services.AddSession();
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddHttpClient<IAsymmetricEncryptionService, AsymmetricEncryptionService>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7090/");
});
builder.Services.AddRazorPages(); // Added to support Razor Pages
builder.Services.AddScoped<IHashingService, HashingService>();
// For symmetric encryption (if needed)
builder.Services.AddScoped<ISymmetricEncryptionService, SymmetricEncryptionService>();

// For asymmetric encryption – note the use of HttpClient:
builder.Services.AddHttpClient<IAsymmetricEncryptionService, AsymmetricEncryptionService>();
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var roles = new[] { "Admin", "User" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();


// 1) Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.UseMiddleware<BlazorApp1.Middleware.RequireCprMiddleware>();

app.UseAntiforgery();

// 2) Map Identity (login, register, etc.)
app.MapRazorPages();  // required for Identity UI

// 3) Map your Blazor components
// Force authentication on *all* pages by calling RequireAuthorization()
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .RequireAuthorization();

// 4) Alternatively, if you have a fallback page like _Host:
app.Run();