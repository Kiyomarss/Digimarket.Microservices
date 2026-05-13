using System.ComponentModel.DataAnnotations;

namespace Identity.Api.Contracts.Auth;

public sealed record RegisterRequest(
    [Required] string Email,
    [Required] string Password);