using System.Text.Json;

namespace BlazorApp2.Services;

public class DeviceCatalogService
{
    private readonly IWebHostEnvironment _env;

    public DeviceCatalogService(IWebHostEnvironment env)
    {
        _env = env;
    }

    public async Task<List<string>> GetAllModelsAsync()
    {
        var path = Path.Combine(_env.WebRootPath, "data", "deviceModels.json");

        if (!File.Exists(path))
            return new List<string>();

        var json = await File.ReadAllTextAsync(path);

        var doc = JsonSerializer.Deserialize<DeviceCatalogDoc>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return doc?.Categories?
            .SelectMany(c => c.Models.Select(m => $"{c.Name} - {m}"))
            .ToList()
            ?? new List<string>();
    }

    private class DeviceCatalogDoc
    {
        public List<Category> Categories { get; set; } = new();
    }

    private class Category
    {
        public string Name { get; set; } = "";
        public List<string> Models { get; set; } = new();
    }
}
