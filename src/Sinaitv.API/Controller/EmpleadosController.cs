using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Dapper;
using Sinaitv.API.Models;

namespace Sinaitv.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Requiere estar autenticado
    public class EmpleadosController : ControllerBase
    {
        private readonly IConfiguration _config;

        public EmpleadosController(IConfiguration config)
        {
            _config = config;
        }

        // GET: api/empleados?empresa=TRANS_NNU
        [HttpGet]
        public async Task<IActionResult> GetEmpleados([FromQuery] string empresa = "TRANS_NNU")
        {
            var sinaitvConnStr = _config.GetConnectionString("SinaitvConnection");
            
            // Cadena dinámica a la BD de Nómina solicitada (TRANS_NNU, SERPRA_N)
            var nomBD = empresa.Trim().ToUpper();
            var nominaConnStr = $"Server=192.168.0.2;Database={nomBD};Integrated Security=True;TrustServerCertificate=True;";

            using var dbNomina = new SqlConnection(nominaConnStr);
            using var dbSinaitv = new SqlConnection(sinaitvConnStr);

            // Obtener empleados de Profit
            var sqlProfit = @"
                SELECT 
                    @CodEmpresa AS CodEmpresa,
                    LTRIM(RTRIM(cod_emp)) AS CodEmp,
                    LTRIM(RTRIM(ci)) AS Cedula,
                    LTRIM(RTRIM(nombre_completo)) AS NombreCompleto,
                    LTRIM(RTRIM(status)) AS Status
                FROM snemple
                WHERE status = 'A'";

            var empleados = (await dbNomina.QueryAsync<EmpleadoProfitDto>(sqlProfit, new { CodEmpresa = nomBD })).ToList();

            // Obtener mapeo actual desde SINAITVDB
            var sqlMapeo = "SELECT ID_Biometrico, Cod_Emp FROM MapeoBiometricoEmpleado WHERE Cod_Empresa = @CodEmpresa";
            var mapeos = (await dbSinaitv.QueryAsync<dynamic>(sqlMapeo, new { CodEmpresa = nomBD }))
                         .ToDictionary(m => (string)m.Cod_Emp.ToString().Trim(), m => (int)m.ID_Biometrico);

            foreach (var emp in empleados)
            {
                if (mapeos.TryGetValue(emp.CodEmp, out int idBio))
                {
                    emp.IdBiometrico = idBio;
                }
            }

            return Ok(empleados);
        }

        // POST: api/empleados/mapear (Solo Analista o Administrador)
        [HttpPost("mapear")]
        [Authorize(Roles = "Analista,Administrador")]
        public async Task<IActionResult> MapearEmpleado([FromBody] MapeoBiometricoRequest request)
        {
            var sinaitvConnStr = _config.GetConnectionString("SinaitvConnection");
            using var db = new SqlConnection(sinaitvConnStr);

            var sql = @"
                MERGE MapeoBiometricoEmpleado AS Target
                USING (SELECT @IdBiometrico AS ID_Biometrico, @CodEmpresa AS Cod_Empresa, @CodEmp AS Cod_Emp, @Cedula AS Cedula) AS Source
                ON (Target.Cod_Empresa = Source.Cod_Empresa AND Target.Cod_Emp = Source.Cod_Emp)
                WHEN MATCHED THEN
                    UPDATE SET Target.ID_Biometrico = Source.ID_Biometrico, Target.Cedula = Source.Cedula
                WHEN NOT MATCHED THEN
                    INSERT (ID_Biometrico, Cod_Empresa, Cod_Emp, Cedula)
                    VALUES (Source.ID_Biometrico, Source.Cod_Empresa, Source.Cod_Emp, Source.Cedula);";

            await db.ExecuteAsync(sql, request);

            return Ok(new { mensaje = "Mapeo guardado exitosamente" });
        }
    }
}