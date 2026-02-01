using System.Text.Json;

namespace BlazorApp2.Services;

public class GradeAPricingService
{
    private readonly IWebHostEnvironment _env;

    public GradeAPricingService(IWebHostEnvironment env)
    {
        _env = env;
    }

    public async Task<PricingResult> GetPricingAsync(string? deviceValue, string? diagnostic)
    {
        deviceValue = (deviceValue ?? "").Trim();
        diagnostic = (diagnostic ?? "").Trim();

        if (string.IsNullOrWhiteSpace(diagnostic))
            return PricingResult.Empty("No diagnostic selected");

        var path = Path.Combine(_env.WebRootPath, "data", "gradeA_pricing.json");
        if (!File.Exists(path))
            return PricingResult.Empty($"Missing pricing file: {path}");

        PricingDoc? doc;
        try
        {
            var json = await File.ReadAllTextAsync(path);

            doc = JsonSerializer.Deserialize<PricingDoc>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (Exception ex)
        {
            return PricingResult.Empty($"Pricing JSON failed to load: {ex.Message}");
        }

        if (doc == null)
            return PricingResult.Empty("Pricing JSON returned null");

        // Safety: never null
        doc.Rules ??= new();
        doc.DiagnosticDefaults ??= new();

        var category = ExtractCategory(deviceValue);

        // Normalize for matching
        var dv = deviceValue;
        var diag = diagnostic;

        // 1) Exact device + diagnostic 
        var r1 = doc.Rules.FirstOrDefault(r =>
            !string.IsNullOrWhiteSpace(r.DeviceExact) &&
            r.DeviceExact.Equals(dv, StringComparison.OrdinalIgnoreCase) &&
            r.Diagnostic.Equals(diag, StringComparison.OrdinalIgnoreCase));

        if (r1 != null) return r1.ToResult($"deviceExact: {r1.DeviceExact}");

        // 2) deviceContains + diagnostic
        var r2 = doc.Rules.FirstOrDefault(r =>
            !string.IsNullOrWhiteSpace(r.DeviceContains) &&
            dv.Contains(r.DeviceContains, StringComparison.OrdinalIgnoreCase) &&
            r.Diagnostic.Equals(diag, StringComparison.OrdinalIgnoreCase));

        if (r2 != null) return r2.ToResult($"deviceContains: {r2.DeviceContains}");

        
        var r3 = doc.Rules.FirstOrDefault(r =>
            !string.IsNullOrWhiteSpace(r.Category) &&
            r.Category.Equals(category, StringComparison.OrdinalIgnoreCase) &&
            r.Diagnostic.Equals(diag, StringComparison.OrdinalIgnoreCase));

        if (r3 != null) return r3.ToResult($"category: {r3.Category}");

        // 4) fallback: diagnostic defaults
        var d1 = doc.DiagnosticDefaults.FirstOrDefault(d =>
            d.Diagnostic.Equals(diag, StringComparison.OrdinalIgnoreCase));

        return d1?.ToResult("diagnosticDefaults") ?? PricingResult.Empty("No pricing match found");
    }

    // IMPORTANT:
    // Your JSON categories look like "Apple - iPhone" or "Samsung - Galaxy S Series"
    // Your device dropdown values look like "Apple - iPhone - iPhone 8 Plus"
    // So category should be FIRST TWO segments, not just first segment.
    private static string ExtractCategory(string deviceValue)
    {
        // Device strings look like: "Apple - iPhone - iPhone 8 Plus"
        // We want the category to be: "Apple - iPhone"
        if (string.IsNullOrWhiteSpace(deviceValue)) return "";

        var parts = deviceValue.Split(" - ", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (parts.Length >= 2)
            return $"{parts[0]} - {parts[1]}";

        return "";
    }




    private class PricingDoc
    {
        public int Version { get; set; }
        public string? Currency { get; set; }

        public List<PricingRule>? Rules { get; set; }
        public List<PricingRule>? DiagnosticDefaults { get; set; }
    }

    private class PricingRule
    {
        public string? DeviceExact { get; set; }
        public string? DeviceContains { get; set; }
        public string? Category { get; set; }
        public string Diagnostic { get; set; } = "";

        public PartInfo Part { get; set; } = new();
        public LaborInfo Labor { get; set; } = new();

        public PricingResult ToResult(string matchedBy) => new()
        {
            PartsCost = Part.Price,
            LaborCost = Labor.Price,
            PartName = Part.Name ?? "",
            PartGrade = string.IsNullOrWhiteSpace(Part.Grade) ? "A" : Part.Grade!,
            MatchedBy = matchedBy
        };
    }

    private class PartInfo
    {
        public string? Grade { get; set; } = "A";
        public string? Name { get; set; } = "";
        public decimal Price { get; set; } = 0m;
        public string? Source { get; set; } = "";
        public string? ReferenceUrl { get; set; } = "";
    }

    private class LaborInfo
    {
        public decimal Price { get; set; } = 0m;
    }
}

public class PricingResult
{
    public decimal PartsCost { get; set; }
    public decimal LaborCost { get; set; }
    public string PartName { get; set; } = "";
    public string PartGrade { get; set; } = "A";

    // Debug text so you can SEE what matched
    public string MatchedBy { get; set; } = "";

    public static PricingResult Empty(string matchedBy = "")
        => new() { MatchedBy = matchedBy };
}
