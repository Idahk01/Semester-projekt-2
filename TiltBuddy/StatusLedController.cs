using System.Device.Pwm;
using System.Device.Pwm.Drivers;

public class StatusLedController
{
    private readonly PwmChannel _pwm;

    public StatusLedController(int gpioPin = 18, int frequency = 1000)
    {
        _pwm = new SoftwarePwmChannel(gpioPin, frequency, dutyCycle: 0.0, usePrecisionTimer: true);
        _pwm.Start();
    }

    public void SetBrightness(double level)
    {
        // level skal være mellem 0.0 (slukket) og 1.0 (fuldt lys)
        _pwm.DutyCycle = Math.Clamp(level, 0.0, 1.0);
    }

    public void TurnOn() => SetBrightness(1.0);
    public void TurnOff() => SetBrightness(0.0);

    public void Dispose()
    {
        _pwm?.Dispose();
    }
}
