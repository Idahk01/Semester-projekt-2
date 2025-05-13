
    class Program
    {
        private static Afstand afstand;
        private static Tilt tilt;
        private static Kapacitiv kapacitiv;
        private static StatusLedController? StatusLed;
        
        private static HomeAssistant haController;

        private static bool systemOn = false;
        private static bool tiltActive = false;
        private static double tiltAngleThreshold = 30.0;
        private static LEDController? LedController;

        static void Main(string[] args)
        {
            Console.WriteLine("Initialiserer sensorer...");

            // Opret sensor-objekter
            afstand = new Afstand();
            tilt = new Tilt();
            kapacitiv = new Kapacitiv();
            LedController = new LEDController();
            StatusLed = new StatusLedController(); // GPIO18 som default
            StatusLed.TurnOff(); // start slukket


            haController = new HomeAssistant(); //Initialiser HomeAssistant

            Console.WriteLine("Alle sensorer initialiseret. Starter måleløkke...");
            Thread.Sleep(500);
            
            LedController.ControlLed1(false);
            LedController.ControlLed2(false);
            LedController.ControlLed3(false);

            // Hovedløkke for måling og visning
            while (true)
            {
                // Håndtér touch-knap toggle
                if(kapacitiv.HasToggled())
                {
                    systemOn = !systemOn;
                    Console.WriteLine(systemOn ? "System TÆNDT" : "system SLUKKET");

                    // Valgfrit: sluk LED'er når system slukkes
                    if(!systemOn)
                    {
                        StatusLed?.TurnOff();
                         LedController.ControlLed1(false);
                         LedController.ControlLed2(false);
                         LedController.ControlLed3(false);
                        _currentZone = -1;
                    }
                    else
                    {
                        StatusLed?.TurnOn();
                    }

                   Thread.Sleep(200); // debounce ekstra 
                }
                // Kør sensorer KUN når systemet er tændt
                if(systemOn)
                {
                    CheckForTilt();
                }
               
                Thread.Sleep(200);
            }
        }
        
        public enum FunctionId
        {
            TurnOnOffLight,
            TurnOnOffTV,
            Error
        }
        private static void CheckForTilt()
        {
            double? angle = tilt.TiltAngle();
            int? distance = afstand.GetDistance();
            
            Console.WriteLine($"Distance: {(distance.HasValue ? distance.Value.ToString() : "FEJL/Ugyldig")} mm");
            Console.WriteLine($"Angle:    {(angle.HasValue ? angle.Value.ToString("F1") : "FEJL")} °");

            LEDS(distance);
            
            if (angle > tiltAngleThreshold && !tiltActive)
            {
                Console.WriteLine("Tilt right (accelerometer) registreret – toggler lampe...");
                tiltActive = true;
                var action = CalculateFunction(distance);

                switch (action)
                {
                    case FunctionId.TurnOnOffLight:
                        TurnOnOffLight();
                        break;
                    case FunctionId.TurnOnOffTV:
                        TurnOnOffTV();
                        break;
                    case FunctionId.Error:
                        Error();
                        break;
                }
            }
            else if (angle < tiltAngleThreshold - 5.0) // hysterese
            {
                tiltActive = false;
            }
        }

        private static FunctionId CalculateFunction(int? mm) => 
            mm switch 
            {
                < 150 => FunctionId.TurnOnOffTV,
                >= 150 and < 300 => FunctionId.TurnOnOffLight,
                _ => FunctionId.Error
            };

        private static void TurnOnOffLight()
        {
            Console.WriteLine("\u001b[32mTurning on/off light...\u001b[0m");
            //haController.ToggleLamp();
        }
        
        private static void TurnOnOffTV()
        {
            Console.WriteLine("\u001b[31mTurning on/off TV...\u001b[0m");
            haController.ToggleTV();
        }

        private static void Error()
        {
            Console.WriteLine("\u001b[33mError!\u001b[0m");
        }

        private static int _currentZone = -1;

        private static void LEDS(int? distance)
        {
            if (!distance.HasValue)
                return;

            int newZone = distance <= 150 ? 0 :
                distance <= 300 ? 1 : 2;

            if (newZone == _currentZone)
                return;

            _currentZone = newZone;

            LedController.ControlLed1(newZone == 0);
            LedController.ControlLed2(newZone == 1);
            LedController.ControlLed3(newZone == 2);
        }
    }
