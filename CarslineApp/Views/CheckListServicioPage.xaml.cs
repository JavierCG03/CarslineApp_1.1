using CarslineApp.ViewModels;

namespace CarslineApp.Views
{
    public partial class CheckListServicioPage : ContentPage
    {
        public CheckListServicioPage(int trabajoId, int ordenId, string vehiculo)
        {
            InitializeComponent();

            Title = $"Servicio - {vehiculo}";
            BindingContext = new CheckListServicioViewModel(trabajoId, ordenId);
        }

        private void Radio_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            if (!e.Value) return;

            var radio = (RadioButton)sender;
            var vm = (CheckListServicioViewModel)BindingContext;

            vm.SetValor(
                radio.GroupName,
                radio.Value?.ToString()
            );
        }

        private async void Guardar_Clicked(object sender, EventArgs e)
        {
            await DisplayAlert("Checklist", "Checklist guardado", "OK");
        }
    }
}
