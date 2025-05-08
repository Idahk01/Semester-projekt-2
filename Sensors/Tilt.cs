using System;
using System.Device.I2c;

namespace TiltBuddy.Sensors
{
    public class Tilt
    {
        const int mpuAddress = 0x68;  // MPU-9150 I2C-adresse
        const int busId = 1;     // Typisk I2C-bus på Raspberry Pi

        private I2cDevice? mpuDevice;

        public void InitSensors()
        {
            // Undgå at geninitialisere
            if (mpuDevice != null)
            {
                Console.WriteLine("Tilt-sensor allerede initialiseret.");
                return;
            }

            Console.WriteLine("Opretter I2C-forbindelse til MPU-9150...");
            Thread.Sleep(200);
            var mpuSettings = new I2cConnectionSettings(busId, mpuAddress);
            mpuDevice = I2cDevice.Create(mpuSettings);
            Thread.Sleep(200);

            // Væk MPU-9150 fra sleep mode
            mpuDevice.Write(new byte[] { 0x6B, 0x00 });
            Console.WriteLine("MPU vækket kommando sendt.");
            Thread.Sleep(100);
        }

        public Tilt()
        {
            mpuDevice = I2cDevice.Create(new I2cConnectionSettings(busId, mpuAddress));

            // Væk MPU-9150 fra sleep mode
            mpuDevice.Write(new byte[] { 0x6B, 0x00 });
        }

        /// <summary>
        /// Beregn “roll” (hældningsvinkel) i grader ud fra accelerator-registre
        /// </summary>
        public double? TiltAngle()
        {
            if (mpuDevice == null)
            {
                Console.WriteLine("MPU: Fejl - Sensor ikke initialiseret.");
                return null;
            }

            try
            {
                byte[] accelBuffer = new byte[6];
                mpuDevice.WriteRead(new byte[] { 0x3B }, accelBuffer);

                short accelX = (short)((accelBuffer[0] << 8) | accelBuffer[1]);
                short accelY = (short)((accelBuffer[2] << 8) | accelBuffer[3]);
                short accelZ = (short)((accelBuffer[4] << 8) | accelBuffer[5]);

                double Ax = accelX / 16384.0; // Antager ±2g
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
}
