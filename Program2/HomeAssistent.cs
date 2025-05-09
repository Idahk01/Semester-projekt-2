using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaendLys_Semesterprojekt_2
{
    // <<boundary>> Home Assistant
    class HomeAssistant
    {
        public bool Tilstand { get; private set; } = false;

        public void Taend()
        {
            Tilstand = true;
            Console.WriteLine("Home Assistant er tændt.");
        }

        public void Sluk()
        {
            Tilstand = false;
            Console.WriteLine("Home Assistant er slukket.");
        }
    }
}
