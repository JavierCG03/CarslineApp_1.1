namespace CarslineApp.Views
{
    public partial class CheckListServicioPage : ContentPage
    {
        public int TrabajoId { get; private set; }
        public int OrdenId { get; private set; }

        public CheckListServicioPage()
        {
            InitializeComponent();
        }

        public CheckListServicioPage(int trabajoId, int ordenId, string vehiculo)
            : this()
        {
            TrabajoId = trabajoId;
            OrdenId = ordenId;
            Title = $"Servicio - {vehiculo}";
        }
    }
}