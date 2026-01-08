using BlazorApp2.Components;
using BlazorApp2.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// ADDED: SQLite DbContext registration
builder.Services.AddDbContext<QuickFixDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("QuickFixDb")));

var app = builder.Build();

// ADDED: auto-create SQLite database + tables if they don't exist
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<QuickFixDbContext>();
    db.Database.EnsureCreated();
}

//  MapGet must be AFTER app is built
app.MapGet("/db-test", async (QuickFixDbContext db) =>
{
    db.Tickets.Add(new BlazorApp2.Data.Models.Ticket
    {
        Title = "First QuickFix Ticket",
        Description = "SQLite write test"
    });

    await db.SaveChangesAsync();

    var count = await db.Tickets.CountAsync();
    return $"SQLite write OK. Ticket count = {count}";
});

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
