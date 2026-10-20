using Sinaitv.API.Models;

namespace Sinaitv.API.Services
{
    public interface IAuthService
    {
        Task<UserSessionDto?> AuthenticateAsync(string codUsuario, string password);
    }
}