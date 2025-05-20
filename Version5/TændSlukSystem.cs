using System;
using System.Threading;

public class TændSlukSystem
{
    private readonly Afstand _afstand;
    private readonly Tilt _tilt;
    private readonly Kapacitiv _kapacitiv;
    private readonly LEDController _ledController;
    private readonly HomeAssistant _haController;
    private readonly TændSlukFunktion _funktion;

    private bool _systemOn    = false;
    private bool _tiltActive  = false;
    private int  _currentZone = -1;
    private const double tiltAngleThreshold = 30.0;
    
    // baseUrl fx "http://172.20.10.6:8123", bearerToken din long-lived token
    public TændSlukSystem(string baseUrl, string bearerToken)
    {
        Console.WriteLine("Initialiserer sensorer og HomeAssistant…");
        _afstand        = new Afstand();
        _tilt           = new Tilt();
        _kapacitiv      = new Kapacitiv();
        _ledController  = new LEDController();
        _haController   = new HomeAssistant(baseUrl, bearerToken);
        _funktion = new TændSlukFunktion(_haController);

        // Sluk alle zoner ved start
        _ledController.ControlLed1(false);
        _ledController.ControlLed2(false);
        _ledController.ControlLed3(false);
    }

    public void Start()
    {
        while (true)
        {
            // Toggle system vha. touch
            if (_kapacitiv.HasToggled())
            {
                ToggleSystem();
                Thread.Sleep(200);
            }

            if (_systemOn)
                CheckForTilt();

            Thread.Sleep(200);
        }
    }

    private void ToggleSystem()
    {
        _systemOn = !_systemOn;
        Console.WriteLine(_systemOn ? "SYSTEM TÆNDT" : "SYSTEM SLUKKET");

        if (!_systemOn)
        {
            _ledController.SetSystemInactive();
            _ledController.ControlLed1(false);
            _ledController.ControlLed2(false);
            _ledController.ControlLed3(false);
            _currentZone = -1;
        }
        else
        {
            _ledController.TurnOnStatusLed();
        }
    }

    private void CheckForTilt()
    {
        double? angle = _tilt.TiltAngle();
        int? distance = _afstand.GetDistance();
            
        Console.WriteLine($"Distance: {(distance.HasValue ? distance.Value.ToString() : "FEJL/Ugyldig")} mm");
        Console.WriteLine($"Angle:    {(angle.HasValue ? angle.Value.ToString("F1") : "FEJL")} °");

        LEDS(distance);

        if (angle > tiltAngleThreshold && !_tiltActive)
        {
            _tiltActive = true;
            if (distance.HasValue)
            {
                var funktion = _funktion.CalculateFunction(distance.Value);
                _funktion.Execute(funktion);
            }
        }
        else if (angle < tiltAngleThreshold - 5.0)
        {
            _tiltActive = false;
        }
    }

    private void LEDS(int? distance)
    {
        if (!distance.HasValue) return;

        int newZone = distance <= 150 ? 0
                   : distance <= 300 ? 1
                   : 2;
        // newZone får tildelt en værdi alt efter værdien på distance

        if (newZone == _currentZone) return; // Sørger for kun at opdatere LEDs når hånden flytter sig til ny zone
        _currentZone = newZone;

        // Opdatere LED udfra hvad newZone's værdi
        _ledController.ControlLed1(newZone == 0);
        _ledController.ControlLed2(newZone == 1);
        _ledController.ControlLed3(newZone == 2);
        
        
    }
}
