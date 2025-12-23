using CarslineApp.Models;
using CarslineApp.Services;
using CarslineApp.Views;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace CarslineApp.ViewModels.ViewModelsHome
{
    public class TecnicoMainViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;
        private int _estadoTrabajoSeleccionado = 2;
        private bool _isLoading;
        private string _nombreUsuarioActual = string.Empty;


        private ObservableCollection<MiTrabajoDto> _trabajosServicio = new();
        private ObservableCollection<MiTrabajoDto> _trabajosReparacion = new();
        private ObservableCollection<MiTrabajoDto> _trabajosDiagnostico = new();
        private ObservableCollection<MiTrabajoDto> _trabajosGarantia = new();
        private ObservableCollection<MiTrabajoDto> _trabajosReacondicionamiento = new();


        public TecnicoMainViewModel()
        {
            _apiService = new ApiService();

            // Comandos de navegación
            VerPendientesCommand = new Command(() => CambiarEstadoTrabajo(2));
            VerPausadosCommand = new Command(() => CambiarEstadoTrabajo(5));
            VerFinalizadosCommand = new Command(() => CambiarEstadoTrabajo(4));

            // Comandos de acciones
            RefreshCommand = new Command(async () => await CargarTrabajos());
            LogoutCommand = new Command(async () => await OnLogout());

            // Solo cargar nombre de usuario aquí
            NombreUsuarioActual = Preferences.Get("user_name", "Tecnico");
        }

        public async Task InicializarAsync()
        {
            await CargarTrabajos();
        }

        #region Propiedades

        public string NombreUsuarioActual
        {
            get => _nombreUsuarioActual;
            set { _nombreUsuarioActual = value; OnPropertyChanged(); }
        }

        public int EstadoTrabajoSeleccionado
        {
            get => _estadoTrabajoSeleccionado;
            set
            {
                _estadoTrabajoSeleccionado = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TituloSeccion));
                OnPropertyChanged(nameof(EsPendiente));
                OnPropertyChanged(nameof(EsPausado));
                OnPropertyChanged(nameof(EsFinalizado));
            }
        }

        public string TituloSeccion => EstadoTrabajoSeleccionado switch
        {

            2 => "PENDIENTES",
            5 => "PAUSADOS",
            4 => "FINALIZADOS",
            _ => "TRABAJOS"
        };

        public bool EsPendiente => EstadoTrabajoSeleccionado == 2;
        public bool EsPausado => EstadoTrabajoSeleccionado == 5;
        public bool EsFinalizado => EstadoTrabajoSeleccionado == 4;

        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        public ObservableCollection<MiTrabajoDto> TrabajosServicio
        {
            get => _trabajosServicio;
            set { _trabajosServicio = value; OnPropertyChanged(); }
        }

        public ObservableCollection<MiTrabajoDto> TrabajosReparacion
        {
            get => _trabajosReparacion;
            set { _trabajosReparacion = value; OnPropertyChanged(); }
        }
        public ObservableCollection<MiTrabajoDto> TrabajosDiagnostico
        {
            get => _trabajosDiagnostico;
            set { _trabajosDiagnostico = value; OnPropertyChanged(); }
        }
        public ObservableCollection<MiTrabajoDto> TrabajosGarantia
        {
            get => _trabajosGarantia;
            set { _trabajosGarantia = value; OnPropertyChanged(); }
        }
        public ObservableCollection<MiTrabajoDto> TrabajosReacondicionamiento
        {
            get => _trabajosReacondicionamiento;
            set { _trabajosReacondicionamiento = value; OnPropertyChanged(); }
        }



        public bool HayTrabajosServicio => TrabajosServicio.Any();
        public bool HayTrabajosReparacion => TrabajosReparacion.Any();
        public bool HayTrabajosDiagnostico => TrabajosDiagnostico.Any();
        public bool HayTrabajosGarantia => TrabajosGarantia.Any();
        public bool HayTrabajosReacondicionamiento=> TrabajosReacondicionamiento.Any();


        #endregion

        #region Comandos

        public ICommand VerPendientesCommand { get; }
        public ICommand VerPausadosCommand { get; }
        public ICommand VerFinalizadosCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand LogoutCommand { get; }


        #endregion

        #region Métodos

        private async void CambiarEstadoTrabajo(int estadoTrabajo)
        {
            EstadoTrabajoSeleccionado = estadoTrabajo;
            await CargarTrabajos();
        }

        private async Task CargarTrabajos()
        {
            IsLoading = true;

            try
            {
                int tecnicoId = Preferences.Get("user_id", 0);

                var response = await _apiService.ObtenerMisTrabajosAsync(
                    tecnicoId,
                    EstadoTrabajoSeleccionado
                );

                if (response == null || response.Trabajos == null)
                    return;

                // 🔹 LIMPIAR LISTAS
                TrabajosServicio.Clear();
                TrabajosDiagnostico.Clear();
                TrabajosReparacion.Clear();
                TrabajosGarantia.Clear();
                TrabajosReacondicionamiento.Clear();

                // 🔹 SEPARAR POR TIPO DE ORDEN
                foreach (var trabajo in response.Trabajos)
                {
                    switch (trabajo.TipoOrden)
                    {
                        case 1:
                            TrabajosServicio.Add(trabajo);
                            break;

                        case 2:
                            TrabajosDiagnostico.Add(trabajo);
                            break;

                        case 3:
                            TrabajosReparacion.Add(trabajo);
                            break;

                        case 4:
                            TrabajosGarantia.Add(trabajo);
                            break;

                        case 5:
                            TrabajosReacondicionamiento.Add(trabajo);
                            break;
                    }
                }

                // 🔹 NOTIFICAR DASHBOARD
                NotificarCambiosDashboards();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error CargarTrabajos: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }


        private void NotificarCambiosDashboards()
        {
            OnPropertyChanged(nameof(TrabajosServicio));
            OnPropertyChanged(nameof(TrabajosReparacion));
            OnPropertyChanged(nameof(TrabajosDiagnostico));
            OnPropertyChanged(nameof(TrabajosGarantia));
            OnPropertyChanged(nameof(TrabajosReacondicionamiento));
            OnPropertyChanged(nameof(HayTrabajosServicio));
            OnPropertyChanged(nameof(HayTrabajosReparacion));
            OnPropertyChanged(nameof(HayTrabajosDiagnostico));
            OnPropertyChanged(nameof(HayTrabajosGarantia));
            OnPropertyChanged(nameof(HayTrabajosReacondicionamiento));
        }



        private async Task OnLogout()
        {
            bool confirm = await Application.Current.MainPage.DisplayAlert(
                "Cerrar Sesión",
                "¿Estás seguro que deseas cerrar sesión?",
                "Sí",
                "No");

            if (confirm)
            {
                Preferences.Clear();
                Application.Current.MainPage = new NavigationPage(new LoginPage())
                {
                    BarBackgroundColor = Color.FromArgb("#D60000"),
                    BarTextColor = Colors.White
                };
            }
        }

        #endregion

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}