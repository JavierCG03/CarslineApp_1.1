
namespace CarslineApp.Models
{
    // ============================================
    // ✅ MODELOS DE ÓRDENES - ACTUALIZADOS
    // ============================================

    /// <summary>
    /// DTO simplificado para lista de órdenes (dashboard)
    /// </summary>
    public class OrdenDetalladaDto
    {
        public int Id { get; set; }
        public string NumeroOrden { get; set; } = string.Empty;
        public string VehiculoCompleto { get; set; } = string.Empty;
        public string ClienteNombre { get; set; } = string.Empty;
        public string ClienteTelefono { get; set; } = string.Empty;
        public string HoraPromesa { get; set; } = string.Empty;
        public string HoraInicio { get; set; } = string.Empty;
        public string HoraFin { get; set; } = string.Empty;
        public string NombreTecnico { get; set; } = string.Empty;
        public decimal CostoTotal { get; set; }
        public int EstadoId { get; set; }

        // ✅ NUEVOS CAMPOS
        public int TotalTrabajos { get; set; }
        public int TrabajosCompletados { get; set; }
        public decimal ProgresoGeneral { get; set; }

        // Propiedades calculadas
        public bool EsPendiente => EstadoId == 1;
        public bool EsProceso => EstadoId == 2;
        public bool EsFinalizada => EstadoId == 3;

        // ✅ NUEVAS PROPIEDADES CALCULADAS
        public string ProgresoTexto => $"{TrabajosCompletados}/{TotalTrabajos}";
        public string ProgresoFormateado => $"{ProgresoGeneral:F1}%";
        public bool TieneTrabajos => TotalTrabajos > 0;

        // Color de progreso
        public Color ColorProgreso
        {
            get
            {
                if (ProgresoGeneral >= 100) return Color.FromArgb("#4CAF50"); // Verde
                if (ProgresoGeneral >= 50) return Color.FromArgb("#FF9800");  // Naranja
                return Color.FromArgb("#2196F3"); // Azul
            }
        }
    }

    /// <summary>
    /// DTO completo de orden con trabajos (vista detalle)
    /// </summary>
    public class OrdenConTrabajosDto
    {
        public int Id { get; set; }
        public string NumeroOrden { get; set; } = string.Empty;
        public int TipoOrdenId { get; set; }
        public string TipoOrden { get; set; } = string.Empty;
        public string ClienteNombre { get; set; } = string.Empty;
        public string ClienteTelefono { get; set; } = string.Empty;
        public string VehiculoCompleto { get; set; } = string.Empty;
        public string VIN { get; set; } = string.Empty;
        public string Placas { get; set; } = string.Empty;
        public string AsesorNombre { get; set; } = string.Empty;
        public int KilometrajeActual { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime FechaHoraPromesaEntrega { get; set; }
        public int EstadoOrdenId { get; set; }
        public string EstadoOrden { get; set; } = string.Empty;
        public decimal CostoTotal { get; set; }
        public int TotalTrabajos { get; set; }
        public int TrabajosCompletados { get; set; }
        public decimal ProgresoGeneral { get; set; }
        public string? ObservacionesAsesor { get; set; }

        // Lista de trabajos
        public List<TrabajoDto> Trabajos { get; set; } = new();

        // Propiedades calculadas
        public string ProgresoTexto => $"{TrabajosCompletados}/{TotalTrabajos}";
        public string ProgresoFormateado => $"{ProgresoGeneral:F1}%";
        public bool TieneTrabajosEnProceso => Trabajos.Any(t => t.EnProceso);
        public bool TieneTrabajosCompletados => Trabajos.Any(t => t.EstaCompletado);
        public bool TieneTrabajos => Trabajos.Any();
    }

    /// <summary>
    /// Request para crear orden con trabajos
    /// </summary>
    public class CrearOrdenConTrabajosRequest
    {
        public int TipoOrdenId { get; set; }
        public int ClienteId { get; set; }
        public int VehiculoId { get; set; }
        public int KilometrajeActual { get; set; }
        public DateTime FechaHoraPromesaEntrega { get; set; }
        public string? ObservacionesAsesor { get; set; }
        public List<string> Trabajos { get; set; } = new();
    }

    // ============================================
    // MODELOS DE SERVICIOS (REFERENCIA)
    // ============================================

    public class TipoServicioDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public string PrecioFormateado => $"${Precio:N2}";
    }

    public class ServicioExtraDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public string Categoria { get; set; } = string.Empty;
        public bool Seleccionado { get; set; }
        public string PrecioFormateado => $"${Precio:N2}";
    }

    // ============================================
    // RESPUESTAS DE ÓRDENES
    // ============================================

    public class CrearOrdenResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string NumeroOrden { get; set; } = string.Empty;
        public int OrdenId { get; set; }
        public int TotalTrabajos { get; set; }
        public decimal CostoTotal { get; set; }
    }
}
