using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaendLys_Semesterprojekt_2
{
    // <<controller>> UC1 – Tænd/Sluk system
    class UC1_TaendSystem
    {
        private bool erTaendt = false;
        private KapacitivKnap knap;
        private StatusLED led;
        private HomeAssistant home;

        public UC1_TaendSystem(KapacitivKnap k, StatusLED l, HomeAssistant h)
        {
            knap = k;
            led = l;
            home = h;
        }

        public void ToggleSystem()
        {
            if (!erTaendt)
            {
                erTaendt = true;
                home.Taend();
                led.Taend();
            }
            else
            {
                erTaendt = false;
                home.Sluk();
                led.Sluk();
            }
        }
    }
}
