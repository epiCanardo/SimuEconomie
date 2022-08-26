using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Backend;

namespace Economy
{
    public class Ship : Storage
    {
        public Entity Localisation { get; set; }

        public int TotalCargoHold()
        {
            if (Merchendises.Any())
                return Merchendises.Sum(x => x.StackWeight);

            return 0;
        }
    }
}
