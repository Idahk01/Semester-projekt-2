using System;
using System.Device.I2c;
using Iot.Device.Vl53L0X;
using System.Device.I2c;

    public class Afstand
    {
        const int vlAddress = 0x29;
        const int busId = 1;

        private I2cDevice? vlDevice;
        private Vl53L0X? afstandsSensor;

        public void InitSensors()
        {
            // Undgå at geninitialisere
            if (vlDevice != null)
            {
                Console.WriteLine("Afstandssensor allerede initialiseret.");
                return;
            }

            Console.WriteLine("Opretter I2C-forbindelse til VL53L0X...");
            Thread.Sleep(200);
            var vlSettings = new I2cConnectionSettings(busId, vlAddress);
            vlDevice = I2cDevice.Create(vlSettings);
            Thread.Sleep(200);

            afstandsSensor = new Vl53L0X(vlDevice);
            Console.WriteLine("VL53L0X Initialiseret.");
            Thread.Sleep(200);
        }
        
        public Afstand()
        {
            vlDevice = I2cDevice.Create(new I2cConnectionSettings(busId, vlAddress));
            afstandsSensor = new Vl53L0X(vlDevice);
        }

        /// <summary>
        /// Henter afstand i mm, eller null ved invalid (>8190) eller fejl
        /// </summary>
        public int? GetDistance()
        {
            if (afstandsSensor == null)
            {
                Console.WriteLine("VL53L0X: Fejl - Sensor ikke initialiseret.");
                return null;
            }

            try
            {
                ushort distanceMm = afstandsSensor.Distance;
                if (distanceMm < 8190)
                    return distanceMm;
                else
                {
                    Console.WriteLine($"VL53L0X: Måling ugyldig (afstand {distanceMm} >= 8190)");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"VL53L0X: Fejl under læsning: {ex.Message}");
                return null;
            }
        }
    }
