using System;
using System.Device.I2c;
using Iot.Device.Mpr121;
using System.Collections.Generic;
using System.Threading.Channels;

    public class Kapacitiv
    {
        const int mprAddress = 0x5A;   // MPR121 I2C-adresse
        const int busId = 1;      // Typisk I2C-bus på RPi

        private I2cDevice? mprDevice;
        private Mpr121? mprSensor;

        public void InitSensors()
        {
            // Undgå at geninitialisere
            if (mprDevice != null)
            {
                Console.WriteLine("Kapacitiv sensor allerede initialiseret.");
                return;
            }

            Console.WriteLine("Opretter I2C-forbindelse til MPR121...");
            Thread.Sleep(200);
            var mprSettings = new I2cConnectionSettings(busId, mprAddress);
            mprDevice = I2cDevice.Create(mprSettings);
            Thread.Sleep(200);

            mprSensor = new Mpr121(mprDevice, 100, configuration: new Mpr121Configuration());
            Console.WriteLine("MPR121 Initialiseret.");
            Thread.Sleep(200);
        }

        public Kapacitiv()
        {
            // Init I2C og selve sensoren
            mprDevice = I2cDevice.Create(new I2cConnectionSettings(busId, mprAddress));
            mprSensor = new Mpr121(mprDevice, 100, configuration: new Mpr121Configuration());
        }

        /// <summary>
        /// Returner true hvis mindst én kanal er “touched”
        /// </summary>
        public bool CheckForTouch()
        {
            if (mprSensor is null)
                throw new InvalidOperationException("Kald InitSensors() før du læser touch.");

            IReadOnlyDictionary<Channels, bool> statuses = mprSensor.ReadChannelStatuses();
            foreach (var touched in statuses.Values)
                if (touched) return true;

            return false;
        }
    }
