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
                Console.WriteLine("\u001b[32mToggler lampe...\u001b[0m");
                // Antag at du har en ToggleLamp() i HomeAssistant
                _haController.ToggleLamp();
                break;

            case FunktionId.TændSlukTV:
                Console.WriteLine("\u001b[31mToggler TV...\u001b[0m");
                _haController.ToggleTV();
                break;

            default:
                Console.WriteLine("\u001b[33mFejl: Ingen gyldig funktion valgt.\u001b[0m");
                break;
        }
    }
}
