using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CarslineApp.Models;
using CarslineApp.Services;

namespace CarslineApp.ViewModels
{
    public class CrearOrdenViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;
        private readonly int _tipoOrdenId;

        private int _pasoActual = 1; // 1=Cliente, 2=Vehículo, 3=Orden
        private bool _isLoading;
        private string _errorMessage = string.Empty;

        // ✅ NUEVO: Para manejar listas de resultados
        private ObservableCollection<ClienteDto> _clientesEncontrados = new();
        private ObservableCollection<VehiculoDto> _vehiculosEncontrados = new();
        private bool _mostrarListaClientes;
        private bool _mostrarListaVehiculos;

        // DATOS DEL CLIENTE
        private int _clienteId;
        private string _nombreBusquedaCliente = string.Empty;
        private string _rfc = string.Empty;
        private string _nombreCompleto = string.Empty;
        private string _telefonoMovil = string.Empty;
        private string _telefonoCasa = string.Empty;
        private string _correoElectronico = string.Empty;
        private string _colonia = string.Empty;
        private string _calle = string.Empty;
        private string _numeroExterior = string.Empty;
        private string _municipio = string.Empty;
        private string _estado = string.Empty;
        private string _codigoPostal = string.Empty;

        // DATOS DEL VEHÍCULO
        private int _vehiculoId;
        private string _ultimos4VIN = string.Empty;
        private string _vin = string.Empty;
        private string _marca = string.Empty;
        private string _modelo = string.Empty;
        private string _version = string.Empty;
        private int _anio = DateTime.Now.Year;
        private string _color = string.Empty;
        private string _placas = string.Empty;
        private int _kilometrajeInicial;

        // DATOS DE LA ORDEN
        private int _kilometrajeActual;
        private DateTime _fechaHoraPromesa = DateTime.Now.AddHours(5);
        private string _observaciones = string.Empty;
        private TipoServicioDto _tipoServicioSeleccionado;
        private ObservableCollection<TipoServicioDto> _tiposServicio = new();
        private ObservableCollection<ServicioExtraDto> _serviciosExtra = new();
        private decimal _costoTotal;

        public CrearOrdenViewModel(int tipoOrdenId)
        {
            _tipoOrdenId = tipoOrdenId;
            _apiService = new ApiService();

            // Comandos
            BuscarClienteCommand = new Command(async () => await BuscarCliente());
            SeleccionarClienteCommand = new Command<ClienteDto>(async (cliente) => await SeleccionarCliente(cliente));
            BuscarVehiculoCommand = new Command(async () => await BuscarVehiculo());
            SeleccionarVehiculoCommand = new Command<VehiculoDto>(async (vehiculo) => await SeleccionarVehiculo(vehiculo));
            HabilitarEdicionClienteCommand = new Command(() => ModoEdicionCliente = true);
            HabilitarEdicionVehiculoCommand = new Command(() => ModoEdicionVehiculo = true);
            SiguienteCommand = new Command(async () => await Siguiente(), () => !IsLoading);
            AnteriorCommand = new Command(() => Anterior());
            CrearOrdenServicioCommand = new Command(async () => await CrearOrdenServicio(), () => !IsLoading);
            //CrearOrdenReparacionCommand = new Command(async () => await CrearOrdenReparacion(), () => !IsLoading);
            //CrearOrdenDiagnosticoCommand = new Command(async () => await CrearOrdenDiagnostico(), () => !IsLoading);
            //CrearOrdenGarantiaCommand = new Command(async () => await CrearOrdenGarantia(), () => !IsLoading);
            EditarGuardarClienteCommand = new Command(async () => await EditarGuardarCliente());
            EditarGuardarVehiculoCommand = new Command(async () => await EditarGuardarVehiculo());

            CargarCatalogos();

            OnPropertyChanged(nameof(EsServicio));
            OnPropertyChanged(nameof(EsDiagnostico));
            OnPropertyChanged(nameof(EsReparacion));
            OnPropertyChanged(nameof(EsGarantia));
        }
        #region Propiedades de Servicio Subsecuente - NUEVO

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

        #region Métodos de Cálculo de Servicio Subsecuente - NUEVO

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

            System.Diagnostics.Debug.WriteLine($"📊 Cálculo de servicio:");
            System.Diagnostics.Debug.WriteLine($"   Último servicio: {ultimoServicio.TipoServicio}");
            System.Diagnostics.Debug.WriteLine($"   Km anterior: {ultimoServicio.KilometrajeRegistrado:N0}");
            System.Diagnostics.Debug.WriteLine($"   Km actual: {KilometrajeActual:N0}");
            System.Diagnostics.Debug.WriteLine($"   Km recorridos: {kmRecorridos:N0}");
            System.Diagnostics.Debug.WriteLine($"   Meses transcurridos: {mesesTranscurridos}");

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

        #region Propiedades Generales

        public int PasoActual
        {
            get => _pasoActual;
            set
            {
                _pasoActual = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(MostrarPaso1));
                OnPropertyChanged(nameof(MostrarPaso2));
                OnPropertyChanged(nameof(MostrarPaso3));
                OnPropertyChanged(nameof(MostrarBotonSiguiente));
                OnPropertyChanged(nameof(MostrarBotonCrear));
                OnPropertyChanged(nameof(TituloPaso));
            }
        }

        public bool MostrarBotonEditarCliente => ClienteId > 0;
        public bool MostrarBotonEditarVehiculo => VehiculoId > 0;
        public bool MostrarPaso1 => PasoActual == 1;
        public bool MostrarPaso2 => PasoActual == 2;
        public bool MostrarPaso3 => PasoActual == 3;
        public bool MostrarBotonSiguiente => PasoActual < 3;
        public bool MostrarBotonCrear => PasoActual == 3;

        public bool CamposClienteBloqueados => ClienteId > 0 && !ModoEdicionCliente;
        public bool CampoPlacasBloqueado => VehiculoId > 0 && !ModoEdicionVehiculo;
        public bool CamposVehiculoBloqueados => VehiculoId > 0;

        public bool EsServicio => _tipoOrdenId == 1;
        public bool EsDiagnostico => _tipoOrdenId == 2;
        public bool EsReparacion => _tipoOrdenId == 3;
        public bool EsGarantia => _tipoOrdenId == 4;

        private bool _modoEdicionCliente;
        public bool ModoEdicionCliente
        {
            get => _modoEdicionCliente;
            set
            {
                _modoEdicionCliente = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TextoBotonCliente));
                OnPropertyChanged(nameof(ColorBotonCliente));
                OnPropertyChanged(nameof(CamposClienteBloqueados));
            }
        }

        public string TextoBotonCliente => ModoEdicionCliente ? "💾 Guardar Cambios" : "✏️ Editar";
        public string ColorBotonCliente => ModoEdicionCliente ? "#4CAF50" : "#FF9800";

        private bool _modoEdicionVehiculo;
        public bool ModoEdicionVehiculo
        {
            get => _modoEdicionVehiculo;
            set
            {
                _modoEdicionVehiculo = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TextoBotonVehiculo));
                OnPropertyChanged(nameof(ColorBotonVehiculo));
                OnPropertyChanged(nameof(CampoPlacasBloqueado));
            }
        }

        public string TextoBotonVehiculo => ModoEdicionVehiculo ? "💾 Guardar Placas" : "✏️ Editar Placas";
        public string ColorBotonVehiculo => ModoEdicionVehiculo ? "#4CAF50" : "#FF9800";

        public string TituloPaso => PasoActual switch
        {
            1 => "DATOS DEL CLIENTE",
            2 => "DATOS DEL VEHÍCULO",
            3 => _tipoOrdenId switch
            {
                1 => "DATOS DEL SERVICIO",
                2 => "DATOS DEL DIAGNÓSTICO",
                3 => "DATOS DE LA REPARACIÓN",
                4 => "DATOS DE LA GARANTÍA",
                _ => ""
            },
            _ => "CREAR ORDEN"
        };

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
                ((Command)SiguienteCommand).ChangeCanExecute();
                ((Command)CrearOrdenServicioCommand).ChangeCanExecute();
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(); }
        }

        #endregion

        #region Propiedades Cliente - ACTUALIZADAS

        // ✅ NUEVO: Lista de clientes encontrados
        public ObservableCollection<ClienteDto> ClientesEncontrados
        {
            get => _clientesEncontrados;
            set { _clientesEncontrados = value; OnPropertyChanged(); }
        }

        // ✅ NUEVO: Mostrar/ocultar lista
        public bool MostrarListaClientes
        {
            get => _mostrarListaClientes;
            set { _mostrarListaClientes = value; OnPropertyChanged(); }
        }

        // ✅ NUEVO: Campo de búsqueda por nombre
        public string NombreBusquedaCliente
        {
            get => _nombreBusquedaCliente;
            set { _nombreBusquedaCliente = value; OnPropertyChanged(); ErrorMessage = string.Empty; }
        }

        public int ClienteId
        {
            get => _clienteId;
            set
            {
                _clienteId = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(MostrarBotonEditarCliente));
                OnPropertyChanged(nameof(CamposClienteBloqueados));
            }
        }

        public string RFC
        {
            get => _rfc;
            set { _rfc = value.ToUpper(); OnPropertyChanged(); }
        }

        public string NombreCompleto
        {
            get => _nombreCompleto;
            set { _nombreCompleto = value; OnPropertyChanged(); ErrorMessage = string.Empty; }
        }

        public string TelefonoMovil
        {
            get => _telefonoMovil;
            set { _telefonoMovil = value; OnPropertyChanged(); ErrorMessage = string.Empty; }
        }

        public string TelefonoCasa
        {
            get => _telefonoCasa;
            set { _telefonoCasa = value; OnPropertyChanged(); }
        }

        public string CorreoElectronico
        {
            get => _correoElectronico;
            set { _correoElectronico = value; OnPropertyChanged(); }
        }

        public string Colonia
        {
            get => _colonia;
            set { _colonia = value; OnPropertyChanged(); }
        }

        public string Calle
        {
            get => _calle;
            set { _calle = value; OnPropertyChanged(); }
        }

        public string NumeroExterior
        {
            get => _numeroExterior;
            set { _numeroExterior = value; OnPropertyChanged(); }
        }

        public string Municipio
        {
            get => _municipio;
            set { _municipio = value; OnPropertyChanged(); }
        }

        public string Estado
        {
            get => _estado;
            set { _estado = value; OnPropertyChanged(); }
        }

        public string CodigoPostal
        {
            get => _codigoPostal;
            set { _codigoPostal = value; OnPropertyChanged(); }
        }

        #endregion

        #region Propiedades Vehículo - ACTUALIZADAS

        // ✅ NUEVO: Lista de vehículos encontrados
        public ObservableCollection<VehiculoDto> VehiculosEncontrados
        {
            get => _vehiculosEncontrados;
            set { _vehiculosEncontrados = value; OnPropertyChanged(); }
        }

        // ✅ NUEVO: Mostrar/ocultar lista
        public bool MostrarListaVehiculos
        {
            get => _mostrarListaVehiculos;
            set { _mostrarListaVehiculos = value; OnPropertyChanged(); }
        }

        // ✅ NUEVO: Campo de búsqueda por últimos 4 VIN
        public string Ultimos4VIN
        {
            get => _ultimos4VIN;
            set { _ultimos4VIN = value.ToUpper(); OnPropertyChanged(); ErrorMessage = string.Empty; }
        }

        public int VehiculoId
        {
            get => _vehiculoId;
            set
            {
                _vehiculoId = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(MostrarBotonEditarVehiculo));
                OnPropertyChanged(nameof(CampoPlacasBloqueado));
                OnPropertyChanged(nameof(CamposVehiculoBloqueados));
            }
        }

        public string VIN
        {
            get => _vin;
            set { _vin = value.ToUpper(); OnPropertyChanged(); ErrorMessage = string.Empty; }
        }

        public string Marca
        {
            get => _marca;
            set { _marca = value; OnPropertyChanged(); ErrorMessage = string.Empty; }
        }

        public string Modelo
        {
            get => _modelo;
            set { _modelo = value; OnPropertyChanged(); ErrorMessage = string.Empty; }
        }

        public string Version
        {
            get => _version;
            set { _version = value; OnPropertyChanged(); ErrorMessage = string.Empty; }
        }

        public int Anio
        {
            get => _anio;
            set { _anio = value; OnPropertyChanged(); }
        }

        public string Color
        {
            get => _color;
            set { _color = value; OnPropertyChanged(); }
        }

        public string Placas
        {
            get => _placas;
            set { _placas = value.ToUpper(); OnPropertyChanged(); }
        }

        public int KilometrajeInicial
        {
            get => _kilometrajeInicial;
            set { _kilometrajeInicial = value; OnPropertyChanged(); }
        }

        #endregion

        #region Propiedades Orden

        public int KilometrajeActual
        {
            get => _kilometrajeActual;
            set
            {
                _kilometrajeActual = value;
                OnPropertyChanged();

                // ✅ NUEVO: Recalcular servicio subsecuente cuando cambie el kilometraje
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

        #region Comandos

        public ICommand HabilitarEdicionClienteCommand { get; }
        public ICommand HabilitarEdicionVehiculoCommand { get; }
        public ICommand BuscarClienteCommand { get; }
        public ICommand SeleccionarClienteCommand { get; } // ✅ NUEVO
        public ICommand BuscarVehiculoCommand { get; }
        public ICommand SeleccionarVehiculoCommand { get; } // ✅ NUEVO
        public ICommand SiguienteCommand { get; }
        public ICommand AnteriorCommand { get; }
        public ICommand CrearOrdenCommand { get; }
        public ICommand CrearOrdenServicioCommand { get; }
        public ICommand CrearOrdenReparacionCommand { get; }
        public ICommand CrearOrdenDiagnosticoCommand { get; }
        public ICommand CrearOrdenGarantiaCommand { get; }
        public ICommand EditarGuardarClienteCommand { get; }
        public ICommand EditarGuardarVehiculoCommand { get; }

        #endregion

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // CONTINUACIÓN DE CrearOrdenViewModel - MÉTODOS

        #region Métodos de Búsqueda - ACTUALIZADOS

        /// <summary>
        /// ✅ NUEVO: Buscar clientes por nombre
        /// </summary>
        private async Task BuscarCliente()
        {
            if (ModoEdicionCliente)
            {
                await GuardarCambiosCliente();
                return;
            }

            if (string.IsNullOrWhiteSpace(NombreBusquedaCliente) || NombreBusquedaCliente.Length < 3)
            {
                ErrorMessage = "Ingresa al menos 3 caracteres del nombre";
                return;
            }

            IsLoading = true;
            ErrorMessage = string.Empty;
            MostrarListaClientes = false;

            try
            {
                var response = await _apiService.BuscarClientesPorNombreAsync(NombreBusquedaCliente);

                if (response.Success && response.Clientes != null && response.Clientes.Any())
                {
                    ClientesEncontrados.Clear();
                    foreach (var cliente in response.Clientes)
                    {
                        ClientesEncontrados.Add(cliente);
                    }

                    if (ClientesEncontrados.Count == 1)
                    {
                        // Si solo hay un resultado, cargarlo automáticamente
                        await SeleccionarCliente(ClientesEncontrados[0]);
                    }
                    else
                    {
                        // Si hay múltiples resultados, mostrar la lista
                        MostrarListaClientes = true;
                        ErrorMessage = $"Se encontraron {ClientesEncontrados.Count} clientes. Selecciona uno:";
                    }
                }
                else
                {
                    ErrorMessage = response.Message ?? "Cliente no encontrado. Puedes registrar uno nuevo.";
                    MostrarListaClientes = false;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error: {ex.Message}";
                MostrarListaClientes = false;
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// ✅ NUEVO: Seleccionar cliente de la lista
        /// </summary>
        private async Task SeleccionarCliente(ClienteDto clienteSeleccionado)
        {
            if (clienteSeleccionado == null) return;

            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                // Obtener datos completos del cliente
                var response = await _apiService.ObtenerClientePorIdAsync(clienteSeleccionado.Id);

                if (response.Success && response.Cliente != null)
                {
                    ClienteId = response.Cliente.Id;
                    NombreCompleto = response.Cliente.NombreCompleto;
                    RFC = response.Cliente.RFC;
                    TelefonoMovil = response.Cliente.TelefonoMovil;
                    TelefonoCasa = response.Cliente.TelefonoCasa ?? "";
                    CorreoElectronico = response.Cliente.CorreoElectronico ?? "";
                    Colonia = response.Cliente.Colonia ?? "";
                    Calle = response.Cliente.Calle ?? "";
                    NumeroExterior = response.Cliente.NumeroExterior ?? "";
                    Municipio = response.Cliente.Municipio ?? "";
                    Estado = response.Cliente.Estado ?? "";
                    CodigoPostal = response.Cliente.CodigoPostal ?? "";

                    MostrarListaClientes = false;

                    await Application.Current.MainPage.DisplayAlert(
                        "✅ Cliente Seleccionado",
                        $"Se han cargado los datos de {response.Cliente.NombreCompleto}",
                        "OK");
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

        /// <summary>
        /// ✅ NUEVO: Buscar vehículos por últimos 4 dígitos del VIN
        /// </summary>
        private async Task BuscarVehiculo()
        {
            if (ModoEdicionVehiculo)
            {
                await GuardarCambiosVehiculo();
                return;
            }

            if (string.IsNullOrWhiteSpace(Ultimos4VIN) || Ultimos4VIN.Length != 4)
            {
                ErrorMessage = "Ingresa exactamente 4 caracteres del VIN";
                return;
            }

            IsLoading = true;
            ErrorMessage = string.Empty;
            MostrarListaVehiculos = false;

            try
            {
                var response = await _apiService.BuscarVehiculosPorUltimos4VINAsync(Ultimos4VIN);

                if (response.Success && response.Vehiculos != null && response.Vehiculos.Any())
                {
                    VehiculosEncontrados.Clear();
                    foreach (var vehiculo in response.Vehiculos)
                    {
                        VehiculosEncontrados.Add(vehiculo);
                    }

                    if (VehiculosEncontrados.Count == 1)
                    {
                        // Si solo hay un resultado, cargarlo automáticamente
                        await SeleccionarVehiculo(VehiculosEncontrados[0]);
                    }
                    else
                    {
                        // Si hay múltiples resultados, mostrar la lista
                        MostrarListaVehiculos = true;
                        ErrorMessage = $"Se encontraron {VehiculosEncontrados.Count} vehículos. Selecciona uno:";
                    }
                }
                else
                {
                    ErrorMessage = response.Message ?? "Vehículo no encontrado. Puedes registrar uno nuevo.";
                    MostrarListaVehiculos = false;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error: {ex.Message}";
                MostrarListaVehiculos = false;
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// ✅ NUEVO: Seleccionar vehículo de la lista
        /// </summary>
        private async Task SeleccionarVehiculo(VehiculoDto vehiculoSeleccionado)
        {
            if (vehiculoSeleccionado == null) return;

            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                // Obtener datos completos del vehículo
                var response = await _apiService.ObtenerVehiculoPorIdAsync(vehiculoSeleccionado.Id);

                if (response.Success && response.Vehiculo != null)
                {
                    VehiculoId = response.Vehiculo.Id;
                    VIN = response.Vehiculo.VIN;
                    Marca = response.Vehiculo.Marca;
                    Modelo = response.Vehiculo.Modelo;
                    Version = response.Vehiculo.Version;
                    Anio = response.Vehiculo.Anio;
                    Color = response.Vehiculo.Color;
                    Placas = response.Vehiculo.Placas;
                    KilometrajeInicial = response.Vehiculo.KilometrajeInicial;

                    MostrarListaVehiculos = false;

                    await Application.Current.MainPage.DisplayAlert(
                        "✅ Vehículo Seleccionado",
                        $"Se ha cargado: {response.Vehiculo.VehiculoCompleto}\nCliente: {response.Vehiculo.NombreCliente}",
                        "OK");
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

        #endregion

        #region Métodos de Edición

        private async Task EditarGuardarCliente()
        {
            if (!ModoEdicionCliente)
            {
                ModoEdicionCliente = true;
                return;
            }

            await GuardarCambiosCliente();
        }

        private async Task GuardarCambiosCliente()
        {
            if (!ValidarCliente()) return;

            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                var request = new ClienteRequest
                {
                    NombreCompleto = NombreCompleto,
                    RFC = RFC,
                    TelefonoMovil = TelefonoMovil,
                    TelefonoCasa = TelefonoCasa,
                    CorreoElectronico = CorreoElectronico,
                    Colonia = Colonia,
                    Calle = Calle,
                    NumeroExterior = NumeroExterior,
                    Municipio = Municipio,
                    Estado = Estado,
                    CodigoPostal = CodigoPostal
                };

                var response = await _apiService.ActualizarClienteAsync(ClienteId, request);

                if (response.Success)
                {
                    ModoEdicionCliente = false;

                    await Application.Current.MainPage.DisplayAlert(
                        "✅ Éxito",
                        "Los datos del cliente han sido actualizados correctamente",
                        "OK");
                }
                else
                {
                    ErrorMessage = response.Message;
                    await Application.Current.MainPage.DisplayAlert(
                        "❌ Error",
                        response.Message,
                        "OK");
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error: {ex.Message}";
                await Application.Current.MainPage.DisplayAlert(
                    "❌ Error",
                    $"Error al actualizar: {ex.Message}",
                    "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task EditarGuardarVehiculo()
        {
            if (!ModoEdicionVehiculo)
            {
                ModoEdicionVehiculo = true;
                return;
            }

            await GuardarCambiosVehiculo();
        }

        private async Task GuardarCambiosVehiculo()
        {
            if (string.IsNullOrWhiteSpace(Placas))
            {
                ErrorMessage = "Las placas son requeridas";
                await Application.Current.MainPage.DisplayAlert(
                    "⚠️ Advertencia",
                    "Debes ingresar las placas del vehículo",
                    "OK");
                return;
            }

            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                var response = await _apiService.ActualizarPlacasVehiculoAsync(VehiculoId, Placas);

                if (response.Success)
                {
                    ModoEdicionVehiculo = false;

                    await Application.Current.MainPage.DisplayAlert(
                        "✅ Éxito",
                        "Las placas han sido actualizadas correctamente",
                        "OK");
                }
                else
                {
                    ErrorMessage = response.Message;
                    await Application.Current.MainPage.DisplayAlert(
                        "❌ Error",
                        response.Message,
                        "OK");
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error: {ex.Message}";
                await Application.Current.MainPage.DisplayAlert(
                    "❌ Error",
                    $"Error al actualizar placas: {ex.Message}",
                    "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        #endregion

        #region Métodos de Navegación

        private void Anterior()
        {
            if (PasoActual > 1)
            {
                PasoActual--;

                // Ocultar listas al regresar
                if (PasoActual == 1)
                {
                    MostrarListaClientes = false;
                }
                else if (PasoActual == 2)
                {
                    MostrarListaVehiculos = false;
                }
            }
        }

        #endregion

        #region Validación y Creación de Orden

        private bool ValidarCliente()
        {
            if (string.IsNullOrWhiteSpace(NombreCompleto))
            {
                ErrorMessage = "El nombre completo es requerido";
                return false;
            }

            if (string.IsNullOrWhiteSpace(RFC) || RFC.Length < 12)
            {
                ErrorMessage = "El RFC es requerido (mínimo 12 caracteres)";
                return false;
            }

            if (string.IsNullOrWhiteSpace(TelefonoMovil))
            {
                ErrorMessage = "El teléfono móvil es requerido";
                return false;
            }

            return true;
        }

        private bool ValidarVehiculo()
        {
            if (string.IsNullOrWhiteSpace(VIN) || VIN.Length != 17)
            {
                ErrorMessage = "El VIN debe tener 17 caracteres";
                return false;
            }

            if (string.IsNullOrWhiteSpace(Marca))
            {
                ErrorMessage = "La marca es requerida";
                return false;
            }

            if (string.IsNullOrWhiteSpace(Modelo))
            {
                ErrorMessage = "El modelo es requerido";
                return false;
            }

            if (string.IsNullOrWhiteSpace(Version))
            {
                ErrorMessage = "La Version es requerida";
                return false;
            }

            if (Anio < 2000 || Anio > DateTime.Now.Year + 1)
            {
                ErrorMessage = "El año ingresado del vehiculo no es válido";
                return false;
            }

            if (KilometrajeInicial <= 0)
            {
                ErrorMessage = "Ingresa el kilometraje inicial";
                return false;
            }

            if (KilometrajeActual < KilometrajeInicial)
            {
                ErrorMessage = "El kilometraje Actual no puede ser menor al Inicial";
                return false;
            }

            return true;
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

                System.Diagnostics.Debug.WriteLine($"   Total trabajos: {trabajos.Count}");
                foreach (var trabajo in trabajos)
                {
                    System.Diagnostics.Debug.WriteLine($"      • {trabajo}");
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

        #region Otros Métodos

        private async void CargarCatalogos()
        {
            try
            {
                var tipos = await _apiService.ObtenerTiposServicioAsync();
                TiposServicio.Clear();
                foreach (var tipo in tipos)
                {
                    TiposServicio.Add(tipo);
                }

                var extras = await _apiService.ObtenerServiciosFrecuentesAsync();
                ServiciosExtra.Clear();
                foreach (var extra in extras)
                {
                    ServiciosExtra.Add(extra);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error al cargar catálogos: {ex.Message}";
            }
        }

        #endregion
        // AGREGAR ESTAS PROPIEDADES Y MÉTODOS A CrearOrdenViewModel

        #region Propiedades de Historial - NUEVO

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

        // MODIFICAR EL MÉTODO Siguiente() PARA CARGAR HISTORIAL AL LLEGAR AL PASO 3

        private async Task Siguiente()
        {
            ErrorMessage = string.Empty;

            if (PasoActual == 1)
            {
                if (ModoEdicionCliente)
                {
                    ErrorMessage = "Debes guardar los cambios del cliente antes de continuar";
                    await Application.Current.MainPage.DisplayAlert(
                        "⚠️ Atención",
                        "Por favor, guarda los cambios del cliente antes de continuar",
                        "OK");
                    return;
                }

                if (!ValidarCliente()) return;

                if (ClienteId == 0)
                {
                    IsLoading = true;
                    try
                    {
                        var request = new ClienteRequest
                        {
                            NombreCompleto = NombreCompleto,
                            RFC = RFC,
                            TelefonoMovil = TelefonoMovil,
                            TelefonoCasa = TelefonoCasa,
                            CorreoElectronico = CorreoElectronico,
                            Colonia = Colonia,
                            Calle = Calle,
                            NumeroExterior = NumeroExterior,
                            Municipio = Municipio,
                            Estado = Estado,
                            CodigoPostal = CodigoPostal
                        };

                        var response = await _apiService.CrearClienteAsync(request);

                        if (response.Success)
                        {
                            ClienteId = response.ClienteId;
                        }
                        else
                        {
                            ErrorMessage = response.Message;
                            return;
                        }
                    }
                    finally
                    {
                        IsLoading = false;
                    }
                }

                PasoActual = 2;
            }
            else if (PasoActual == 2)
            {
                if (ModoEdicionVehiculo)
                {
                    ErrorMessage = "Debes guardar los cambios de las placas antes de continuar";
                    await Application.Current.MainPage.DisplayAlert(
                        "⚠️ Atención",
                        "Por favor, guarda los cambios de las placas antes de continuar",
                        "OK");
                    return;
                }

                if (!ValidarVehiculo()) return;

                if (VehiculoId == 0)
                {
                    IsLoading = true;
                    try
                    {
                        var request = new VehiculoRequest
                        {
                            ClienteId = ClienteId,
                            VIN = VIN,
                            Marca = Marca,
                            Modelo = Modelo,
                            Version = Version,
                            Anio = Anio,
                            Color = Color,
                            Placas = Placas,
                            KilometrajeInicial = KilometrajeInicial
                        };

                        var response = await _apiService.CrearVehiculoAsync(request);

                        if (response.Success)
                        {
                            VehiculoId = response.VehiculoId;
                        }
                        else
                        {
                            ErrorMessage = response.Message;
                            return;
                        }
                    }
                    finally
                    {
                        IsLoading = false;
                    }
                }
                // ✅ NUEVO: Cargar historial al pasar al paso 3
                await CargarHistorialVehiculo();

                PasoActual = 3;
            }
        }

        #region Métodos de Historial - NUEVO

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

                    // ✅ NUEVO: Calcular servicio subsecuente si hay kilometraje actual
                    if (KilometrajeActual > 0)
                    {
                        CalcularServicioSubsecuente();
                    }
                }
                else
                {
                    TieneHistorial = false;
                    ResumenHistorial = "Sin historial de servicios";

                    // ✅ NUEVO: Sin historial = Servicio Externo
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


    }

}