using QuickFix.Components;
using QuickFix.Data;
using Microsoft.EntityFrameworkCore;
using QuickFix.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using QuickFix.Auth;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// ADDED: SQLite DbContext registration
builder.Services.AddDbContext<QuickFixDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("QuickFixDb")));

builder.Services.AddScoped<TicketServices>();

//  Auth/Login services (Session-based)
builder.Services.AddScoped<AuthService>();

//NEW ADMIN TINGS
builder.Services.AddScoped<AdminService>();


//  Session storage + auth state provider
builder.Services.AddScoped<ProtectedSessionStorage>();
builder.Services.AddScoped<SessionAuthStateProvider>();

//  Tell Blazor that our provider IS the AuthenticationStateProvider
builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
    sp.GetRequiredService<SessionAuthStateProvider>());


//  Enables [Authorize] + <AuthorizeView> in Razor Components
builder.Services.AddAuthorizationCore();

builder.Services.AddScoped<DeviceCatalogService>();
builder.Services.AddScoped<GradeAPricingService>();
builder.Services.AddScoped<TicketServices>();
var port = Environment.GetEnvironmentVariable("PORT");

if (!string.IsNullOrWhiteSpace(port))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

var app = builder.Build();


// =====================================================
// APPLY DATABASE MIGRATIONS ON STARTUP
// =====================================================
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<QuickFix.Data.QuickFixDbContext>();
    db.Database.Migrate();

   
}

 using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<QuickFix.Data.QuickFixDbContext>();

        db.Database.Migrate();

        var admin = db.Users.FirstOrDefault(u => u.Email == "immanuellipscomb11@gmail.com");

        if (admin != null)
        {
            admin.Role = "Admin";
            db.SaveChanges();
        }
    }

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
