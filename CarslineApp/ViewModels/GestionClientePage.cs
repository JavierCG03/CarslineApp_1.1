using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CarslineApp.Models;
using CarslineApp.Services;
using CarslineApp.ViewModels.Modules;

namespace CarslineApp.ViewModels
{
    /// <summary>
    /// ViewModel standalone para gestión de clientes
    /// Usa ClienteModule internamente
    /// </summary>
    public class GestionClienteViewModel : INotifyPropertyChanged
    {
        public ClienteModule ClienteModule { get; }

        private bool _isLoading;

        public GestionClienteViewModel()
        {
            ClienteModule = new ClienteModule(new ApiService());

            // Comandos
            BuscarCommand = new Command(async () => await Buscar());
            SeleccionarCommand = new Command<ClienteDto>(async (c) => await Seleccionar(c));
            GuardarCommand = new Command(async () => await Guardar(), () => !IsLoading);
            NuevoCommand = new Command(() => Nuevo());
            EditarCommand = new Command(() => ClienteModule.HabilitarEdicion());

            // Sincronizar estado
            ClienteModule.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(ClienteModule.IsLoading))
                {
                    IsLoading = ClienteModule.IsLoading;
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

        public bool MostrarFormulario => ClienteModule.ClienteId > 0 || !string.IsNullOrEmpty(ClienteModule.NombreCompleto);

        #endregion

        #region Comandos

        public ICommand BuscarCommand { get; }
        public ICommand SeleccionarCommand { get; }
        public ICommand GuardarCommand { get; }
        public ICommand NuevoCommand { get; }
        public ICommand EditarCommand { get; }

        #endregion

        #region Métodos

        private async Task Buscar()
        {
            await ClienteModule.BuscarClientesAsync();
        }

        private async Task Seleccionar(ClienteDto cliente)
        {
            if (cliente == null) return;

            bool cargado = await ClienteModule.CargarClienteAsync(cliente.Id);

            if (cargado)
            {
                OnPropertyChanged(nameof(MostrarFormulario));

                await Application.Current.MainPage.DisplayAlert(
                    "✅ Cliente Cargado",
                    $"Se cargaron los datos de {cliente.NombreCompleto}",
                    "OK");
            }
        }

        private async Task Guardar()
        {
            int resultado = await ClienteModule.GuardarClienteAsync();

            if (resultado > 0)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "✅ Éxito",
                    ClienteModule.ClienteId > 0
                        ? "Cliente actualizado correctamente"
                        : $"Cliente creado con ID: {resultado}",
                    "OK");

                OnPropertyChanged(nameof(MostrarFormulario));
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert(
                    "❌ Error",
                    ClienteModule.ErrorMessage,
                    "OK");
            }
        }

        private void Nuevo()
        {
            ClienteModule.Limpiar();
            OnPropertyChanged(nameof(MostrarFormulario));
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