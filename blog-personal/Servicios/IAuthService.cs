using blog_personal.Models;

namespace blog_personal.Servicios;

public interface IAuthService
{
    Task<string?> LoginAsync(string username, string password);
    
    Task<bool> ChangePasswordAsync(string currentPassword, string newPassword);
    Task InitializeAsync();
}