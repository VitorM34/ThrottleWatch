using System.ComponentModel.DataAnnotations;

namespace ThrottleWatch.Dashboard.Models;

public sealed class AlertRuleFormModel
{
    public Guid? Id { get; set; }

    [Required(ErrorMessage = "Name is required.")]
    [StringLength(100, ErrorMessage = "Name must be at most 100 characters.")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Description must be at most 500 characters.")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Condition is required.")]
    [StringLength(500, ErrorMessage = "Condition must be at most 500 characters.")]
    public string Condition { get; set; } = "block_rate";

    [Range(0, double.MaxValue, ErrorMessage = "Threshold must be greater than or equal to 0.")]
    public double Threshold { get; set; } = 10;

    [Required]
    public AlertSeverity Severity { get; set; } = AlertSeverity.Warning;

    [Range(0, int.MaxValue, ErrorMessage = "Cooldown must be greater than or equal to 0.")]
    public int CooldownMinutes { get; set; } = 15;

    public bool IsEnabled { get; set; } = true;

    public static AlertRuleFormModel CreateNew() => new();

    public static AlertRuleFormModel FromRule(AlertRuleInfo rule) => new()
    {
        Id = rule.Id,
        Name = rule.Name,
        Description = rule.Description,
        Condition = rule.Condition,
        Threshold = rule.Threshold,
        Severity = rule.Severity,
        CooldownMinutes = rule.CooldownMinutes,
        IsEnabled = rule.IsEnabled
    };
}
