using System;
using System.Device.Gpio;
using System.Device.Pwm;
using System.Device.Pwm.Drivers;
using System.Threading;

public class LEDController : IDisposable
{
    private GpioController _gpioController;
    private PwmChannel _statusLedPwmChannel;
    private bool _disposed = false;

    private const int Led1Pin = 17; // GPIO17 (BCM-nummerering)
    private const int Led2Pin = 27; // GPIO27
    private const int Led3Pin = 22; // GPIO22
    private const int StatusLedPin = 18; // GPIO18 (BCM-nummerering) for status LED (PWM)
    private const int StatusLedFrequency = 1000; // Frekvens for PWM status LED

    private bool _isLed1On = false;
    private bool _isLed2On = false;
    private bool _isLed3On = false;
    // Vi behøver ikke en separat boolean for status LED's on/off, da lysstyrken (0.0 til 1.0) definerer dette.

    // Initialiserer en ny instans af LedController klassen.
    // Åbner GPIO-porte for standard LED'er og initialiserer PWM for status LED.
    
    public LEDController()
    {
        try
        {
            _gpioController = new GpioController(PinNumberingScheme.Logical); // Bruger BCM-nummerering

            // Åbn pins som output og sæt dem til LOW (slukket) for standard LED'er
            InitializePin(Led1Pin);
            InitializePin(Led2Pin);
            InitializePin(Led3Pin);

            // Initialiser PWM for status LED
            // SoftwarePwmChannel tager sig af pin-opsætningen.
            _statusLedPwmChannel = new SoftwarePwmChannel(
                pinNumber: StatusLedPin,
                frequency: StatusLedFrequency,
                dutyCycle: 0.0, // Start slukket
                usePrecisionTimer: true);
            _statusLedPwmChannel.Start();
            SetStatusLedBrightness(0.2);// StatusLed lyser svagt med 20% som standard

            Console.WriteLine("LedController initialiseret. Alle LED'er er slukket.");
        }
        catch (PlatformNotSupportedException ex)
        {
            Console.WriteLine($"FEJL: GPIO eller PWM er ikke understøttet på denne platform. {ex.Message}");
            Console.WriteLine("Kører du dette på en Raspberry Pi med GPIO biblioteker installeret og nødvendige tilladelser?");
            // Kast undtagelsen videre, eller håndter den på en måde der giver mening for din applikation
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"FEJL under initialisering af GpioController eller PwmChannel: {ex.Message}");
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
            if (_gpioController.GetPinMode(pinNumber) != PinMode.Output)
            {
                 _gpioController.SetPinMode(pinNumber, PinMode.Output);
            }
            _gpioController.Write(pinNumber, PinValue.Low);
            Console.WriteLine($"GPIO pin {pinNumber} var allerede åben, sikret som output og sat til LOW.");
        }
    }

    // Tænder eller slukker LED 1.
    public void ControlLed1(bool turnOn)
    {
        if (_gpioController == null) return;
        _gpioController.Write(Led1Pin, turnOn ? PinValue.High : PinValue.Low);
        _isLed1On = turnOn;
        Console.WriteLine($"LED 1 {(turnOn ? "tændt" : "slukket")} (GPIO {Led1Pin})");
    }

    // Tænder eller slukker LED 2.
    public void ControlLed2(bool turnOn)
    {
        if (_gpioController == null) return;
        _gpioController.Write(Led2Pin, turnOn ? PinValue.High : PinValue.Low);
        _isLed2On = turnOn;
        Console.WriteLine($"LED 2 {(turnOn ? "tændt" : "slukket")} (GPIO {Led2Pin})");
    }

    // Tænder eller slukker LED 3.
    public void ControlLed3(bool turnOn)
    {
        if (_gpioController == null) return;
        _gpioController.Write(Led3Pin, turnOn ? PinValue.High : PinValue.Low);
        _isLed3On = turnOn;
        Console.WriteLine($"LED 3 {(turnOn ? "tændt" : "slukket")} (GPIO {Led3Pin})");
    }

    // Toggler tilstanden for LED 1.
    public void ToggleLed1()
    {
        ControlLed1(!_isLed1On);
    }

    // Toggler tilstanden for LED 2.
    public void ToggleLed2()
    {
        ControlLed2(!_isLed2On);
    }

    // Toggler tilstanden for LED 3.
    public void ToggleLed3()
    {
        ControlLed3(!_isLed3On);
    }
    

    // Sætter lysstyrken på Status LED.
    // Lysstyrkeniveau mellem 0.0 (slukket) og 1.0 (fuld lysstyrke)
    public void SetStatusLedBrightness(double level)
    {
        if (_statusLedPwmChannel == null) return; // PWM channel ikke initialiseret
        _statusLedPwmChannel.DutyCycle = Math.Clamp(level, 0.0, 1.0);
        Console.WriteLine($"Status LED (GPIO {StatusLedPin}) lysstyrke sat til {level * 100:F0}%.");
    }
    
    // Tænder Status LED ved fuld lysstyrke.
    public void TurnOnStatusLed()
    {
        SetStatusLedBrightness(0.8);
        Console.WriteLine($"Status LED (GPIO {StatusLedPin}) tændt.");
    }
    
    // Sætter Status LED til inaktiv
    public void SetSystemInactive()
    {
        SetStatusLedBrightness(0.05);
        Console.WriteLine($"Status LED (GPIO {StatusLedPin}) sat til 5% (inaktiv tilstand).");
    }

    // Slukker Status LED.
    public void TurnOffStatusLed()
    {
        SetStatusLedBrightness(0.0);
        Console.WriteLine($"Status LED (GPIO {StatusLedPin}) slukket.");
    }
}
    
    
