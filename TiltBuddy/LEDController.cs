using System;
using System.Device.Gpio; // NuGet: System.Device.Gpio
using System.Threading;

public class LEDController : IDisposable
{
        private GpioController _gpioController;
        private bool _disposed = false;

        // Definer GPIO portnumrene for hver LED
        // VIGTIGT: Erstat disse med de faktiske GPIO BCM numre, du bruger!
        private const int Led1Pin = 17; // GPIO17 (BCM-nummerering)
        private const int Led2Pin = 27; // GPIO27
        private const int Led3Pin = 22; // GPIO22

        private bool _isLed1On = false;
        private bool _isLed2On = false;
        private bool _isLed3On = false;

        /// <summary>
        /// Initialiserer en ny instans af LedController klassen.
        /// Åbner GPIO-porte og sætter dem som output, initialt slukket.
        /// </summary>
        public LEDController()
        {
            try
            {
                _gpioController = new GpioController(PinNumberingScheme.Logical); // Bruger BCM-nummerering

                // Åbn pins som output og sæt dem til LOW (slukket)
                InitializePin(Led1Pin);
                InitializePin(Led2Pin);
                InitializePin(Led3Pin);

                Console.WriteLine("LedController initialiseret. LED'er er slukket.");
            }
            catch (PlatformNotSupportedException ex)
            {
                Console.WriteLine($"FEJL: GPIO er ikke understøttet på denne platform. {ex.Message}");
                Console.WriteLine("Kører du dette på en Raspberry Pi med GPIO biblioteker installeret?");
                // Kast undtagelsen videre, eller håndter den på en måde der giver mening for din applikation
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FEJL under initialisering af GpioController: {ex.Message}");
                throw;
            }
        }

        private void InitializePin(int pinNumber)
        {
            if (!_gpioController.IsPinOpen(pinNumber))
            {
                _gpioController.OpenPin(pinNumber, PinMode.Output);
                _gpioController.Write(pinNumber, PinValue.Low); // Start med LED slukket
                Console.WriteLine($"GPIO pin {pinNumber} initialiseret som output og sat til LOW.");
            }
            else
            {
                // Hvis pinnen allerede er åben (f.eks. af en anden del af programmet eller en tidligere kørsel),
                // sørg for, at den er i output mode og sat til LOW.
                // Dette er mere en sikkerhedsforanstaltning; ideelt set bør kun én controller styre en pin.
                if (_gpioController.GetPinMode(pinNumber) != PinMode.Output)
                {
                     _gpioController.SetPinMode(pinNumber, PinMode.Output);
                }
                _gpioController.Write(pinNumber, PinValue.Low);
                Console.WriteLine($"GPIO pin {pinNumber} var allerede åben, sikret som output og sat til LOW.");
            }
        }

        /// <summary>
        /// Tænder eller slukker LED 1.
        /// </summary>
        /// <param name="turnOn">True for at tænde LED'en, false for at slukke.</param>
        public void ControlLed1(bool turnOn)
        {
            if (_gpioController == null) return; // GpioController ikke initialiseret
            _gpioController.Write(Led1Pin, turnOn ? PinValue.High : PinValue.Low);
            _isLed1On = turnOn;
            Console.WriteLine($"LED 1 {(turnOn ? "tændt" : "slukket")} (GPIO {Led1Pin})");
        }

        /// <summary>
        /// Tænder eller slukker LED 2.
        /// </summary>
        /// <param name="turnOn">True for at tænde LED'en, false for at slukke.</param>
        public void ControlLed2(bool turnOn)
        {
            if (_gpioController == null) return;
            _gpioController.Write(Led2Pin, turnOn ? PinValue.High : PinValue.Low);
            _isLed2On = turnOn;
            Console.WriteLine($"LED 2 {(turnOn ? "tændt" : "slukket")} (GPIO {Led2Pin})");
        }

        /// <summary>
        /// Tænder eller slukker LED 3.
        /// </summary>
        /// <param name="turnOn">True for at tænde LED'en, false for at slukke.</param>
        public void ControlLed3(bool turnOn)
        {
            if (_gpioController == null) return;
            _gpioController.Write(Led3Pin, turnOn ? PinValue.High : PinValue.Low);
            _isLed3On = turnOn;
            Console.WriteLine($"LED 3 {(turnOn ? "tændt" : "slukket")} (GPIO {Led3Pin})");
        }

        /// <summary>
        /// Toggler tilstanden for LED 1.
        /// </summary>
        public void ToggleLed1()
        {
            ControlLed1(!_isLed1On);
        }

        /// <summary>
        /// Toggler tilstanden for LED 2.
        /// </summary>
        public void ToggleLed2()
        {
            ControlLed2(!_isLed2On);
        }

        /// <summary>
        /// Toggler tilstanden for LED 3.
        /// </summary>
        public void ToggleLed3()
        {
            ControlLed3(!_isLed3On);
        }

        /// <summary>
        /// Returnerer den aktuelle kendte tilstand for LED 1.
        /// </summary>
        public bool IsLed1On => _isLed1On;

        /// <summary>
        /// Returnerer den aktuelle kendte tilstand for LED 2.
        /// </summary>
        public bool IsLed2On => _isLed2On;

        /// <summary>
        /// Returnerer den aktuelle kendte tilstand for LED 3.
        /// </summary>
        public bool IsLed3On => _isLed3On;


        /// <summary>
        /// Frigør ressourcer brugt af GpioController.
        /// Lukker åbne pins.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                // Frigør administrerede ressourcer
                if (_gpioController != null)
                {
                    // Luk pins, hvis de er åbne
                    // Det er god praksis at returnere pins til en kendt (input) eller neutral tilstand,
                    // men for simpel LED-kontrol er det ofte nok bare at lukke dem.
                    // Hvis de ikke lukkes, kan de forblive i output-mode ved næste programstart.
                    if (_gpioController.IsPinOpen(Led1Pin)) _gpioController.ClosePin(Led1Pin);
                    if (_gpioController.IsPinOpen(Led2Pin)) _gpioController.ClosePin(Led2Pin);
                    if (_gpioController.IsPinOpen(Led3Pin)) _gpioController.ClosePin(Led3Pin);

                    _gpioController.Dispose();
                    _gpioController = null; // Sæt til null for at undgå yderligere brug
                    Console.WriteLine("LedController Disposed: GPIO pins lukket.");
                }
            }

            // Frigør uadministrerede ressourcer (hvis nogen)

            _disposed = true;
        }

        // Destructor (Finalizer) - kun nødvendig hvis du har uadministrerede ressourcer
        // direkte i denne klasse, hvilket GpioController håndterer internt.
        // ~LedController()
        // {
        //     Dispose(false);
        // }
    }
    
