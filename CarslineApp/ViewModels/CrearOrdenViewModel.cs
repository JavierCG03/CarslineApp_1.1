using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CarslineApp.Models;
using CarslineApp.Services;
using CarslineApp.ViewModels.Modules;

namespace CarslineApp.ViewModels
{
    /// <summary>
    /// ViewModel Orquestador para el flujo completo de creación de orden
    /// Coordina los módulos: Cliente, Vehículo, Orden e Historial
    /// </summary>
    public class CrearOrdenViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;

        // 🎯 MÓDULOS INDEPENDIENTES
        public ClienteModule ClienteModule { get; }
        public VehiculoModule VehiculoModule { get; }
        public OrdenModule OrdenModule { get; }
        public HistorialModule HistorialModule { get; }

        // Estado de navegación
        private int _pasoActual = 1; // 1=Cliente, 2=Vehículo, 3=Orden
        private bool _isLoading;

        public CrearOrdenViewModel(int tipoOrdenId)
        {
            _apiService = new ApiService();

            // Inicializar módulos (compartiendo el mismo ApiService)
            ClienteModule = new ClienteModule(_apiService);
            VehiculoModule = new VehiculoModule(_apiService);
            OrdenModule = new OrdenModule(tipoOrdenId, _apiService);
            HistorialModule = new HistorialModule(_apiService);

            // Suscribirse a cambios de estado de los módulos
            SuscribirseAModulos();

            // Comandos de navegación
            SiguienteCommand = new Command(async () => await Siguiente(), () => !IsLoading);
            AnteriorCommand = new Command(() => Anterior());
            CrearOrdenCommand = new Command(async () => await CrearOrden(), () => !IsLoading);

            // Comandos delegados a módulos
            BuscarClienteCommand = new Command(async () => await ClienteModule.BuscarClientesAsync());
            SeleccionarClienteCommand = new Command<ClienteDto>(async (c) => await OnClienteSeleccionado(c));
            EditarGuardarClienteCommand = new Command(async () => await EditarGuardarCliente());

            BuscarVehiculoCommand = new Command(async () => await VehiculoModule.BuscarVehiculosAsync());
            SeleccionarVehiculoCommand = new Command<VehiculoDto>(async (v) => await OnVehiculoSeleccionado(v));
            EditarGuardarVehiculoCommand = new Command(async () => await EditarGuardarVehiculo());

            // Inicializar catálogos de orden
            _ = OrdenModule.InicializarAsync();
        }

        #region Propiedades de Navegación

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

        public bool MostrarPaso1 => PasoActual == 1;
        public bool MostrarPaso2 => PasoActual == 2;
        public bool MostrarPaso3 => PasoActual == 3;
        public bool MostrarBotonSiguiente => PasoActual < 3;
        public bool MostrarBotonCrear => PasoActual == 3;

        public string TituloPaso => PasoActual switch
        {
            1 => "DATOS DEL CLIENTE",
            2 => "DATOS DEL VEHÍCULO",
            3 => OrdenModule.EsServicio ? "DATOS DEL SERVICIO" :
                 OrdenModule.EsDiagnostico ? "DATOS DEL DIAGNÓSTICO" :
                 OrdenModule.EsReparacion ? "DATOS DE LA REPARACIÓN" :
                 OrdenModule.EsGarantia ? "DATOS DE LA GARANTÍA" : "CREAR ORDEN",
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
                ((Command)CrearOrdenCommand).ChangeCanExecute();
            }
        }

        #endregion

        #region Comandos

        public ICommand SiguienteCommand { get; }
        public ICommand AnteriorCommand { get; }
        public ICommand CrearOrdenCommand { get; }
        public ICommand BuscarClienteCommand { get; }
        public ICommand SeleccionarClienteCommand { get; }
        public ICommand EditarGuardarClienteCommand { get; }
        public ICommand BuscarVehiculoCommand { get; }
        public ICommand SeleccionarVehiculoCommand { get; }
        public ICommand EditarGuardarVehiculoCommand { get; }

        #endregion

        #region Métodos de Navegación

        private async Task Siguiente()
        {
            if (PasoActual == 1)
            {
                // Validar y guardar cliente
                if (ClienteModule.ModoEdicion)
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "⚠️ Atención",
                        "Guarda los cambios del cliente antes de continuar",
                        "OK");
                    return;
                }

                if (!ClienteModule.Validar())
                {
                    return;
                }

                // Crear cliente si es nuevo
                if (ClienteModule.ClienteId == 0)
                {
                    IsLoading = true;
                    try
                    {
                        int clienteId = await ClienteModule.GuardarClienteAsync();
                        if (clienteId == 0) return;

                        VehiculoModule.ClienteId = clienteId; // Vincular con vehículo
                    }
                    finally
                    {
                        IsLoading = false;
                    }
                }
                else
                {
                    VehiculoModule.ClienteId = ClienteModule.ClienteId;
                }

                PasoActual = 2;
            }
            else if (PasoActual == 2)
            {
                // Validar y guardar vehículo
                if (VehiculoModule.ModoEdicion)
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "⚠️ Atención",
                        "Guarda los cambios de las placas antes de continuar",
                        "OK");
                    return;
                }

                if (!VehiculoModule.Validar())
                {
                    return;
                }

                // Crear vehículo si es nuevo
                if (VehiculoModule.VehiculoId == 0)
                {
                    IsLoading = true;
                    try
                    {
                        int vehiculoId = await VehiculoModule.GuardarVehiculoAsync();
                        if (vehiculoId == 0) return;
                    }
                    finally
                    {
                        IsLoading = false;
                    }
                }

                // Cargar historial del vehículo
                await HistorialModule.CargarHistorialAsync(VehiculoModule.VehiculoId);

                // Establecer kilometraje actual desde el inicial
                if (OrdenModule.KilometrajeActual == 0)
                {
                    OrdenModule.KilometrajeActual = VehiculoModule.KilometrajeInicial;
                }

                PasoActual = 3;
            }
        }

        private void Anterior()
        {
            if (PasoActual > 1)
            {
                PasoActual--;

                // Ocultar listas al regresar
                if (PasoActual == 1)
                {
                    ClienteModule.MostrarLista = false;
                }
                else if (PasoActual == 2)
                {
                    VehiculoModule.MostrarLista = false;
                }
            }
        }

        #endregion

        #region Métodos de Creación de Orden

        private async Task CrearOrden()
        {
            IsLoading = true;

            try
            {
                var (success, numeroOrden, costoTotal, message) = await OrdenModule.CrearOrdenAsync(
                    ClienteModule.ClienteId,
                    VehiculoModule.VehiculoId,
                    VehiculoModule.KilometrajeInicial
                );

                if (success)
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "✅ ¡Éxito!",
                        $"Orden {numeroOrden} creada exitosamente\nCosto Total: ${costoTotal:N2}",
                        "OK");

                    await Application.Current.MainPage.Navigation.PopAsync();
                    MessagingCenter.Send(this, "OrdenCreada");
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "❌ Error",
                        message,
                        "OK");
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        #endregion

        #region Métodos de Módulos

        private async Task EditarGuardarCliente()
        {
            if (!ClienteModule.ModoEdicion)
            {
                ClienteModule.HabilitarEdicion();
                return;
            }

            IsLoading = true;
            try
            {
                int resultado = await ClienteModule.GuardarClienteAsync();

                if (resultado > 0)
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "✅ Éxito",
                        "Los datos del cliente han sido actualizados",
                        "OK");
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "❌ Error",
                        ClienteModule.ErrorMessage,
                        "OK");
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task EditarGuardarVehiculo()
        {
            if (!VehiculoModule.ModoEdicion)
            {
                VehiculoModule.HabilitarEdicion();
                return;
            }

            IsLoading = true;
            try
            {
                int resultado = await VehiculoModule.GuardarVehiculoAsync();

                if (resultado > 0)
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "✅ Éxito",
                        "Las placas han sido actualizadas",
                        "OK");
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "❌ Error",
                        VehiculoModule.ErrorMessage,
                        "OK");
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task OnClienteSeleccionado(ClienteDto cliente)
        {
            if (cliente == null) return;

            IsLoading = true;
            try
            {
                bool cargado = await ClienteModule.CargarClienteAsync(cliente.Id);
                VehiculoModule.ClienteId = cliente.Id;

                if (cargado)
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "✅ Cliente Seleccionado",
                        $"Se han cargado los datos de {cliente.NombreCompleto}",
                        "OK");
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task OnVehiculoSeleccionado(VehiculoDto vehiculo)
        {
            if (vehiculo == null) return;

            IsLoading = true;
            try
            {
                bool cargado = await VehiculoModule.CargarVehiculoAsync(vehiculo.Id);

                if (cargado)
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "✅ Vehículo Seleccionado",
                        $"Se ha cargado: {vehiculo.VehiculoCompleto}\nCliente: {vehiculo.NombreCliente}",
                        "OK");
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        #endregion

        #region Sincronización de Estado

        /// <summary>
        /// Suscribirse a cambios de estado de los módulos para sincronizar IsLoading
        /// </summary>
        private void SuscribirseAModulos()
        {
            ClienteModule.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(ClienteModule.IsLoading))
                {
                    IsLoading = ClienteModule.IsLoading;
                }
            };

            VehiculoModule.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(VehiculoModule.IsLoading))
                {
                    IsLoading = VehiculoModule.IsLoading;
                }
            };

            OrdenModule.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(OrdenModule.IsLoading))
                {
                    IsLoading = OrdenModule.IsLoading;
                }
            };

            HistorialModule.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(HistorialModule.IsLoading))
                {
                    IsLoading = HistorialModule.IsLoading;
                }
            };
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