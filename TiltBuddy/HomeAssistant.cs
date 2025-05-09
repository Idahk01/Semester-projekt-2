using System;
using System.Device.I2c;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class HomeAssistantController
{
    // URL til din Home Assistant instans
    private const string HaBaseUrl = "http://172.20.10.13:8123";
    // Din Long-Lived Access Token til Home Assistant
    private const string HaToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJmMDFjOGJjMzBhNGE0NDAzYWJiNWU5NjdjYzdiZjU1MiIsImlhdCI6MTc0Mzk2OTY5NiwiZXhwIjoyMDU5MzI5Njk2fQ.perXXUfe-44-SzCOS1t1h-4BZ0AxvOiRTbI-16yxVvU";
    // ID på den standardenhed (lampe) der skal styres i Home Assistant
    private const string DefaultEntityId = "input_boolean.fake_lamp";

    private readonly HttpClient _httpClient;
    
    public HomeAssistantController()
    {
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", HaToken);
    }
    
    public async Task<bool> ToggleDefaultEntityAsync()
    {
        return await ToggleEntityAsync(DefaultEntityId);
    }
    
    public async Task<bool> ToggleEntityAsync(string entityId)
    {
        string domain = entityId.Split('.')[0];
        string servicePath = $"api/services/{domain}/toggle";
        string serviceUrl = $"{HaBaseUrl.TrimEnd('/')}/{servicePath}";
        
        var content = new StringContent($"{{\"entity_id\": \"{entityId}\"}}",
            Encoding.UTF8, "application/json");

        try
        {
            HttpResponseMessage response = await _httpClient.PostAsync(serviceUrl, content);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Enhed '{entityId}' togglet succesfuldt via {serviceUrl}.");
                return true;
            }
            else
            {
                string responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Fejl ved toggling af enhed '{entityId}': {response.StatusCode} - {responseContent} (URL: {serviceUrl})");
                return false;
            }
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Netværksfejl ved toggling af enhed '{entityId}': {ex.Message}");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Undtagelse ved toggling af enhed '{entityId}': {ex.Message}");
            return false;
        }
    }
}
