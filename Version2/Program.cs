class Program
{
    static void Main(string[] args)
    {
        Sensors sensors = new Sensors();

        sensors.InitSensors();
        while (true)
        {
            int? distance = sensors.GetDistance(); // Kald med ()
            // Udskriv resultatet, tjek for null (.HasValue)
            Console.WriteLine($"Distance: {(distance.HasValue ? distance.Value.ToString() : "FEJL/Ugyldig")} mm");

            Thread.Sleep(100);

            // --- Rettelse 2: Kald TiltAngle() som metode og håndter nullable double? ---
            double? angle = sensors.TiltAngle(); // Kald med ()
            // Udskriv resultatet, tjek for null (.HasValue) og formater med "F1" hvis gyldig
            Console.WriteLine($"Angle:    {(angle.HasValue ? angle.Value.ToString("F1") : "FEJL")} °");

            Thread.Sleep(100);
        }
    }
}
