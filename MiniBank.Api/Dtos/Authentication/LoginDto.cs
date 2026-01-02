using System;
using System.ComponentModel.DataAnnotations;
namespace MiniBank.Api.Dtos.Authentication;

public class LoginDto
{
    [Required]
    [EmailAddress]
    public string? Email { get; set; }
    [Required]
    public string? Password { get; set; }

}
