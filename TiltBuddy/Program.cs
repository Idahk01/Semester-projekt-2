using System;
using System.Threading;
using TiltBuddy.Sensors;


    internal class Program
    {
        private static Afstand afstand;
        private static Kapacitiv kapacitiv;
        private static Tilt tilt;

        static void Main(string[] args)
        {
            Console.WriteLine("Initialiserer sensorer...");

            // Opret sensor-objekter
            afstand = new Afstand();
            kapacitiv = new Kapacitiv();
            tilt = new Tilt();

            // Initialiser sensorer
            afstand.InitSensors();
            kapacitiv.InitSensors();
            tilt.InitSensors();

            var haController = new HomeAssistant(); //Initialiser HomeAssistant

            private static bool tiltActive = false;
            private static double tiltAngleThreshold = 30.0;

            Console.WriteLine("Alle sensorer initialiseret. Starter måleløkke...");
            Thread.Sleep(500);

            // Hovedløkke for måling og visning
            while (true)
            {
                CheckForTilt();
            }
        }
        
        public enum FunctionId
        {
            TurnOnLight,
            TurnOffLight,
            Error
        }
        private static void CheckForTilt()
        {
            double? angle = tilt.TiltAngle();
            int? distance = afstand.GetDistance();
            
            Console.WriteLine($"Distance: {(distance.HasValue ? distance.Value.ToString() : "FEJL/Ugyldig")} mm");
            Console.WriteLine($"Angle:    {(angle.HasValue ? angle.Value.ToString("F1") : "FEJL")} °");
            
            if (angle > tiltAngleThreshold && !tiltActive)
            {
                Console.WriteLine("Tilt right (accelerometer) registreret – toggler lampe...");
                tiltActive = true;
                var action = CalculateFunction(distance);

                switch (action)
                {
                    case FunctionId.TurnOnLight:
                        TurnOnLight();
                        break;
                    case FunctionId.TurnOffLight:
                        TurnOffLight();
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
                < 150 => FunctionId.TurnOnLight,
                >= 150 and < 300 => FunctionId.TurnOffLight,
                _ => FunctionId.Error
            };

        private static void TurnOnLight()
        {
            Console.WriteLine("\u001b[32mTurning on light...\u001b[0m");
            haController.ToggleDefaultEntityAsync();
        }
        
        private static void TurnOffLight()
        {
            Console.WriteLine("\u001b[31mTurning off light...\u001b[0m");
            haController.ToggleDefaultEntityAsync();
        }

        private static void Error()
        {
            Console.WriteLine("\u001b[33mError!\u001b[0m");
        }
    }
