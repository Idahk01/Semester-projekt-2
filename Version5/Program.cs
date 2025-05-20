class Program
{
    static void Main(string[] args)
    {
        // Sæt din Home Assistant-url og token her:
        string baseUrl     = "http://172.20.10.6:8123";
        string bearerToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJmMDFjOGJjMzBhNGE0NDAzYWJiNWU5NjdjYzdiZjU1MiIsImlhdCI6MTc0Mzk2OTY5NiwiZXhwIjoyMDU5MzI5Njk2fQ.perXXUfe-44-SzCOS1t1h-4BZ0AxvOiRTbI-16yxVvU";

        var system = new TændSlukSystem(baseUrl, bearerToken);
        system.Start();
    }
}
