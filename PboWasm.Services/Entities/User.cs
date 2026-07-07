using System;
using System.ComponentModel.DataAnnotations;

namespace PboWasm.Services.Entities;

public class User
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool IsEmailValidated { get; set; }
    public string? ValidationCode { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
