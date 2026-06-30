using QuickFix.Components;
using QuickFix.Data;
using Microsoft.EntityFrameworkCore;
using QuickFix.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using QuickFix.Auth;

var builder = WebApplication.CreateBuilder(args);

// =====================================================
// ADD SERVICES
// =====================================================
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddDbContext<QuickFixDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("QuickFixDb")));

builder.Services.AddScoped<TicketServices>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<AdminService>();

builder.Services.AddScoped<ProtectedSessionStorage>();
builder.Services.AddScoped<SessionAuthStateProvider>();

builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
    sp.GetRequiredService<SessionAuthStateProvider>());

builder.Services.AddAuthorizationCore();

builder.Services.AddScoped<DeviceCatalogService>();
builder.Services.AddScoped<GradeAPricingService>();

// =====================================================
// RENDER PORT SETUP
// =====================================================
var port = Environment.GetEnvironmentVariable("PORT");

if (!string.IsNullOrWhiteSpace(port))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

var app = builder.Build();

// =====================================================
// APPLY DATABASE MIGRATIONS + SEED ADMIN USER
// =====================================================
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<QuickFixDbContext>();
    var auth = scope.ServiceProvider.GetRequiredService<AuthService>();

    db.Database.Migrate();

    var adminEmail = "immanuellipscomb11@gmail.com";
    var adminName = "Admin";
    var adminPassword = "Admin123!";

    var admin = db.Users.FirstOrDefault(u => u.Email == adminEmail);

    if (admin == null)
    {
        admin = await auth.RegisterAsync(adminName, adminEmail, adminPassword);

        if (admin != null)
        {
            admin.Role = "Admin";
            db.SaveChanges();
        }
    }
    else
    {
        if (admin.Role != "Admin")
        {
            admin.Role = "Admin";
            db.SaveChanges();
        }
    }
}

// =====================================================
// CONFIGURE HTTP REQUEST PIPELINE
// =====================================================
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