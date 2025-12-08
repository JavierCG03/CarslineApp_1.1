using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CarslineApp.Models;
using CarslineApp.Services;

namespace CarslineApp.ViewModels.Modules
{
    /// <summary>
    /// Módulo independiente para gestión de vehículos
    /// Maneja: búsqueda, creación, actualización y validación de vehículos
    /// </summary>
    public class VehiculoModule : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;

        // Estado del módulo
        private bool _isLoading;
        private string _errorMessage = string.Empty;
        private bool _modoEdicion;

        // Búsqueda
        private string _ultimos4VIN = string.Empty;
        private ObservableCollection<VehiculoDto> _vehiculosEncontrados = new();
        private bool _mostrarLista;

        // Datos del vehículo
        private int _vehiculoId;
        private int _clienteId; // Requerido para crear vehículo
        private string _vin = string.Empty;
        private string _marca = string.Empty;
        private string _modelo = string.Empty;
        private string _version = string.Empty;
        private int _anio = DateTime.Now.Year;
        private string _color = string.Empty;
        private string _placas = string.Empty;
        private int _kilometrajeInicial;

        public VehiculoModule(ApiService apiService = null)
        {
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

        public bool ModoEdicion
        {
            get => _modoEdicion;
            set
            {
                _modoEdicion = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TextoBotonAccion));
                OnPropertyChanged(nameof(ColorBotonAccion));
                OnPropertyChanged(nameof(CampoPlacasBloqueado));
            }
        }

        public string Ultimos4VIN
        {
            get => _ultimos4VIN;
            set { _ultimos4VIN = value.ToUpper(); OnPropertyChanged(); ErrorMessage = string.Empty; }
        }

        public ObservableCollection<VehiculoDto> VehiculosEncontrados
        {
            get => _vehiculosEncontrados;
            set { _vehiculosEncontrados = value; OnPropertyChanged(); }
        }

        public bool MostrarLista
        {
            get => _mostrarLista;
            set { _mostrarLista = value; OnPropertyChanged(); }
        }

        public int VehiculoId
        {
            get => _vehiculoId;
            set
            {
                _vehiculoId = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(EsVehiculoExistente));
                OnPropertyChanged(nameof(CamposVehiculoBloqueados));
                OnPropertyChanged(nameof(CampoPlacasBloqueado));
                OnPropertyChanged(nameof(MostrarBotonEditar));
            }
        }

        public int ClienteId
        {
            get => _clienteId;
            set { _clienteId = value; OnPropertyChanged(); }
        }

        public string VIN
        {
            get => _vin;
            set { _vin = value.ToUpper(); OnPropertyChanged(); }
        }

        public string Marca
        {
            get => _marca;
            set { _marca = value; OnPropertyChanged(); }
        }

        public string Modelo
        {
            get => _modelo;
            set { _modelo = value; OnPropertyChanged(); }
        }

        public string Version
        {
            get => _version;
            set { _version = value; OnPropertyChanged(); }
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

        // Propiedades calculadas
        public bool EsVehiculoExistente => VehiculoId > 0;
        public bool CamposVehiculoBloqueados => EsVehiculoExistente;
        public bool CampoPlacasBloqueado => EsVehiculoExistente && !ModoEdicion;
        public bool MostrarBotonEditar => EsVehiculoExistente;
        public string TextoBotonAccion => ModoEdicion ? "💾 Guardar Placas" : "✏️ Editar Placas";
        public string ColorBotonAccion => ModoEdicion ? "#4CAF50" : "#FF9800";

        // Información completa del vehículo para display
        public string VehiculoCompleto => string.IsNullOrEmpty(Marca)
            ? "Sin datos"
            : $"{Marca} {Modelo} {Anio}";

        #endregion

        #region Métodos Públicos

        /// <summary>
        /// Buscar vehículos por últimos 4 dígitos del VIN
        /// </summary>
        public async Task<bool> BuscarVehiculosAsync()
        {
            if (string.IsNullOrWhiteSpace(Ultimos4VIN) || Ultimos4VIN.Length != 4)
            {
                ErrorMessage = "Ingresa exactamente 4 caracteres del VIN";
                return false;
            }

            IsLoading = true;
            ErrorMessage = string.Empty;
            MostrarLista = false;

            try
            {
                System.Diagnostics.Debug.WriteLine($"🔍 Buscando vehículos con VIN terminado en: {Ultimos4VIN}");

                var response = await _apiService.BuscarVehiculosPorUltimos4VINAsync(Ultimos4VIN);

                if (response.Success && response.Vehiculos != null && response.Vehiculos.Any())
                {
                    VehiculosEncontrados.Clear();
                    foreach (var vehiculo in response.Vehiculos)
                    {
                        VehiculosEncontrados.Add(vehiculo);
                    }

                    System.Diagnostics.Debug.WriteLine($"✅ Se encontraron {VehiculosEncontrados.Count} vehículos");

                    if (VehiculosEncontrados.Count == 1)
                    {
                        // Si solo hay un resultado, cargarlo automáticamente
                        await CargarVehiculoAsync(VehiculosEncontrados[0].Id);
                    }
                    else
                    {
                        // Si hay múltiples resultados, mostrar la lista
                        MostrarLista = true;
                        ErrorMessage = $"Se encontraron {VehiculosEncontrados.Count} vehículos. Selecciona uno:";
                    }

                    return true;
                }
                else
                {
                    ErrorMessage = response.Message ?? "Vehículo no encontrado. Puedes registrar uno nuevo.";
                    MostrarLista = false;
                    System.Diagnostics.Debug.WriteLine($"ℹ️ No se encontraron vehículos");
                    return false;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error al buscar: {ex.Message}";
                MostrarLista = false;
                System.Diagnostics.Debug.WriteLine($"❌ Error en búsqueda: {ex.Message}");
                return false;
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Cargar datos completos de un vehículo por su ID
        /// </summary>
        public async Task<bool> CargarVehiculoAsync(int vehiculoId)
        {
            if (vehiculoId <= 0)
            {
                ErrorMessage = "ID de vehículo inválido";
                return false;
            }

            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                System.Diagnostics.Debug.WriteLine($"🔄 Cargando vehículo ID: {vehiculoId}");

                var response = await _apiService.ObtenerVehiculoPorIdAsync(vehiculoId);

                if (response.Success && response.Vehiculo != null)
                {
                    VehiculoId = response.Vehiculo.Id;
                    ClienteId = response.Vehiculo.ClienteId;
                    VIN = response.Vehiculo.VIN;
                    Marca = response.Vehiculo.Marca;
                    Modelo = response.Vehiculo.Modelo;
                    Version = response.Vehiculo.Version;
                    Anio = response.Vehiculo.Anio;
                    Color = response.Vehiculo.Color;
                    Placas = response.Vehiculo.Placas ?? "";
                    KilometrajeInicial = response.Vehiculo.KilometrajeInicial;

                    MostrarLista = false;

                    System.Diagnostics.Debug.WriteLine($"✅ Vehículo cargado: {VehiculoCompleto}");
                    return true;
                }
                else
                {
                    ErrorMessage = response.Message ?? "Error al cargar vehículo";
                    System.Diagnostics.Debug.WriteLine($"❌ Error: {ErrorMessage}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"❌ Excepción: {ex.Message}");
                return false;
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Guardar vehículo (crear nuevo o actualizar placas)
        /// </summary>
        public async Task<int> GuardarVehiculoAsync()
        {
            if (!Validar())
            {
                return 0;
            }

            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                if (VehiculoId > 0)
                {
                    // ACTUALIZAR: Solo se pueden actualizar las placas
                    System.Diagnostics.Debug.WriteLine($"🔄 Actualizando placas del vehículo ID: {VehiculoId}");

                    if (string.IsNullOrWhiteSpace(Placas))
                    {
                        ErrorMessage = "Las placas son requeridas";
                        return 0;
                    }

                    var response = await _apiService.ActualizarPlacasVehiculoAsync(VehiculoId, Placas);

                    if (response.Success)
                    {
                        ModoEdicion = false;
                        System.Diagnostics.Debug.WriteLine($"✅ Placas actualizadas: {Placas}");
                        return VehiculoId;
                    }
                    else
                    {
                        ErrorMessage = response.Message;
                        System.Diagnostics.Debug.WriteLine($"❌ Error: {response.Message}");
                        return 0;
                    }
                }
                else
                {
                    // CREAR NUEVO VEHÍCULO
                    if (ClienteId <= 0)
                    {
                        ErrorMessage = "Debe seleccionar un cliente primero";
                        System.Diagnostics.Debug.WriteLine("❌ Falta ClienteId");
                        return 0;
                    }

                    System.Diagnostics.Debug.WriteLine($"🔄 Creando nuevo vehículo para cliente ID: {ClienteId}");

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
                        System.Diagnostics.Debug.WriteLine($"✅ Vehículo creado con ID: {VehiculoId}");
                        return VehiculoId;
                    }
                    else
                    {
                        ErrorMessage = response.Message;
                        System.Diagnostics.Debug.WriteLine($"❌ Error: {response.Message}");
                        return 0;
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"❌ Excepción: {ex.Message}");
                return 0;
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Validar datos del vehículo
        /// </summary>
        public bool Validar()
        {
            if (string.IsNullOrWhiteSpace(VIN) || VIN.Length != 17)
            {
                ErrorMessage = "El VIN debe tener exactamente 17 caracteres";
                return false;
            }

            // Validar que el VIN solo contenga caracteres válidos (alfanuméricos, sin I, O, Q)
            if (!System.Text.RegularExpressions.Regex.IsMatch(VIN, @"^[A-HJ-NPR-Z0-9]{17}$"))
            {
                ErrorMessage = "El VIN contiene caracteres inválidos";
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
                ErrorMessage = "La versión es requerida";
                return false;
            }

            if (Anio < 2000 || Anio > DateTime.Now.Year + 1)
            {
                ErrorMessage = $"El año debe estar entre 2000 y {DateTime.Now.Year + 1}";
                return false;
            }

            if (KilometrajeInicial < 0)
            {
                ErrorMessage = "El kilometraje inicial no puede ser negativo";
                return false;
            }

            if (KilometrajeInicial == 0 && VehiculoId == 0) // Solo requerir en creación
            {
                ErrorMessage = "Ingresa el kilometraje inicial del vehículo";
                return false;
            }

            // Las placas son opcionales en algunos casos
            if (!string.IsNullOrWhiteSpace(Placas) && Placas.Length > 10)
            {
                ErrorMessage = "Las placas no pueden exceder 10 caracteres";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Limpiar todos los datos del formulario
        /// </summary>
        public void Limpiar()
        {
            VehiculoId = 0;
            ClienteId = 0;
            Ultimos4VIN = string.Empty;
            VIN = string.Empty;
            Marca = string.Empty;
            Modelo = string.Empty;
            Version = string.Empty;
            Anio = DateTime.Now.Year;
            Color = string.Empty;
            Placas = string.Empty;
            KilometrajeInicial = 0;
            ErrorMessage = string.Empty;
            MostrarLista = false;
            ModoEdicion = false;

            VehiculosEncontrados.Clear();

            System.Diagnostics.Debug.WriteLine("🧹 Módulo de vehículo limpiado");
        }

        /// <summary>
        /// Habilitar modo edición (solo para placas)
        /// </summary>
        public void HabilitarEdicion()
        {
            if (VehiculoId > 0)
            {
                ModoEdicion = true;
                System.Diagnostics.Debug.WriteLine("✏️ Modo edición de placas habilitado");
            }
        }

        /// <summary>
        /// Obtener resumen del vehículo para confirmación
        /// </summary>
        public string ObtenerResumen()
        {
            if (VehiculoId == 0 && string.IsNullOrEmpty(VIN))
            {
                return "Sin vehículo seleccionado";
            }

            string resumen = $"{VehiculoCompleto}\n";
            resumen += $"VIN: {VIN}\n";
            resumen += $"Version: {Version}\n";

            if (!string.IsNullOrEmpty(Color))
            {
                resumen += $"Color: {Color}\n";
            }

            if (!string.IsNullOrEmpty(Placas))
            {
                resumen += $"Placas: {Placas}\n";
            }

            resumen += $"Kilometraje inicial: {KilometrajeInicial:N0} km";

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