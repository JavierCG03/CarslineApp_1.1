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
        public string TipoServicio { get; set; } = string.Empty; // ✅ AGREGADO

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
        public int TipoServicioId { get; set; }
        public int KilometrajeActual { get; set; }
        public DateTime FechaHoraPromesaEntrega { get; set; }
        public string? ObservacionesAsesor { get; set; }
        public List<string> Trabajos { get; set; } = new();
    }

    /// <summary>
    /// ✅ AGREGADO: Response para crear orden con trabajos
    /// </summary>
    public class CrearOrdenResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string NumeroOrden { get; set; } = string.Empty;
        public int OrdenId { get; set; }
        public int TotalTrabajos { get; set; }
        public decimal CostoTotal { get; set; }
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
    public class TrabajoDto
    {
        public int Id { get; set; }
        public int OrdenGeneralId { get; set; }
        public string Trabajo { get; set; } = string.Empty;
        public int? TecnicoAsignadoId { get; set; }
        public string? TecnicoNombre { get; set; }
        public DateTime? FechaHoraAsignacionTecnico { get; set; }
        public DateTime? FechaHoraInicio { get; set; }
        public DateTime? FechaHoraTermino { get; set; }
        public string? DuracionFormateada { get; set; }
        public string? IncidenciasServicio { get; set; }
        public string? ComentariosTecnico { get; set; }
        public string? ComentariosJefeTaller { get; set; }
        public int EstadoTrabajo { get; set; }
        public string? EstadoTrabajoNombre { get; set; }
        public string? ColorEstado { get; set; }
        public DateTime FechaCreacion { get; set; }

        // Propiedades calculadas
        public bool EsPendiente => EstadoTrabajo == 1;
        public bool EnProceso => EstadoTrabajo == 2;
        public bool EstaCompletado => EstadoTrabajo == 3;
        public bool EstaPausado => EstadoTrabajo == 4;
        public bool EstaCancelado => EstadoTrabajo == 5;

        // Color visual según estado
        public Color ColorVisualEstado => EstadoTrabajo switch
        {
            1 => Color.FromArgb("#FFA500"), // Pendiente - Naranja
            2 => Color.FromArgb("#2196F3"), // En Proceso - Azul
            3 => Color.FromArgb("#4CAF50"), // Completado - Verde
            4 => Color.FromArgb("#FF9800"), // Pausado - Naranja oscuro
            5 => Color.FromArgb("#F44336"), // Cancelado - Rojo
            _ => Colors.Gray
        };
    }
}