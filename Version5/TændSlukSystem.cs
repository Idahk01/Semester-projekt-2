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

    private bool _systemOn = false;
    private bool _tiltActive = false;
    private int _currentZone = -1;
    private const double TiltVinkelGrænse = 30.0;

    public TændSlukSystem()
    {
        Console.WriteLine("Initialiserer sensorer...");
        _afstand       = new Afstand();
        _tilt          = new Tilt();
        _kapacitiv     = new Kapacitiv();
        _ledController = new LEDController();
        _haController  = new HomeAssistant();
        _funktionHandler = new TændSlukFunktion(_haController);

        Console.WriteLine("Alle sensorer initialiseret. Starter måleløkke...");
        Thread.Sleep(500);

        // Sluk alle LED’er ved start
        _ledController.ControlLed1(false);
        _ledController.ControlLed2(false);
        _ledController.ControlLed3(false);
    }

    public void Start()
    {
        while (true)
        {
            // Touch-button toggle
            if (_kapacitiv.HasToggled())
            {
                ToggleSystem();
                Thread.Sleep(200); // debounce
            }

            // Sensor-kørsel kun når systemet er tændt
            if (_systemOn)
                CheckForTilt();

            Thread.Sleep(200);
        }
    }

    private void ToggleSystem()
    {
        _systemOn = !_systemOn;
        Console.WriteLine(_systemOn ? "System TÆNDT" : "System SLUKKET");

        if (!_systemOn)
        {
            // Inaktiv: status-LED på 20% og alle zone-LED slukket
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
        double? vinkel   = _tilt.TiltAngle();
        int? afstandMm   = _afstand.GetDistance();

        Console.WriteLine($"Distance: {(afstandMm.HasValue ? afstandMm.Value.ToString() : "FEJL")} mm");
        Console.WriteLine($"Vinkel:   {(vinkel   .HasValue ? vinkel   .Value.ToString("F1") : "FEJL")} °");

        UpdateZoneLeds(afstandMm);

        if (vinkel > TiltVinkelGrænse && !_tiltActive)
        {
            Console.WriteLine("Tilt registreret – udfører funktion…");
            _tiltActive = true;

            if (afstandMm.HasValue)
            {
                var funktion = _funktionHandler.BeregnFunktion(afstandMm.Value);
                _funktionHandler.UdførFunktion(funktion);
            }
        }
        else if (vinkel < TiltVinkelGrænse - 5.0)
        {
            _tiltActive = false;
        }
    }

    private void UpdateZoneLeds(int? afstandMm)
    {
        if (!afstandMm.HasValue)
            return;

        int nyZone = afstandMm <= 150 ? 0 :
                     afstandMm <= 300 ? 1 : 2;

        if (nyZone == _currentZone) return;
        _currentZone = nyZone;

        _ledController.ControlLed1(nyZone == 0);
        _ledController.ControlLed2(nyZone == 1);
        _ledController.ControlLed3(nyZone == 2);
    }
}
