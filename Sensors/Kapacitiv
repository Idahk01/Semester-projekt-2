using System;
using System.Device.Gpio;

public class Kapacitiv
{
    private readonly int _pin = 17;
    private readonly GpioController _gpio;
    private bool lastStableState = false;
    private DateTime lastTouchTime = DateTime.MinValue;
    private readonly int debounceMs = 300;

    public Kapacitiv()
    {
        _gpio = new GpioController();
        _gpio.OpenPin(_pin, PinMode.InputPullDown); // Brug pull-down for stabilitet
    }

    /// <summary>
    /// Returnerer true ved hver ny trykændring (HIGH→LOW eller LOW→HIGH)
    /// </summary>
    public bool HasToggled()
    {
        bool currentState = _gpio.Read(_pin) == PinValue.High;

        // Hvis tilstanden har ændret sig
        if (currentState != lastStableState)
        {
            lastStableState = currentState;
            var now = DateTime.UtcNow;

            if ((now - lastTouchTime).TotalMilliseconds >= debounceMs)
            {
                lastTouchTime = now;
                return true;
            }
        }

        return false;
    }
}
