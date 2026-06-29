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


var app = builder.Build();




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
