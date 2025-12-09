using CarslineApp.ViewModels;

namespace CarslineApp.Views
{
    public partial class CrearOrdenPage : ContentPage
    {
        private readonly CrearOrdenViewModel _viewModel;

        public CrearOrdenPage(int tipoOrdenId)
        {
            InitializeComponent();
            _viewModel = new CrearOrdenViewModel(tipoOrdenId);
            BindingContext = _viewModel;
        }

        /// <summary>
        /// ? Evento para recalcular costo cuando se selecciona/deselecciona un servicio extra
        /// </summary>
        private void OnServicioExtraChanged(object sender, CheckedChangedEventArgs e)
        {
            // Recalcular el costo total cuando cambia la selección
            _viewModel.OrdenModule.CalcularCostoTotal();
        }
    }
}