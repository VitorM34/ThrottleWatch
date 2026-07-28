using System.ComponentModel.DataAnnotations;

namespace ThrottleWatch.Dashboard.Models;

public sealed class AlertRuleFormModel
{
    public Guid? Id { get; set; }

    [Required(ErrorMessage = "Nome é obrigatório.")]
    [StringLength(100, ErrorMessage = "Nome deve ter no máximo 100 caracteres.")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Descrição deve ter no máximo 500 caracteres.")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Condição é obrigatória.")]
    [StringLength(500, ErrorMessage = "Condição deve ter no máximo 500 caracteres.")]
    public string Condition { get; set; } = "block_rate";

    [Range(0, double.MaxValue, ErrorMessage = "Threshold deve ser maior ou igual a 0.")]
    public double Threshold { get; set; } = 10;

    [Required]
    public AlertSeverity Severity { get; set; } = AlertSeverity.Warning;

    [Range(0, int.MaxValue, ErrorMessage = "Cooldown deve ser maior ou igual a 0.")]
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
