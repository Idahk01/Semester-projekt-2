using System;
using System.Threading;

public class TændSlukSystem
{
    private readonly Afstand _afstand;
    private readonly Tilt _tilt;
    private readonly Kapacitiv _kapacitiv;
    private readonly LEDController _ledController;
    private readonly HomeAssistant _haController;
    private readonly TændSlukFunktion _funktionHandler;

    private bool _systemOn    = false;
    private bool _tiltActive  = false;
    private int  _currentZone = -1;
    private const double TiltVinkelGrænse = 30.0;

    /// <summary>
    /// baseUrl fx "http://172.20.10.6:8123", bearerToken din long-lived token
    /// </summary>
    public TændSlukSystem(string baseUrl, string bearerToken)
    {
        Console.WriteLine("Initialiserer sensorer og HomeAssistant…");
        _afstand        = new Afstand();
        _tilt           = new Tilt();
        _kapacitiv      = new Kapacitiv();
        _ledController  = new LEDController();
        _haController   = new HomeAssistant(baseUrl, bearerToken);
        _funktionHandler = new TændSlukFunktion(_haController);

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
        var vinkel   = _tilt.TiltAngle();
        var afstand  = _afstand.GetDistance();

        Console.WriteLine($"Distance: {(afstand.HasValue ? afstand.Value + " mm" : "FEJL")}");
        Console.WriteLine($"Vinkel:   {(vinkel.HasValue  ? vinkel.Value.ToString("F1") + " °" : "FEJL")}");

        UpdateZoneLeds(afstand);

        if (vinkel > TiltVinkelGrænse && !_tiltActive)
        {
            _tiltActive = true;
            if (afstand.HasValue)
            {
                var funktion = _funktionHandler.BeregnFunktion(afstand.Value);
                _funktionHandler.UdførFunktion(funktion);
            }
        }
        else if (vinkel < TiltVinkelGrænse - 5.0)
        {
            _tiltActive = false;
        }
    }

    private void UpdateZoneLeds(int? afstand)
    {
        if (!afstand.HasValue) return;

        int nyZone = afstand <= 150 ? 0
                   : afstand <= 300 ? 1
                   : 2;

        if (nyZone == _currentZone) return;
        _currentZone = nyZone;

        _ledController.ControlLed1(nyZone == 0);
        _ledController.ControlLed2(nyZone == 1);
        _ledController.ControlLed3(nyZone == 2);
    }
}
