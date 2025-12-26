
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
        public void SetValor(string campo, object valor)
        {
            var prop = typeof(CheckListServicioModel).GetProperty(campo);
            if (prop == null) return;

            if (prop.PropertyType == typeof(string))
            {
                prop.SetValue(CheckList, valor?.ToString());
            }
            else if (prop.PropertyType == typeof(bool?) || prop.PropertyType == typeof(bool))
            {
                prop.SetValue(CheckList, Convert.ToBoolean(valor));
            }
        }

    }
}

