using System.ComponentModel.DataAnnotations;

namespace CareerFlow.Web.Dtos;

public class RegisterRequest
{
    [Required, MaxLength(80)] public string DisplayName { get; set; } = string.Empty;
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    [Required, MinLength(8)] public string Password { get; set; } = string.Empty;
    public string AccountType { get; set; } = string.Empty;
    public bool IsDiscoverable { get; set; }
}

public class LoginRequest
{
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    [Required] public string Password { get; set; } = string.Empty;
    public bool RememberMe { get; set; }
}

public record UserDto(string Id, string DisplayName, string Email, IReadOnlyList<string> Roles);