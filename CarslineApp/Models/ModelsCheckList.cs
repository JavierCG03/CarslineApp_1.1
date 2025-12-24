
namespace CarslineApp.Models
{
    public class CheckListServicioModel
    {
       
        public int TrabajoId { get; set; }
        public int OrdenId { get; set; }
        //Sistema de Direccion
        public string Bieletas { get; set; }
        public string Terminales { get; set; }
        public string CajaDireccion { get; set; }
        public string Volante { get; set; }

        //Sistema de Suspension
        public string AmortiguadoresDelanteros { get; set; }
        public string AmortiguadoresTraseros { get; set; }
        public string BarraEstabilizadora { get; set; }
        public string Horquillas { get; set; }

        //Neumaticos
        public string NeumaticosDelanteros { get; set; }
        public string NeumaticosTraseros { get; set; }
        public string Balanceo { get; set; }
        public string Alineacion { get; set; }

        //Luces
        public string LucesAltas { get; set; }
        public string LucesBajas { get; set; }
        public string LucesAntiniebla { get; set; }
        public string LucesReversa { get; set; }
        public string LucesDireccionales { get; set; }
        public string LucesIntermitentes { get; set; }

        //Sistema de Frenos
        public string DiscosTamboresDelanteros { get; set; }
        public string DiscosTamboresTraseros { get; set; }
        public string BalatasDelanteras { get; set; }
        public string BalatasTraseras { get; set; }


        public string NivelAceite { get; set; }
        public string FiltroAceite { get; set; }
        public string PresionLlantas { get; set; }
    }
}

