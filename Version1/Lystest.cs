using System;
using System.Device.I2c;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

class Program
{
    const int deviceAddress = 0x68; // MPU-9150 I2C-adresse
    const int busId = 1;            // Typisk I2C-bus på Raspberry Pi

    static async Task Main(string[] args)
    {
        var connectionSettings = new I2cConnectionSettings(busId, deviceAddress);
        using var i2cDevice = I2cDevice.Create(connectionSettings);

        // Væk sensoren fra sleep mode
        i2cDevice.Write(new byte[] { 0x6B, 0x00 });

        // Til registrering af tilt right
        bool tiltRightActive = false;
        double tiltAngleThreshold = 30.0; // Tærskel i grader

        while (true)
        {
            // Læs accelerometerdata (registre 0x3B til 0x40)
            byte[] accelBuffer = new byte[6]; //Henter 2 bytes for hver af de 3 akser (X, Y, Z)
            i2cDevice.WriteRead(new byte[] { 0x3B }, accelBuffer);

            //Målingerne samles og den højeste byte forskydes 8 bits mod venstre
            //Værdierne kan være negativ derfor bruges short
            short accelX = (short)((accelBuffer[0] << 8) | accelBuffer[1]);
            short accelY = (short)((accelBuffer[2] << 8) | accelBuffer[3]);
            short accelZ = (short)((accelBuffer[4] << 8) | accelBuffer[5]);

            // Konverter til G (med ±2g opsætning → 16384 LSB/g)
            // Følsomheden er sat til 2g (1g = 16384, ikke forvekslet med tyngdeacceleration
            double Ax = accelX / 16384.0;
            double Ay = accelY / 16384.0;
            double Az = accelZ / 16384.0;

            // Beregn absolut hældningsvinkel i grader udfra acceleration (Atan2 bruges)
            double pitch = Math.Atan2(Ax, Math.Sqrt(Ay * Ay + Az * Az)) * (180.0 / Math.PI);
            double roll = Math.Atan2(Ay, Math.Sqrt(Ax * Ax + Az * Az)) * (180.0 / Math.PI);

            Console.WriteLine($"Pitch: {pitch:F2}°, Roll: {roll:F2}°");

            // Tjek om en "tilt right" er sket baseret på roll
            if (roll > tiltAngleThreshold && !tiltRightActive)
            {
                Console.WriteLine("Tilt right (accelerometer) registreret – toggler lampe...");
                await ToggleLampAsync();
                tiltRightActive = true;
            }
            else if (roll < tiltAngleThreshold - 5.0) // hysterese
            {
                tiltRightActive = false;
            }

            Thread.Sleep(100); // Opdatering ca. 10 gange i sekundet
        }
    }

    private static async Task ToggleLampAsync()
    {
        // Virtuel knap/Boolean som skifter stadie (toggle). Dette er linket til knappen
        string haUrl = "http://172.20.10.13:8123/api/services/input_boolean/toggle"; 
        
        // Nøglen til at få adgang til HomeAssistant og tilladelse til at ændre lampens stadie
        string token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJmMDFjOGJjMzBhNGE0NDAzYWJiNWU5NjdjYzdiZjU1MiIsImlhdCI6MTc0Mzk2OTY5NiwiZXhwIjoyMDU5MzI5Njk2fQ.perXXUfe-44-SzCOS1t1h-4BZ0AxvOiRTbI-16yxVvU";

        // Heroprettes en HttpClient der bruges til at kommunikere over nettet (http) og sende anmodninger
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token); // Fortæller jeg har adgang og bruger nøglen fra før

        // Dataen der sendes til Homeasistant om at det er "input_boolean.fake_lamp" der styres (denne skiftes ud med den reele lampe)
        // Sendes i formattet JSON og UTF8 sikrer at koden er korrekt til netværket
        var content = new StringContent("{\"entity_id\": \"input_boolean.fake_lamp\"}",
            Encoding.UTF8, "application/json");

        try
        {
            // Her sendes andmodningen, med det data vi configurerede før... Vi kan derefter tjekke om vores ændringer har virket eller ej
            var response = await client.PostAsync(haUrl, content);
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Lampe togglet succesfuldt.");
            }
            else
            {
                Console.WriteLine("Fejl ved toggling af lampe: " + response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Exception ved toggling af lampe: " + ex.Message);
        }
    }
}
