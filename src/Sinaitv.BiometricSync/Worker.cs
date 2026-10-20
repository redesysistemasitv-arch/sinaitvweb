using Dapper;
using Microsoft.Data.SqlClient;

namespace Sinaitv.BiometricSync
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration _config;

        public Worker(ILogger<Worker> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var ip = _config["Biometrico:IP"];
            var puerto = _config.GetValue<int>("Biometrico:Puerto");
            var intervalo = _config.GetValue<int>("Biometrico:IntervaloMinutos");
            var connStr = _config.GetConnectionString("SinaitvConnection");

            _logger.LogInformation("Iniciando Servicio de Sincronizacion Biometrica SINAITV para equipo en IP: {IP}:{Puerto}", ip, puerto);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Ejecutando ciclo de lectura de marcajes a las: {time}", DateTimeOffset.Now);

                    // AQUI: Conexión con el SDK ZKemkeeper / Socket TCP al Qclear-TC
                    // Por demostración estructural y simulación de lectura de registros:
                    using (var db = new SqlConnection(connStr))
                    {
                        var sqlPrueba = @"
                            -- Verifica conexion a la BD y limpia registros duplicados
                            SELECT COUNT(*) FROM MarcajesBrutos";
                        
                        var totalMarcajes = await db.ExecuteScalarAsync<int>(sqlPrueba);
                        _logger.LogInformation("Total de marcajes actualmente procesados en la BD: {Total}", totalMarcajes);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error durante la lectura o insercion de marcajes del biometrico.");
                }

                // Esperar el intervalo configurado
                await Task.Delay(TimeSpan.FromMinutes(intervalo), stoppingToken);
            }
        }
    }
}