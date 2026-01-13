using BlazorApp2.Components;
using BlazorApp2.Data;
using Microsoft.EntityFrameworkCore;
using BlazorApp2.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using BlazorApp2.Auth;


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

//  Session storage + auth state provider
builder.Services.AddScoped<ProtectedSessionStorage>();
builder.Services.AddScoped<SessionAuthStateProvider>();

//  Tell Blazor that our provider IS the AuthenticationStateProvider
builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
    sp.GetRequiredService<SessionAuthStateProvider>());


//  Enables [Authorize] + <AuthorizeView> in Razor Components
builder.Services.AddAuthorizationCore();



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
