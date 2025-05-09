using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaendLys_Semesterprojekt_2;

class Program
{
    static void Main(string[] args)
    {
        // Initialisering
        KapacitivKnap knap = new KapacitivKnap();
        StatusLED led = new StatusLED();
        HomeAssistant assistent = new HomeAssistant();
        UC1_TaendSystem controller = new UC1_TaendSystem(knap, led, assistent);

        Console.WriteLine("Tryk [H] for at placere hånden foran sensoren (tænd/sluk).");
        Console.WriteLine("Tryk Ctrl+C for at afslutte.\n");

        while (true)
        {
            var key = Console.ReadKey(true).Key;

            if (key == ConsoleKey.H)
            {
                if (knap.RegistrerHaand())
                {
                    bool signal = knap.SendSignal();
                    if (signal)
                    {
                        controller.ToggleSystem();
                    }
                }
            }
        }
    }
}
