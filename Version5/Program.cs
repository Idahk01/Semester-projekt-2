class Program
{
    static void Main(string[] args)
    {
        // Sæt din Home Assistant-url og token her:
        string baseUrl     = "http://172.20.10.6:8123";
        string bearerToken = "DIN_LONG_LIVED_ACCESS_TOKEN";

        var system = new TændSlukSystem(baseUrl, bearerToken);
        system.Start();
    }
}
