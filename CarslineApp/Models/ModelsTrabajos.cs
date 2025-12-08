
namespace CarslineApp.Models
{

    // ============================================
    // ✅ NUEVOS MODELOS: TRABAJOS
    // ============================================

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
