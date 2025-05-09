using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaendLys_Semesterprojekt_2
{
    // <<boundary>> Kapacitiv knap
    class KapacitivKnap
    {
        private string tilstand = "inaktiv";

        public bool RegistrerHaand()
        {
            Console.WriteLine("Hånd registreret foran sensoren.");
            tilstand = "aktiv";
            return true;
        }

        public bool SendSignal()
        {
            if (tilstand == "aktiv")
            {
                Console.WriteLine("Sender signal til controller...");
                return true;
            }
            return false;
        }
    }

}
