using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaendLys_Semesterprojekt_2
{
    // <<boundary>> Status LED
    class StatusLED
    {
        private string tilstand = "slukket";

        public void Taend()
        {
            tilstand = "tændt";
            Console.WriteLine("LED er tændt.");
        }

        public void Sluk()
        {
            tilstand = "slukket";
            Console.WriteLine("LED er slukket.");
        }
    }
}
