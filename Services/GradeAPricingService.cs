using System.Text.Json;

namespace QuickFix.Services;

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

        doc.Rules ??= new();
        doc.DiagnosticDefaults ??= new();

        var category = ExtractCategory(deviceValue);

        var normalizedDevice = Normalize(deviceValue);
        var normalizedDiagnostic = Normalize(diagnostic);
        var normalizedCategory = Normalize(category);

        // =====================================================
        // 1. EXACT DEVICE + DIAGNOSTIC
        // =====================================================
        var r1 = doc.Rules.FirstOrDefault(r =>
            !string.IsNullOrWhiteSpace(r.DeviceExact) &&
            Normalize(r.DeviceExact) == normalizedDevice &&
            Normalize(r.Diagnostic) == normalizedDiagnostic);

        if (r1 != null)
            return r1.ToResult($"deviceExact: {r1.DeviceExact}");

        // =====================================================
        // 2. DEVICE CONTAINS + DIAGNOSTIC
        // Checks both directions to handle slightly different dropdown text.
        // =====================================================
        var r2 = doc.Rules.FirstOrDefault(r =>
            !string.IsNullOrWhiteSpace(r.DeviceContains) &&
            Normalize(r.Diagnostic) == normalizedDiagnostic &&
            (
                normalizedDevice.Contains(Normalize(r.DeviceContains)) ||
                Normalize(r.DeviceContains).Contains(normalizedDevice)
            ));

        if (r2 != null)
            return r2.ToResult($"deviceContains: {r2.DeviceContains}");

        // =====================================================
        // 3. MODEL NAME + DIAGNOSTIC
        // Example:
        // deviceValue: Apple - iPhone - iPhone 15
        // modelName: iPhone 15
        // =====================================================
        var modelName = ExtractModelName(deviceValue);
        var normalizedModelName = Normalize(modelName);

        var rModel = doc.Rules.FirstOrDefault(r =>
            !string.IsNullOrWhiteSpace(r.DeviceContains) &&
            Normalize(r.Diagnostic) == normalizedDiagnostic &&
            Normalize(r.DeviceContains).Contains(normalizedModelName));

        if (rModel != null)
            return rModel.ToResult($"modelName: {modelName}");

        // =====================================================
        // 4. CATEGORY + DIAGNOSTIC
        // =====================================================
        var r3 = doc.Rules.FirstOrDefault(r =>
            !string.IsNullOrWhiteSpace(r.Category) &&
            Normalize(r.Category) == normalizedCategory &&
            Normalize(r.Diagnostic) == normalizedDiagnostic);

        if (r3 != null)
            return r3.ToResult($"category: {r3.Category}");

        // =====================================================
        // 5. DIAGNOSTIC DEFAULT LAST
        // =====================================================
        var d1 = doc.DiagnosticDefaults.FirstOrDefault(d =>
            Normalize(d.Diagnostic) == normalizedDiagnostic);

        return d1?.ToResult("diagnosticDefaults") ?? PricingResult.Empty("No pricing match found");
    }

    // =====================================================
    // CATEGORY EXTRACTION
    // =====================================================
    private static string ExtractCategory(string deviceValue)
    {
        if (string.IsNullOrWhiteSpace(deviceValue))
            return "";

        if (deviceValue.StartsWith("Apple - iPhone", StringComparison.OrdinalIgnoreCase))
            return "Apple - iPhone";

        if (deviceValue.StartsWith("Samsung - Galaxy", StringComparison.OrdinalIgnoreCase))
            return "Samsung - Galaxy S Series";

        if (deviceValue.StartsWith("Google - Pixel", StringComparison.OrdinalIgnoreCase))
            return "Google - Pixel";

        if (deviceValue.StartsWith("Windows", StringComparison.OrdinalIgnoreCase))
            return "Windows Laptop";

        if (deviceValue.StartsWith("Mac", StringComparison.OrdinalIgnoreCase))
            return "Mac Laptop";

        return "";
    }

    // =====================================================
    // MODEL EXTRACTION
    // =====================================================
    private static string ExtractModelName(string deviceValue)
    {
        if (string.IsNullOrWhiteSpace(deviceValue))
            return "";

        var parts = deviceValue.Split(" - ", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (parts.Length >= 3)
            return parts[2];

        return deviceValue;
    }

    // =====================================================
    // NORMALIZE TEXT FOR MATCHING
    // =====================================================
    private static string Normalize(string? value)
    {
        return (value ?? "")
            .Trim()
            .Replace("–", "-")
            .Replace("—", "-")
            .Replace("_", " ")
            .Replace("  ", " ")
            .ToLowerInvariant();
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
    public string MatchedBy { get; set; } = "";

    public static PricingResult Empty(string matchedBy = "")
        => new() { MatchedBy = matchedBy };
}