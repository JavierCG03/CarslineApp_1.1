using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CarslineApp.Models;
using CarslineApp.Services;
using CarslineApp.ViewModels.Modules;

namespace CarslineApp.ViewModels
{
    /// <summary>
    /// ViewModel standalone para gestión de vehículos
    /// Usa VehiculoModule y ClienteModule internamente
    /// </summary>
    public class GestionVehiculoViewModel : INotifyPropertyChanged
    {
        public VehiculoModule VehiculoModule { get; }
        public ClienteModule ClienteModule { get; }

        private bool _isLoading;
        private bool _mostrarSeleccionCliente = true;

        public GestionVehiculoViewModel()
        {
            var apiService = new ApiService();
            VehiculoModule = new VehiculoModule(apiService);
            ClienteModule = new ClienteModule(apiService);

            // Comandos
            BuscarClienteCommand = new Command(async () => await BuscarCliente());
            SeleccionarClienteCommand = new Command<ClienteDto>(async (c) => await SeleccionarCliente(c));
            BuscarVehiculoCommand = new Command(async () => await BuscarVehiculo());
            SeleccionarVehiculoCommand = new Command<VehiculoDto>(async (v) => await SeleccionarVehiculo(v));
            GuardarCommand = new Command(async () => await Guardar(), () => !IsLoading);
            NuevoCommand = new Command(() => Nuevo());
            EditarCommand = new Command(() => VehiculoModule.HabilitarEdicion());

            // Sincronizar estado
            VehiculoModule.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(VehiculoModule.IsLoading))
                {
                    IsLoading = VehiculoModule.IsLoading;
                }
            };

            ClienteModule.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(ClienteModule.IsLoading))
                {
                    IsLoading = ClienteModule.IsLoading;
                }
                if (e.PropertyName == nameof(ClienteModule.ClienteId))
                {
                    VehiculoModule.ClienteId = ClienteModule.ClienteId;
                    OnPropertyChanged(nameof(ClienteSeleccionado));
                    OnPropertyChanged(nameof(MostrarFormularioVehiculo));
                }
            };
        }

        #region Propiedades

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
                ((Command)GuardarCommand).ChangeCanExecute();
            }
        }

        public bool MostrarSeleccionCliente
        {
            get => _mostrarSeleccionCliente;
            set
            {
                _mostrarSeleccionCliente = value;
                OnPropertyChanged();
            }
        }

        public bool ClienteSeleccionado => ClienteModule.ClienteId > 0;

        public bool MostrarFormularioVehiculo => ClienteSeleccionado &&
            (VehiculoModule.VehiculoId > 0 || !string.IsNullOrEmpty(VehiculoModule.VIN));

        public string InfoClienteSeleccionado => ClienteSeleccionado
            ? $"Cliente: {ClienteModule.NombreCompleto} (RFC: {ClienteModule.RFC})"
            : "Sin cliente seleccionado";

        #endregion

        #region Comandos

        public ICommand BuscarClienteCommand { get; }
        public ICommand SeleccionarClienteCommand { get; }
        public ICommand BuscarVehiculoCommand { get; }
        public ICommand SeleccionarVehiculoCommand { get; }
        public ICommand GuardarCommand { get; }
        public ICommand NuevoCommand { get; }
        public ICommand EditarCommand { get; }

        #endregion

        #region Métodos

        private async Task BuscarCliente()
        {
            await ClienteModule.BuscarClientesAsync();
        }

        private async Task SeleccionarCliente(ClienteDto cliente)
        {
            if (cliente == null) return;

            bool cargado = await ClienteModule.CargarClienteAsync(cliente.Id);

            if (cargado)
            {
                MostrarSeleccionCliente = false;
                OnPropertyChanged(nameof(InfoClienteSeleccionado));

                await Application.Current.MainPage.DisplayAlert(
                    "✅ Cliente Seleccionado",
                    $"Ahora puedes registrar vehículos para {cliente.NombreCompleto}",
                    "OK");
            }
        }

        private async Task BuscarVehiculo()
        {
            if (!ClienteSeleccionado)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "⚠️ Atención",
                    "Primero selecciona un cliente",
                    "OK");
                return;
            }

            await VehiculoModule.BuscarVehiculosAsync();
        }

        private async Task SeleccionarVehiculo(VehiculoDto vehiculo)
        {
            if (vehiculo == null) return;

            bool cargado = await VehiculoModule.CargarVehiculoAsync(vehiculo.Id);

            if (cargado)
            {
                // Cargar también el cliente asociado
                await ClienteModule.CargarClienteAsync(vehiculo.ClienteId);

                OnPropertyChanged(nameof(MostrarFormularioVehiculo));
                OnPropertyChanged(nameof(InfoClienteSeleccionado));

                await Application.Current.MainPage.DisplayAlert(
                    "✅ Vehículo Cargado",
                    $"Se cargó: {vehiculo.VehiculoCompleto}",
                    "OK");
            }
        }

        private async Task Guardar()
        {
            if (!ClienteSeleccionado)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "⚠️ Atención",
                    "Primero selecciona un cliente",
                    "OK");
                return;
            }

            int resultado = await VehiculoModule.GuardarVehiculoAsync();

            if (resultado > 0)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "✅ Éxito",
                    VehiculoModule.VehiculoId > 0
                        ? "Vehículo actualizado correctamente"
                        : $"Vehículo creado con ID: {resultado}",
                    "OK");

                OnPropertyChanged(nameof(MostrarFormularioVehiculo));
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert(
                    "❌ Error",
                    VehiculoModule.ErrorMessage,
                    "OK");
            }
        }

        private void Nuevo()
        {
            VehiculoModule.Limpiar();
            MostrarSeleccionCliente = true;
            OnPropertyChanged(nameof(MostrarFormularioVehiculo));
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