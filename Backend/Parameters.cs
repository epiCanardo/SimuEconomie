using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend
{
    public class Parameters
    {
        public int ConsoleRefreshTick { get; set; } // intervalle de refresh de la console
        public int DemandsUpdateTick { get; set; } // intervalle de mise à jour des demandes sur les biens
        public List<UniversalMerchendise> UniversalMerchendises { get; set; } // liste des biens sur le marché
        public List<Station> Stations { get; set; } // liste des stations
    }
}
