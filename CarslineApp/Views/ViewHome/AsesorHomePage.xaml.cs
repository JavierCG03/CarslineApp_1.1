using CarslineApp.ViewModels.ViewModelsHome;

namespace CarslineApp.Views
{
    public partial class AsesorHomePage : ContentPage
    {
        private readonly AsesorMainViewModel _viewModel;

        public AsesorHomePage()
        {
            InitializeComponent();
            _viewModel = new AsesorMainViewModel();
            BindingContext = _viewModel;
        }

        // ? AGREGAR ESTA SOBRECARGA
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.InicializarAsync(); // ? IMPORTANTE: Cargar órdenes cuando aparece la página
        }
    }
}