namespace Sinaitv.API.Models
{
    public class EmpleadoProfitDto
    {
        public string CodEmpresa { get; set; } = string.Empty;
        public string CodEmp { get; set; } = string.Empty;
        public string Cedula { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int? IdBiometrico { get; set; }
    }

    public class MapeoBiometricoRequest
    {
        public int IdBiometrico { get; set; }
        public string CodEmpresa { get; set; } = string.Empty;
        public string CodEmp { get; set; } = string.Empty;
        public string Cedula { get; set; } = string.Empty;
    }

    public class JustificacionDto
    {
        public int JustificacionID { get; set; }
        public string CodEmpresa { get; set; } = string.Empty;
        public string CodEmp { get; set; } = string.Empty;
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public string Motivo { get; set; } = string.Empty;
        public string CodUsuario { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
    }

    public class CrearJustificacionRequest
    {
        public string CodEmpresa { get; set; } = string.Empty;
        public string CodEmp { get; set; } = string.Empty;
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public string Motivo { get; set; } = string.Empty;
    }
}