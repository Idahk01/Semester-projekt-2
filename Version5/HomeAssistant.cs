using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public class HomeAssistant
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly string _bearerToken;

    // Skift disse til de entity_id'er, du har defineret i din Home Assistant
    private const string LampeId = "switch.innr_sp_221";
    private const string TvId    = "switch.salon_tv";

    /// <summary>
    /// Opret HttpClient og sæt base-URL + Bearer-token op.
    /// </summary>
    /// <param name="baseUrl">Fx "https://homeassistant.local:8123"</param>
    /// <param name="bearerToken">Din Long-Lived Access Token fra Home Assistant</param>
    public HomeAssistant(string baseUrl, string bearerToken)
    {
        _baseUrl     = baseUrl.TrimEnd('/');
        _bearerToken = bearerToken;

        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(_baseUrl)
        };
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _bearerToken);
        _httpClient.DefaultRequestHeaders.Accept
            .Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    /// <summary>
    /// Generisk metode til at toggle en hvilken som helst enhed via Home Assistant.
    /// </summary>
    /// <param name="entityId">Entity-ID, fx "switch.innr_sp_221"</param>
    /// <returns>True hvis kaldet lykkes (HTTP 2xx), ellers false.</returns>
    public async Task<bool> ToggleEntityAsync(string entityId)
    {
        var url = "/api/services/homeassistant/toggle";
        var payload = new { entity_id = entityId };
        var content = new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json");

        try
        {
            var response = await _httpClient.PostAsync(url, content);
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine(
                    $"[HomeAssistant] Fejl toggling af {entityId}: {(int)response.StatusCode} {response.ReasonPhrase}");
                return false;
            }
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[HomeAssistant] Exception ved toggle af {entityId}: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Wrapper til at toggle lampen.
    /// </summary>
    public async Task<bool> ToggleLamp()
    {
        Console.WriteLine($"[HomeAssistant] Toggler lampe ({LampeId})...");
        return await ToggleEntityAsync(LampeId);
    }

    /// <summary>
    /// Wrapper til at toggle TV'et.
    /// </summary>
    public async Task<bool> ToggleTV()
    {
        Console.WriteLine($"[HomeAssistant] Toggler TV ({TvId})...");
        return await ToggleEntityAsync(TvId);
    }
}
