using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CarslineApp.Models;
using CarslineApp.Services;

namespace CarslineApp.ViewModels.Modules
{
    /// <summary>
    /// Módulo independiente para gestión de órdenes de servicio
    /// Maneja: tipos de servicio, servicios extra, cálculo de costos y creación de órdenes
    /// </summary>
    public class OrdenModule : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;
        private readonly int _tipoOrdenId;

        // Estado del módulo
        private bool _isLoading;
        private string _errorMessage = string.Empty;

        // Datos de la orden
        private int _kilometrajeActual;
        private DateTime _fechaHoraPromesa = DateTime.Now.AddHours(4);
        private string _observaciones = string.Empty;
        private TipoServicioDto _tipoServicioSeleccionado;
        private ObservableCollection<TipoServicioDto> _tiposServicio = new();
        private ObservableCollection<ServicioExtraDto> _serviciosFrecuentes = new();
        private decimal _costoTotal;

        public OrdenModule(int tipoOrdenId, ApiService apiService = null)
        {
            _tipoOrdenId = tipoOrdenId;
            _apiService = apiService ?? new ApiService();
        }

        #region Propiedades Públicas

        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(); }
        }

        public int KilometrajeActual
        {
            get => _kilometrajeActual;
            set { _kilometrajeActual = value; OnPropertyChanged(); }
        }

        public DateTime FechaHoraPromesa
        {
            get => _fechaHoraPromesa;
            set { _fechaHoraPromesa = value; OnPropertyChanged(); }
        }

        public string Observaciones
        {
            get => _observaciones;
            set { _observaciones = value; OnPropertyChanged(); }
        }

        public TipoServicioDto TipoServicioSeleccionado
        {
            get => _tipoServicioSeleccionado;
            set
            {
                _tipoServicioSeleccionado = value;
                OnPropertyChanged();
                CalcularCostoTotal();
            }
        }

        public ObservableCollection<TipoServicioDto> TiposServicio
        {
            get => _tiposServicio;
            set { _tiposServicio = value; OnPropertyChanged(); }
        }

        public ObservableCollection<ServicioExtraDto> ServiciosFrecuentes
        {
            get => _serviciosFrecuentes;
            set { _serviciosFrecuentes = value; OnPropertyChanged(); }
        }

        public decimal CostoTotal
        {
            get => _costoTotal;
            set
            {
                _costoTotal = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CostoTotalFormateado));
            }
        }

        public string CostoTotalFormateado => $"${CostoTotal:N2}";

        // Propiedades para identificar el tipo de orden
        public bool EsServicio => _tipoOrdenId == 1;
        public bool EsDiagnostico => _tipoOrdenId == 2;
        public bool EsReparacion => _tipoOrdenId == 3;
        public bool EsGarantia => _tipoOrdenId == 4;

        // Propiedad calculada para la fecha mínima (hoy)
        public DateTime FechaMinima => DateTime.Now.Date;

        #endregion

        #region Métodos Públicos

        /// <summary>
        /// Inicializar catálogos (tipos de servicio y servicios extra)
        /// Debe llamarse al crear el módulo
        /// </summary>
        public async Task InicializarAsync()
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                System.Diagnostics.Debug.WriteLine("🔄 Cargando catálogos de orden...");

                // Cargar tipos de servicio
                var tipos = await _apiService.ObtenerTiposServicioAsync();
                TiposServicio.Clear();
                foreach (var tipo in tipos)
                {
                    TiposServicio.Add(tipo);
                }

                System.Diagnostics.Debug.WriteLine($"✅ Cargados {TiposServicio.Count} tipos de servicio");

                // Cargar servicios extra/frecuentes
                var extras = await _apiService.ObtenerServiciosFrecuentesAsync();
                ServiciosFrecuentes.Clear();
                foreach (var extra in extras)
                {
                    // Asegurarse de que inician deseleccionados
                    extra.Seleccionado = false;
                    ServiciosFrecuentes.Add(extra);
                }

                System.Diagnostics.Debug.WriteLine($"✅ Cargados {ServiciosFrecuentes.Count} servicios extra");
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error al cargar catálogos: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"❌ Error cargando catálogos: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Crear orden completa con todos los datos
        /// </summary>
        public async Task<(bool Success, string NumeroOrden, decimal CostoTotal, string Message)> CrearOrdenAsync(
            int clienteId,
            int vehiculoId,
            int kilometrajeInicial)
        {
            if (!Validar(kilometrajeInicial))
            {
                return (false, null, 0, ErrorMessage);
            }

            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                System.Diagnostics.Debug.WriteLine($"🔄 Creando orden...");
                System.Diagnostics.Debug.WriteLine($"   Cliente ID: {clienteId}");
                System.Diagnostics.Debug.WriteLine($"   Vehículo ID: {vehiculoId}");
                System.Diagnostics.Debug.WriteLine($"   Tipo Orden: {_tipoOrdenId}");
                System.Diagnostics.Debug.WriteLine($"   Tipo Servicio: {TipoServicioSeleccionado?.Nombre}");

                // Obtener IDs de servicios extra seleccionados
                var serviciosSeleccionados = ServiciosFrecuentes
                    .Where(s => s.Seleccionado)
                    .Select(s => s.Id)
                    .ToList();

                System.Diagnostics.Debug.WriteLine($"   Servicios extra: {serviciosSeleccionados.Count}");

                var request = new CrearOrdenConTrabajosRequest
                {
                    TipoOrdenId = _tipoOrdenId,
                    ClienteId = clienteId,
                    VehiculoId = vehiculoId,
                    //TipoServicioId = TipoServicioSeleccionado.Id,
                    KilometrajeActual = KilometrajeActual,
                    FechaHoraPromesaEntrega = FechaHoraPromesa,
                    ObservacionesAsesor = Observaciones,
                    //ServiciosExtraIds = serviciosSeleccionados
                };

                // Obtener ID del asesor desde preferencias
                int asesorId = Preferences.Get("user_id", 0);

                if (asesorId == 0)
                {
                    ErrorMessage = "No se pudo identificar al asesor. Por favor, inicia sesión nuevamente.";
                    return (false, null, 0, ErrorMessage);
                }

                System.Diagnostics.Debug.WriteLine($"   Asesor ID: {asesorId}");
                /*
                var response = await _apiService.CrearOrdenCompletaAsync(request, asesorId);

                if (response.Success)
                {
                    System.Diagnostics.Debug.WriteLine($"✅ Orden creada: {response.NumeroOrden}");
                    return (true, response.NumeroOrden, response.CostoTotal, $"Costo Total: ${response.CostoTotal:N2}");
                }
                else
                {
                    ErrorMessage = response.Message;
                    System.Diagnostics.Debug.WriteLine($"❌ Error: {response.Message}");
                    return (false, null, 0, response.Message);
                }*/
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error al crear orden: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"❌ Excepción: {ex.Message}");
                return (false, null, 0, ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Calcular costo total de la orden (tipo de servicio + servicios extra)
        /// </summary>
        public void CalcularCostoTotal()
        {
            decimal total = 0;

            // Sumar tipo de servicio principal
            if (TipoServicioSeleccionado != null)
            {
                total += TipoServicioSeleccionado.Precio;
            }

            // Sumar servicios extra seleccionados
            total += ServiciosFrecuentes
                .Where(s => s.Seleccionado)
                .Sum(s => s.Precio);

            CostoTotal = total;

            System.Diagnostics.Debug.WriteLine($"💰 Costo total calculado: ${CostoTotal:N2}");
        }

        /// <summary>
        /// Validar datos de la orden antes de crear
        /// </summary>
        public bool Validar(int kilometrajeInicial)
        {
            if (TipoServicioSeleccionado == null)
            {
                ErrorMessage = "Selecciona un tipo de servicio";
                return false;
            }

            if (KilometrajeActual <= 0)
            {
                ErrorMessage = "Ingresa el kilometraje actual";
                return false;
            }

            if (KilometrajeActual < kilometrajeInicial)
            {
                ErrorMessage = "El kilometraje actual no puede ser menor al inicial";
                return false;
            }

            if (FechaHoraPromesa < DateTime.Now)
            {
                ErrorMessage = "La fecha de promesa no puede ser anterior a la fecha actual";
                return false;
            }

            // Validación adicional: fecha de promesa no debe ser muy lejana (ej: más de 30 días)
            if (FechaHoraPromesa > DateTime.Now.AddDays(30))
            {
                ErrorMessage = "La fecha de promesa no puede ser mayor a 30 días";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Limpiar todos los datos de la orden
        /// </summary>
        public void Limpiar()
        {
            KilometrajeActual = 0;
            FechaHoraPromesa = DateTime.Now.AddHours(4);
            Observaciones = string.Empty;
            TipoServicioSeleccionado = null;
            CostoTotal = 0;
            ErrorMessage = string.Empty;

            // Deseleccionar todos los servicios extra
            foreach (var servicio in ServiciosFrecuentes)
            {
                servicio.Seleccionado = false;
            }

            System.Diagnostics.Debug.WriteLine("🧹 Módulo de orden limpiado");
        }

        /// <summary>
        /// Obtener resumen de la orden para confirmación
        /// </summary>
        public string ObtenerResumen()
        {
            if (TipoServicioSeleccionado == null)
            {
                return "Orden sin configurar";
            }

            var serviciosExtra = ServiciosFrecuentes
                .Where(s => s.Seleccionado)
                .Select(s => s.Nombre)
                .ToList();

            string resumen = $"Servicio: {TipoServicioSeleccionado.Nombre}\n";
            resumen += $"Kilometraje: {KilometrajeActual:N0} km\n";
            resumen += $"Fecha promesa: {FechaHoraPromesa:dd/MM/yyyy HH:mm}\n";

            if (serviciosExtra.Any())
            {
                resumen += $"Servicios extra: {string.Join(", ", serviciosExtra)}\n";
            }

            resumen += $"Costo total: {CostoTotalFormateado}";

            return resumen;
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}