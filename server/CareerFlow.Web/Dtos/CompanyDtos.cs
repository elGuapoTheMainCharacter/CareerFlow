using System.ComponentModel.DataAnnotations;

namespace CareerFlow.Web.Dtos;

public class CompanyUpsertRequest
{
    [Required, MaxLength(120)] public string Name { get; set; } = string.Empty;

    // Free text from the user. CompanyService cleans it with UrlNormalizer.
    [MaxLength(200)] public string? Website { get; set; }

    [MaxLength(120)] public string? Location { get; set; }
}

public record CompanyDto(Guid Id, string Name, string? Website, string? Location, int OpportunityCount);