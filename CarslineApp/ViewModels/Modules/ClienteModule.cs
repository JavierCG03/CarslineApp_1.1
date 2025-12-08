using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CarslineApp.Models;
using CarslineApp.Services;

namespace CarslineApp.ViewModels.Modules
{
    /// <summary>
    /// Módulo independiente para gestión de clientes
    /// Puede usarse tanto en CrearOrdenViewModel como en una vista standalone
    /// </summary>
    public class ClienteModule : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;

        // Estado del módulo
        private bool _isLoading;
        private string _errorMessage = string.Empty;
        private bool _modoEdicion;

        // Búsqueda
        private string _nombreBusqueda = string.Empty;
        private ObservableCollection<ClienteDto> _clientesEncontrados = new();
        private bool _mostrarLista;

        // Datos del cliente
        private int _clienteId;
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

        public ClienteModule(ApiService apiService = null)
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
                OnPropertyChanged(nameof(CamposBloqueados));
            }
        }

        public string NombreBusqueda
        {
            get => _nombreBusqueda;
            set { _nombreBusqueda = value; OnPropertyChanged(); ErrorMessage = string.Empty; }
        }

        public ObservableCollection<ClienteDto> ClientesEncontrados
        {
            get => _clientesEncontrados;
            set { _clientesEncontrados = value; OnPropertyChanged(); }
        }

        public bool MostrarLista
        {
            get => _mostrarLista;
            set { _mostrarLista = value; OnPropertyChanged(); }
        }

        public int ClienteId
        {
            get => _clienteId;
            set
            {
                _clienteId = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(EsClienteExistente));
                OnPropertyChanged(nameof(CamposBloqueados));
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
            set { _nombreCompleto = value; OnPropertyChanged(); }
        }

        public string TelefonoMovil
        {
            get => _telefonoMovil;
            set { _telefonoMovil = value; OnPropertyChanged(); }
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

        // Propiedades calculadas
        public bool EsClienteExistente => ClienteId > 0;
        public bool CamposBloqueados => EsClienteExistente && !ModoEdicion;
        public string TextoBotonAccion => ModoEdicion ? "💾 Guardar Cambios" : "✏️ Editar";

        #endregion

        #region Métodos Públicos

        /// <summary>
        /// Buscar clientes por nombre (mínimo 3 caracteres)
        /// </summary>
        public async Task<bool> BuscarClientesAsync()
        {
            if (string.IsNullOrWhiteSpace(NombreBusqueda) || NombreBusqueda.Length < 3)
            {
                ErrorMessage = "Ingresa al menos 3 caracteres del nombre";
                return false;
            }

            IsLoading = true;
            ErrorMessage = string.Empty;
            MostrarLista = false;

            try
            {
                var response = await _apiService.BuscarClientesPorNombreAsync(NombreBusqueda);

                if (response.Success && response.Clientes != null && response.Clientes.Any())
                {
                    ClientesEncontrados.Clear();
                    foreach (var cliente in response.Clientes)
                    {
                        ClientesEncontrados.Add(cliente);
                    }

                    if (ClientesEncontrados.Count == 1)
                    {
                        await CargarClienteAsync(ClientesEncontrados[0].Id);
                    }
                    else
                    {
                        MostrarLista = true;
                        ErrorMessage = $"Se encontraron {ClientesEncontrados.Count} clientes";
                    }

                    return true;
                }
                else
                {
                    ErrorMessage = "Cliente no encontrado. Puedes registrar uno nuevo.";
                    return false;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error: {ex.Message}";
                return false;
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Cargar datos completos de un cliente
        /// </summary>
        public async Task<bool> CargarClienteAsync(int clienteId)
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                var response = await _apiService.ObtenerClientePorIdAsync(clienteId);

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

                    MostrarLista = false;
                    return true;
                }
                else
                {
                    ErrorMessage = response.Message;
                    return false;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error: {ex.Message}";
                return false;
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Guardar o actualizar cliente
        /// </summary>
        public async Task<int> GuardarClienteAsync()
        {
            if (!Validar())
            {
                return 0;
            }

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

                if (ClienteId > 0)
                {
                    // Actualizar
                    var response = await _apiService.ActualizarClienteAsync(ClienteId, request);
                    if (response.Success)
                    {
                        ModoEdicion = false;
                        return ClienteId;
                    }
                    else
                    {
                        ErrorMessage = response.Message;
                        return 0;
                    }
                }
                else
                {
                    // Crear nuevo
                    var response = await _apiService.CrearClienteAsync(request);
                    if (response.Success)
                    {
                        ClienteId = response.ClienteId;
                        return ClienteId;
                    }
                    else
                    {
                        ErrorMessage = response.Message;
                        return 0;
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error: {ex.Message}";
                return 0;
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Validar datos del cliente
        /// </summary>
        public bool Validar()
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

        /// <summary>
        /// Limpiar formulario
        /// </summary>
        public void Limpiar()
        {
            ClienteId = 0;
            NombreBusqueda = string.Empty;
            RFC = string.Empty;
            NombreCompleto = string.Empty;
            TelefonoMovil = string.Empty;
            TelefonoCasa = string.Empty;
            CorreoElectronico = string.Empty;
            Colonia = string.Empty;
            Calle = string.Empty;
            NumeroExterior = string.Empty;
            Municipio = string.Empty;
            Estado = string.Empty;
            CodigoPostal = string.Empty;
            ErrorMessage = string.Empty;
            MostrarLista = false;
            ModoEdicion = false;
        }

        /// <summary>
        /// Habilitar modo edición
        /// </summary>
        public void HabilitarEdicion()
        {
            ModoEdicion = true;
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