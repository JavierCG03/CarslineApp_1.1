using System.Collections.ObjectModel;
using CarslineApp.Models;
using Microsoft.Maui.Controls;

namespace CarslineApp.ViewModels
{
    /// <summary>
    /// Partial class para gestión de órdenes y servicios
    /// </summary>
    public partial class CrearOrdenViewModel
    {
        #region Campos Privados Orden

        private int _kilometrajeActual;
        private DateTime _fechaHoraPromesa = DateTime.Now.AddHours(5);
        private string _observaciones = string.Empty;
        private TipoServicioDto _tipoServicioSeleccionado;
        private ObservableCollection<TipoServicioDto> _tiposServicio = new();
        private ObservableCollection<ServicioExtraDto> _serviciosExtra = new();
        private decimal _costoTotal;
        #endregion

        #region Propiedades Orden

        public int KilometrajeActual
        {
            get => _kilometrajeActual;
            set
            {
                _kilometrajeActual = value;
                OnPropertyChanged();

                // Recalcular servicio subsecuente cuando cambie el kilometraje
                if (TieneHistorial && value > 0)
                {
                    CalcularServicioSubsecuente();
                }
            }
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

        public ObservableCollection<ServicioExtraDto> ServiciosExtra
        {
            get => _serviciosExtra;
            set { _serviciosExtra = value; OnPropertyChanged(); }
        }

        public decimal CostoTotal
        {
            get => _costoTotal;
            set { _costoTotal = value; OnPropertyChanged(); OnPropertyChanged(nameof(CostoTotalFormateado)); }
        }

        public string CostoTotalFormateado => $"${CostoTotal:N2}";

        #endregion

        #region Propiedades de Historial

        private ObservableCollection<HistorialServicioDto> _historialServicios = new();
        private bool _isLoadingHistorial;
        private bool _tieneHistorial;
        private string _resumenHistorial = string.Empty;

        /// <summary>
        /// Historial de servicios del vehículo
        /// </summary>
        public ObservableCollection<HistorialServicioDto> HistorialServicios
        {
            get => _historialServicios;
            set { _historialServicios = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Indicador de carga del historial
        /// </summary>
        public bool IsLoadingHistorial
        {
            get => _isLoadingHistorial;
            set { _isLoadingHistorial = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Indica si el vehículo tiene historial
        /// </summary>
        public bool TieneHistorial
        {
            get => _tieneHistorial;
            set { _tieneHistorial = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Resumen del historial (ej: "3 servicios - Promedio: $1,200")
        /// </summary>
        public string ResumenHistorial
        {
            get => _resumenHistorial;
            set { _resumenHistorial = value; OnPropertyChanged(); }
        }

        #endregion

        #region Propiedades de Servicio Subsecuente

        private string _servicioSugerido = string.Empty;
        private string _mensajeServicioSugerido = string.Empty;
        private string _colorServicioSugerido = "#4CAF50";

        /// <summary>
        /// Tipo de servicio sugerido (1er, 2do, 3er o Externo)
        /// </summary>
        public string ServicioSugerido
        {
            get => _servicioSugerido;
            set { _servicioSugerido = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Mensaje explicando por qué se sugiere ese servicio
        /// </summary>
        public string MensajeServicioSugerido
        {
            get => _mensajeServicioSugerido;
            set { _mensajeServicioSugerido = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Color del indicador de servicio sugerido
        /// </summary>
        public string ColorServicioSugerido
        {
            get => _colorServicioSugerido;
            set { _colorServicioSugerido = value; OnPropertyChanged(); }
        }

        #endregion

        #region Métodos de Historial

        /// <summary>
        /// Cargar historial de servicios del vehículo
        /// </summary>
        private async Task CargarHistorialVehiculo()
        {
            if (VehiculoId <= 0) return;

            IsLoadingHistorial = true;
            TieneHistorial = false;
            HistorialServicios.Clear();

            try
            {
                System.Diagnostics.Debug.WriteLine($"🔍 Cargando historial para vehículo ID: {VehiculoId}");

                var response = await _apiService.ObtenerHistorialVehiculoAsync(VehiculoId);

                if (response.Success && response.Historial != null && response.Historial.Any())
                {
                    foreach (var servicio in response.Historial)
                    {
                        HistorialServicios.Add(servicio);
                    }
                    TieneHistorial = true;

                    // Actualizar resumen con el último servicio
                    var ultimoServicio = response.Historial.First();
                    ResumenHistorial = $"Último servicio: {ultimoServicio.TipoServicio} / ({ultimoServicio.KilometrajeRegistrado:N0} km - {ultimoServicio.FechaFormateada}) ";

                    // Calcular servicio subsecuente si hay kilometraje actual
                    if (KilometrajeActual > 0)
                    {
                        CalcularServicioSubsecuente();
                    }
                }
                else
                {
                    TieneHistorial = false;
                    ResumenHistorial = "Sin historial de servicios";

                    // Sin historial = Servicio Externo
                    ServicioSugerido = "🔧 SERVICIO EXTERNO";
                    MensajeServicioSugerido = "Sin historial de servicios previos";
                    ColorServicioSugerido = "#FF9800";

                    System.Diagnostics.Debug.WriteLine("ℹ️ No se encontró historial");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error al cargar historial: {ex.Message}");
                TieneHistorial = false;
                ResumenHistorial = "Error al cargar historial";

                // En caso de error, sugerir servicio externo
                ServicioSugerido = "🔧 SERVICIO EXTERNO";
                MensajeServicioSugerido = "No se pudo cargar el historial";
                ColorServicioSugerido = "#FF5252";
            }
            finally
            {
                IsLoadingHistorial = false;
            }
        }

        #endregion

        #region Métodos de Cálculo de Servicio Subsecuente

        /// <summary>
        /// Calcular el servicio subsecuente basado en historial y kilometraje actual
        /// </summary>
        private void CalcularServicioSubsecuente()
        {
            // Si no hay historial, es servicio externo
            if (!TieneHistorial || !HistorialServicios.Any())
            {
                ServicioSugerido = "🔧 SERVICIO EXTERNO";
                MensajeServicioSugerido = "Sin historial de servicios previos";
                ColorServicioSugerido = "#FF9800";
                return;
            }

            // Obtener el último servicio
            var ultimoServicio = HistorialServicios.First();

            // Calcular diferencias
            int kmRecorridos = KilometrajeActual - ultimoServicio.KilometrajeRegistrado;
            DateTime fechaActual = DateTime.Now;
            TimeSpan tiempoTranscurrido = fechaActual - ultimoServicio.FechaServicio;
            int mesesTranscurridos = (int)(tiempoTranscurrido.TotalDays / 30);

            // Determinar el tipo de servicio anterior
            string tipoServicioAnterior = ultimoServicio.TipoServicio.ToUpper();

            // Validar si se excedieron los límites (6000 km o 5 meses)
            bool excedioKilometraje = kmRecorridos > 6000;
            bool excedioTiempo = mesesTranscurridos > 5;

            if (excedioKilometraje || excedioTiempo)
            {
                // Se excedieron los límites, servicio externo
                ServicioSugerido = "🔧 SERVICIO EXTERNO";
                ColorServicioSugerido = "#FF9800";

                List<string> razones = new List<string>();
                if (excedioKilometraje)
                    razones.Add($"Excedió los 6,000 km ({kmRecorridos:N0} km recorridos)");
                if (excedioTiempo)
                    razones.Add($"Excedió los 5 meses ({mesesTranscurridos} meses transcurridos)");

                MensajeServicioSugerido = $"⚠️ {string.Join(" y ", razones)}";

                System.Diagnostics.Debug.WriteLine($"   ❌ Servicio EXTERNO: {MensajeServicioSugerido}");
            }
            else
            {
                // Dentro de los límites, determinar servicio subsecuente
                string servicioSubsecuente = DeterminarServicioSubsecuente(tipoServicioAnterior);

                if (servicioSubsecuente == "EXTERNO")
                {
                    ServicioSugerido = "🔧 SERVICIO EXTERNO";
                    ColorServicioSugerido = "#FF9800";
                    MensajeServicioSugerido = "✅ Ya completó los 3 servicios programados";
                }
                else
                {
                    ServicioSugerido = $"✨ {servicioSubsecuente}";
                    ColorServicioSugerido = "#4CAF50";
                    MensajeServicioSugerido = $"✅ En tiempo y forma ({kmRecorridos:N0} km / {mesesTranscurridos} meses desde último servicio)";
                }

                System.Diagnostics.Debug.WriteLine($"   ✅ Servicio sugerido: {ServicioSugerido}");
            }
        }

        /// <summary>
        /// Determinar el servicio subsecuente basado en el servicio anterior
        /// </summary>
        private string DeterminarServicioSubsecuente(string servicioAnterior)
        {
            // Normalizar el nombre del servicio
            if (servicioAnterior.Contains("1") || servicioAnterior.Contains("PRIMER"))
                return "2DO SERVICIO";

            if (servicioAnterior.Contains("2") || servicioAnterior.Contains("SEGUNDO"))
                return "3ER SERVICIO";

            if (servicioAnterior.Contains("3") || servicioAnterior.Contains("TERCER"))
                return "EXTERNO";

            // Si no es ninguno de los anteriores, es externo
            return "EXTERNO";
        }

        #endregion

        #region Métodos de Creación de Orden
        private async Task CrearOrdenReparacion()
        {
            IsLoading = true;
            ErrorMessage = string.Empty;
            try
            {
                var trabajos = new List<string>();
                var serviciosSeleccionados = ServiciosExtra.Where(s => s.Seleccionado).ToList();
                foreach (var servicio in serviciosSeleccionados)
                {
                    trabajos.Add($"{servicio.Nombre} - ${servicio.Precio:N2}");
                }
                var request = new CrearOrdenConTrabajosRequest
                {
                    TipoOrdenId = _tipoOrdenId,
                    ClienteId = ClienteId,
                    VehiculoId = VehiculoId,
                    TipoServicioId = TipoServicioSeleccionado.Id,
                    KilometrajeActual = KilometrajeActual,
                    FechaHoraPromesaEntrega = FechaHoraPromesa,
                    ObservacionesAsesor = Observaciones,
                    Trabajos = trabajos
                };
                int asesorId = Preferences.Get("user_id", 0);
                var response = await _apiService.CrearOrdenConTrabajosAsync(request, asesorId);
                if (response.Success)
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "¡Éxito!",
                        $"Orden {response.NumeroOrden} creada exitosamente\nCosto Total: ${response.CostoTotal:N2}",
                        "OK");

                    await Application.Current.MainPage.Navigation.PopAsync();
                    MessagingCenter.Send(this, "OrdenCreada");
                }
                else
                {
                    ErrorMessage = response.Message;
                }


            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task CrearOrdenServicio()
        {
            if (TipoServicioSeleccionado == null)
            {
                ErrorMessage = "Selecciona un tipo de servicio";
                return;
            }

            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                var trabajos = new List<string>();

                // 1. Agregar el servicio principal
                if (TipoServicioSeleccionado != null)
                {
                    trabajos.Add($"{TipoServicioSeleccionado.Nombre} - ${TipoServicioSeleccionado.Precio:N2}");
                }

                // 2. Agregar servicios extra seleccionados
                var serviciosSeleccionados = ServiciosExtra.Where(s => s.Seleccionado).ToList();
                foreach (var servicio in serviciosSeleccionados)
                {
                    trabajos.Add($"{servicio.Nombre} - ${servicio.Precio:N2}");
                }
                var request = new CrearOrdenConTrabajosRequest
                {
                    TipoOrdenId = _tipoOrdenId,
                    ClienteId = ClienteId,
                    VehiculoId = VehiculoId,
                    TipoServicioId = TipoServicioSeleccionado.Id,
                    KilometrajeActual = KilometrajeActual,
                    FechaHoraPromesaEntrega = FechaHoraPromesa,
                    ObservacionesAsesor = Observaciones,
                    Trabajos = trabajos
                };

                int asesorId = Preferences.Get("user_id", 0);
                var response = await _apiService.CrearOrdenConTrabajosAsync(request, asesorId);

                if (response.Success)
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "¡Éxito!",
                        $"Orden {response.NumeroOrden} creada exitosamente\nCosto Total: ${response.CostoTotal:N2}",
                        "OK");

                    await Application.Current.MainPage.Navigation.PopAsync();
                    MessagingCenter.Send(this, "OrdenCreada");
                }
                else
                {
                    ErrorMessage = response.Message;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        public void CalcularCostoTotal()
        {
            decimal total = 0;

            if (TipoServicioSeleccionado != null)
            {
                total += TipoServicioSeleccionado.Precio;
            }

            total += ServiciosExtra
                .Where(s => s.Seleccionado)
                .Sum(s => s.Precio);

            CostoTotal = total;
        }

        #endregion
    }
}