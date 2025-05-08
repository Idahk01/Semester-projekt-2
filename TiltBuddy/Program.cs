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

            Console.WriteLine("Alle sensorer initialiseret. Starter måleløkke...");
            Thread.Sleep(500);

            // Hovedløkke for måling og visning
            while (true)
            {
                try
                {
                    bool isTouch = kapacitiv.CheckForTouch();
                    int? distance = afstand.GetDistance();
                    double? angle = tilt.TiltAngle();

                    Console.WriteLine($"Touch={isTouch}, Dist={distance} mm, Tilt={angle:F1}°");
                    Thread.Sleep(100);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Fejl under måling: {ex.Message}");
                    Thread.Sleep(1000);
                }
            }
        }
    }
