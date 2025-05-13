using System;
using System.Device.I2c; // Denne nuget pakke skal implementeres som "dotnet add package Iot.Device.Bindings"
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Iot.Device.Vl53L0X; //https://learn.microsoft.com/en-us/dotnet/api/iot.device.vl53l0x.vl53l0x?view=iot-dotnet-latest

class Sensors
{
    const int mpuAddress = 0x68;   // MPU-9150 I2C-adresse
    const int vlAddress = 0x29;    // Vl53L0X I2C-adresse
    const int busId = 1;           // Typisk I2C-bus på Raspberry Pi
    
    private I2cDevice? mpuDevice;
    private I2cDevice? vlDevice; // Separat I2cDevice for Vl53L0X
    private Vl53L0X? afstandsSensor;

    public void InitSensors()
    {
        // Undgå at geninitialisere hvis det allerede er gjort
        if (mpuDevice != null || vlDevice != null)
        {
            Console.WriteLine("Sensorer allerede initialiseret.");
        }

        try
        {
            Console.WriteLine("Opretter I2C forbindelser...");
            var mpuSettings = new I2cConnectionSettings(busId, mpuAddress);
            var vlSettings = new I2cConnectionSettings(busId, vlAddress);

            mpuDevice = I2cDevice.Create(mpuSettings);
            vlDevice = I2cDevice.Create(vlSettings);
            afstandsSensor = new Vl53L0X(vlDevice); 

            Console.WriteLine("Forbundet til MPU sensor.");
            Console.WriteLine("VL53L0X Initialiseret.");

            // Væk MPU-9150 fra sleep mode
            // Tilføj null-tjek før brug (Selvom vi lige har sat den)
            if (mpuDevice != null)
            {
                mpuDevice.Write(new byte[] { 0x6B, 0x00 });
                Console.WriteLine("MPU vækket kommando sendt.");
                Thread.Sleep(100); // Vent kort
            }
            else {
                throw new InvalidOperationException("MPU Device blev ikke oprettet korrekt.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fejl ved oprettelse af I2C-forbindelser: {ex.Message}");
            // Simpel oprydning ved fejl
            afstandsSensor = null; // Vil være null hvis vlDevice er null
            vlDevice?.Dispose();
            mpuDevice?.Dispose();
            vlDevice = null;
            mpuDevice = null;
        }
    }

    public int? GetDistance()
    {
        if (afstandsSensor == null)
        {
            Console.WriteLine("VL53L0X: Fejl - Sensor ikke initialiseret.");
            return null;
        }

        try // Indsæt try-catch omkring I2C operation
        {
            ushort distanceMm = afstandsSensor.Distance;

            if (distanceMm < 8190)
            {
                // Console.WriteLine($"Afstand: {distanceMm} mm"); // Udskriv evt. kun i Main
                return distanceMm;
            }
            else
            {
                Console.WriteLine($"VL53L0X: Måling ugyldig (afstand {distanceMm} >= 8190)");
                return null; // Returner null ved ugyldig
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"VL53L0X: Fejl under læsning: {ex.Message}");
            return null;
        }
    }

    public double? TiltAngle() // Gør static, returtype double og nullable
    {
        // Tilføj null-tjek (Advarsel CS8602)
        if (mpuDevice == null)
        {
            Console.WriteLine("MPU: Fejl - Sensor ikke initialiseret.");
            return null;
        }

        try // Indsæt try-catch omkring I2C operation
        {
            byte[] accelBuffer = new byte[6];
            mpuDevice.WriteRead(new byte[] { 0x3B }, accelBuffer);

            short accelX = (short)((accelBuffer[0] << 8) | accelBuffer[1]);
            short accelY = (short)((accelBuffer[2] << 8) | accelBuffer[3]);
            short accelZ = (short)((accelBuffer[4] << 8) | accelBuffer[5]);

            double Ax = accelX / 16384.0; // Antager +/- 2g
            double Ay = accelY / 16384.0;
            double Az = accelZ / 16384.0;

            double roll = Math.Atan2(Ay, Math.Sqrt(Ax * Ax + Az * Az)) * (180.0 / Math.PI);
            return roll;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"MPU: Fejl under læsning: {ex.Message}");
            return null;
        }
    }
}
