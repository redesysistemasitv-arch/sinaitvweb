using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using Sinaitv.API.Models;

namespace Sinaitv.API.Services
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _config;

        public AuthService(IConfiguration config)
        {
            _config = config;
        }

        public async Task<UserSessionDto?> AuthenticateAsync(string codUsuario, string password)
        {
            var masterConnStr = _config.GetConnectionString("MasterProfitConnection");
            var sinaitvConnStr = _config.GetConnectionString("SinaitvConnection");

            // 1. Validar usuario en MasterProfitPro
            using var dbMaster = new SqlConnection(masterConnStr);
            var sqlUser = @"
                SELECT Cod_Usuario, Desc_Usuario 
                FROM MpUsuario 
                WHERE UPPER(LTRIM(RTRIM(Cod_Usuario))) = UPPER(@CodUsuario) AND Estado = '3'"; // Estado 3 = Activo en Profit

            var userProfit = await dbMaster.QueryFirstOrDefaultAsync<dynamic>(sqlUser, new { CodUsuario = codUsuario.Trim() });

            if (userProfit == null) return null; // Usuario no existe o inactivo

            // 2. Obtener el Rol del usuario desde SINAITVDB
            using var dbSinaitv = new SqlConnection(sinaitvConnStr);
            var sqlRol = @"
                SELECT r.NombreRol 
                FROM UsuariosRoles ur
                INNER JOIN Roles r ON ur.RolID = r.RolID
                WHERE UPPER(LTRIM(RTRIM(ur.Cod_Usuario))) = UPPER(@CodUsuario)";

            string? nombreRol = await dbSinaitv.QueryFirstOrDefaultAsync<string>(sqlRol, new { CodUsuario = codUsuario.Trim() });

            // Si el usuario no tiene rol asignado aún, se le asigna 'SoloLectura' por defecto
            if (string.IsNullOrEmpty(nombreRol))
            {
                nombreRol = "SoloLectura";
            }

            // 3. Generar JWT Token
            var token = GenerateJwtToken(userProfit.Cod_Usuario.ToString().Trim(), userProfit.Desc_Usuario.ToString().Trim(), nombreRol);

            return new UserSessionDto
            {
                CodUsuario = userProfit.Cod_Usuario.ToString().Trim(),
                NombreUsuario = userProfit.Desc_Usuario.ToString().Trim(),
                Rol = nombreRol,
                Token = token
            };
        }

        private string GenerateJwtToken(string codUsuario, string nombreUsuario, string rol)
        {
            var jwtSettings = _config.GetSection("Jwt");
            var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]!);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, codUsuario),
                new Claim(ClaimTypes.Name, nombreUsuario),
                new Claim(ClaimTypes.Role, rol)
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(8),
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}