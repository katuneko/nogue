using System.Text.Json.Serialization;

namespace Nogue.Gameplay.Contracts
{
    public sealed class ContractDTO
    {
        [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;

        [JsonPropertyName("type")] public string Type { get; set; } = "single"; // "weekly" | "single" | "custom"
        [JsonPropertyName("product")] public string Product { get; set; } = string.Empty;
        [JsonPropertyName("qty")] public int Quantity { get; set; }

        [JsonPropertyName("quality_min")] public string? QualityMin { get; set; } // "A"/"B"/"C"

        // 可変: schedule or deadline
        [JsonPropertyName("schedule")] public ScheduleDTO? Schedule { get; set; }
        [JsonPropertyName("deadline")] public DeadlineDTO? Deadline { get; set; }

        [JsonPropertyName("pricing")] public PricingDTO? Pricing { get; set; }
        [JsonPropertyName("penalties")] public PenaltiesDTO? Penalties { get; set; }
        [JsonPropertyName("credit")] public CreditDTO? Credit { get; set; }
    }

    public sealed class ScheduleDTO { [JsonPropertyName("weeks")] public int Weeks { get; set; } [JsonPropertyName("day_of_week")] public int DayOfWeek { get; set; } }
    public sealed class DeadlineDTO { [JsonPropertyName("in_days")] public int InDays { get; set; } }
    public sealed class PricingDTO { [JsonPropertyName("base")] public float Base; public CurveDTO? Curve { get; set; } }
    public sealed class CurveDTO { [JsonPropertyName("lambda")] public float Lambda; [JsonPropertyName("gamma")] public float Gamma; [JsonPropertyName("cap")] public float Cap; }
    public sealed class PenaltiesDTO { [JsonPropertyName("late_per_day")] public float LatePerDay; [JsonPropertyName("cancel_after_days")] public int CancelAfterDays; }
    public sealed class CreditDTO { [JsonPropertyName("delta_on_success")] public int DeltaOnSuccess; [JsonPropertyName("delta_on_cancel")] public int DeltaOnCancel; }
}

