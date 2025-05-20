using System;
using System.Threading.Tasks;

public class TændSlukFunktion
{
    public enum FunktionId
    {
        TændSlukLampe,
        TændSlukTV,
        Fejl
    }

    private readonly HomeAssistant _haController;

    public TændSlukFunktion(HomeAssistant haController)
    {
        _haController = haController;
    }

    /// <summary>
    /// Beregn hvilken funktion, der skal udføres baseret på afstand i mm.
    /// </summary>
    public FunktionId CalculateFunction(int distance)
    {
        if (distance < 150) return FunktionId.TændSlukTV;
        if (distance < 300) return FunktionId.TændSlukLampe;
        return FunktionId.Fejl;
    }

    /// <summary>
    /// Udfør den valgte funktion (blokkerer indtil HTTP-kald er færdigt).
    /// </summary>
    public void Execute(FunktionId funktion)
    {
        switch (funktion)
        {
            case FunktionId.TændSlukLampe:
                Console.WriteLine("Toggler lampe...");
                // Antag at du har en ToggleLamp() i HomeAssistant
                _haController.ToggleLamp().GetAwaiter().GetResult();
                break;

            case FunktionId.TændSlukTV:
                Console.WriteLine("Toggler TV...");
                _haController.ToggleTV().GetAwaiter().GetResult();
                break;

            default:
                Console.WriteLine("Fejl: Ingen gyldig funktion valgt.");
                break;
        }
    }
}
