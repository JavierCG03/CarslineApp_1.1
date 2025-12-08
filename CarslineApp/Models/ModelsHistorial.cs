
namespace CarslineApp.Models
{

    // ============================================
    // HISTORIAL (SIN CAMBIOS MAYORES)
    // ============================================

    public class HistorialServicioDto
    {
        public string NumeroOrden { get; set; } = string.Empty;
        public DateTime FechaServicio { get; set; }
        public string TipoServicio { get; set; } = string.Empty;
        public int KilometrajeRegistrado { get; set; }
        public decimal CostoTotal { get; set; }
        public List<ServicioExtraHistorialDto> ServiciosExtra { get; set; } = new();
        public string ObservacionesAsesor { get; set; } = string.Empty;

        public string FechaFormateada => FechaServicio.ToString("dd/MMM/yyyy");
        public string CostoFormateado => $"${CostoTotal:N2}";
        public int TotalServiciosExtra => ServiciosExtra?.Count ?? 0;
        public bool TieneServiciosExtra => ServiciosExtra != null && ServiciosExtra.Any();
        public string ServiciosExtraResumen => TieneServiciosExtra
            ? $"{TotalServiciosExtra} extra(s): {string.Join(", ", ServiciosExtra.Take(2).Select(s => s.NombreServicio))}{(TotalServiciosExtra > 2 ? "..." : "")}"
            : "Sin servicios extra";

        public Color ColorFondo => TieneServiciosExtra ? Color.FromArgb("#E8F5E9") : Color.FromArgb("#F5F5F5");
    }

    public class ServicioExtraHistorialDto
    {
        public string NombreServicio { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public string PrecioFormateado => $"${Precio:N2}";
    }

    public class HistorialVehiculoResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<HistorialServicioDto> Historial { get; set; } = new();

        public int TotalServicios { get; set; }
        public decimal CostoPromedio { get; set; }
        public int UltimoKilometraje { get; set; }
        public DateTime? UltimaFechaServicio { get; set; }

        public string CostoPromedioFormateado => $"${CostoPromedio:N2}";
        public string UltimaFechaFormateada => UltimaFechaServicio?.ToString("dd/MMM/yyyy") ?? "Sin servicios";
        public bool TieneHistorial => Historial != null && Historial.Any();
        public string ResumenHistorial => TieneHistorial
            ? $"{TotalServicios} servicio(s) - Promedio: {CostoPromedioFormateado}"
            : "Este vehículo no tiene historial de servicios en los últimos 6 meses";
    }


}
