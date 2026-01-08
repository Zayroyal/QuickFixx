using BlazorApp2.Components;
using BlazorApp2.Data;
using Microsoft.EntityFrameworkCore;
using BlazorApp2.Services;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// ADDED: SQLite DbContext registration
builder.Services.AddDbContext<QuickFixDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("QuickFixDb")));

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
