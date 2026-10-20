namespace Sinaitv.API.Models
{
    public class LoginDto
    {
        public string CodUsuario { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class UserSessionDto
    {
        public string CodUsuario { get; set; } = string.Empty;
        public string NombreUsuario { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
    }
}