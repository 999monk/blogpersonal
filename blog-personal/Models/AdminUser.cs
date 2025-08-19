namespace blog_personal.Models;

public class AdminUser
{
    public string? Username { get; set; } = "admin";
    public string? PasswordHash { get; set; }
}