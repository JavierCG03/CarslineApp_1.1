using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CarslineApp.Models;
using CarslineApp.Services;

namespace CarslineApp.ViewModels.Modules
{
    /// <summary>
    /// Módulo para gestión de historial de servicios
    /// </summary>
    public class HistorialModule : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;

        // Estado del módulo
        private bool _isLoading;
        private ObservableCollection<HistorialServicioDto> _historialServicios = new();
        private bool _tieneHistorial;
        private string _resumenHistorial = string.Empty;

        public HistorialModule(ApiService apiService = null)
        {
            _apiService = apiService ?? new ApiService();
        }

        #region Propiedades Públicas

        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        public ObservableCollection<HistorialServicioDto> HistorialServicios
        {
            get => _historialServicios;
            set { _historialServicios = value; OnPropertyChanged(); }
        }

        public bool TieneHistorial
        {
            get => _tieneHistorial;
            set { _tieneHistorial = value; OnPropertyChanged(); }
        }

        public string ResumenHistorial
        {
            get => _resumenHistorial;
            set { _resumenHistorial = value; OnPropertyChanged(); }
        }

        #endregion

        #region Métodos Públicos

        /// <summary>
        /// Cargar historial de servicios de un vehículo
        /// </summary>
        public async Task<bool> CargarHistorialAsync(int vehiculoId)
        {
            if (vehiculoId <= 0)
            {
                TieneHistorial = false;
                ResumenHistorial = "Vehículo no especificado";
                return false;
            }

            IsLoading = true;
            TieneHistorial = false;
            HistorialServicios.Clear();

            try
            {
                System.Diagnostics.Debug.WriteLine($"🔍 Cargando historial para vehículo ID: {vehiculoId}");

                var response = await _apiService.ObtenerHistorialVehiculoAsync(vehiculoId);

                if (response.Success && response.Historial != null && response.Historial.Any())
                {
                    foreach (var servicio in response.Historial)
                    {
                        HistorialServicios.Add(servicio);
                    }

                    TieneHistorial = true;
                    ResumenHistorial = response.ResumenHistorial;

                    System.Diagnostics.Debug.WriteLine($"✅ Historial cargado: {response.TotalServicios} servicios");
                    return true;
                }
                else
                {
                    TieneHistorial = false;
                    ResumenHistorial = "Sin historial en los últimos 6 meses";
                    System.Diagnostics.Debug.WriteLine("ℹ️ No se encontró historial");
                    return false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error al cargar historial: {ex.Message}");
                TieneHistorial = false;
                ResumenHistorial = "Error al cargar historial";
                return false;
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Limpiar historial
        /// </summary>
        public void Limpiar()
        {
            HistorialServicios.Clear();
            TieneHistorial = false;
            ResumenHistorial = string.Empty;
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