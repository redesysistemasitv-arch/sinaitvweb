using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Dapper;
using System.Security.Claims;
using Sinaitv.API.Models;

namespace Sinaitv.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class JustificacionesController : ControllerBase
    {
        private readonly IConfiguration _config;

        public JustificacionesController(IConfiguration config)
        {
            _config = config;
        }

        // GET: api/justificaciones (Accesible para todos los roles)
        [HttpGet]
        public async Task<IActionResult> GetJustificaciones([FromQuery] string codEmpresa, [FromQuery] string codEmp)
        {
            var sinaitvConnStr = _config.GetConnectionString("SinaitvConnection");
            using var db = new SqlConnection(sinaitvConnStr);

            var sql = @"
                SELECT 
                    JustificacionID, Cod_Empresa AS CodEmpresa, Cod_Emp AS CodEmp,
                    FechaInicio, FechaFin, Motivo, Cod_Usuario AS CodUsuario, FechaCreacion
                FROM Justificaciones
                WHERE Cod_Empresa = @CodEmpresa AND Cod_Emp = @CodEmp
                ORDER BY FechaInicio DESC";

            var lista = await db.QueryAsync<JustificacionDto>(sql, new { CodEmpresa = codEmpresa, CodEmp = codEmp });
            return Ok(lista);
        }

        // POST: api/justificaciones (RESTRINGIDO: Bloqueado para SoloLectura, permitido para Analista y Administrador)
        [HttpPost]
        [Authorize(Roles = "Analista,Administrador")]
        public async Task<IActionResult> CrearJustificacion([FromBody] CrearJustificacionRequest request)
        {
            var codUsuarioLogueado = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "ANONIMO";
            var sinaitvConnStr = _config.GetConnectionString("SinaitvConnection");

            using var db = new SqlConnection(sinaitvConnStr);

            var sqlInsert = @"
                INSERT INTO Justificaciones (Cod_Empresa, Cod_Emp, FechaInicio, FechaFin, Motivo, Cod_Usuario)
                VALUES (@CodEmpresa, @CodEmp, @FechaInicio, @FechaFin, @Motivo, @CodUsuario);";

            await db.ExecuteAsync(sqlInsert, new
            {
                request.CodEmpresa,
                request.CodEmp,
                request.FechaInicio,
                request.FechaFin,
                request.Motivo,
                CodUsuario = codUsuarioLogueado
            });

            // Registrar Log de Auditoría
            var sqlLog = @"
                INSERT INTO AuditoriaLogs (Cod_Usuario, Accion, TablaAfectada, Detalle)
                VALUES (@CodUsuario, 'INSERT', 'Justificaciones', @Detalle);";

            var detalle = $"Justificación creada para {request.CodEmp} ({request.CodEmpresa}) del {request.FechaInicio:yyyy-MM-dd} al {request.FechaFin:yyyy-MM-dd}";
            await db.ExecuteAsync(sqlLog, new { CodUsuario = codUsuarioLogueado, Detalle = detalle });

            return Ok(new { mensaje = "Justificación registrada correctamente" });
        }
    }
}