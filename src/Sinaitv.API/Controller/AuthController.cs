using Microsoft.AspNetCore.Mvc;
using Sinaitv.API.Models;
using Sinaitv.API.Services;

namespace Sinaitv.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto login)
        {
            if (string.IsNullOrEmpty(login.CodUsuario))
                return BadRequest(new { mensaje = "El código de usuario es obligatorio" });

            var session = await _authService.AuthenticateAsync(login.CodUsuario, login.Password);

            if (session == null)
                return Unauthorized(new { mensaje = "Usuario inváido o inactivo en Profit" });

            return Ok(session);
        }
    }
}