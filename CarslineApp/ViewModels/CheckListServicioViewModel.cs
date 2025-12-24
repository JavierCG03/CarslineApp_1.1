
using CarslineApp.Models;

namespace CarslineApp.ViewModels
{
    public class CheckListServicioViewModel
    {
        public CheckListServicioModel CheckList { get; }

        public CheckListServicioViewModel(int trabajoId, int ordenId)
        {
            CheckList = new CheckListServicioModel
            {
                TrabajoId = trabajoId,
                OrdenId = ordenId
            };
        }

        public void SetValor(string campo, string valor)
        {
            // campo = NivelAceite, FiltroAceite, etc.
            var prop = typeof(CheckListServicioModel).GetProperty(campo);
            if (prop != null)
            {
                prop.SetValue(CheckList, valor);
            }
        }
    }
}

