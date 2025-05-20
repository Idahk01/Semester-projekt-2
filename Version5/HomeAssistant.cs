using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public class HomeAssistant
{
    private readonly HttpClient _httpClient;
    private const string LampeId = "switch.innr_sp_220";
    private const string TvId    = "switch.tv_plug_semester_projekt";

    public HomeAssistant(string baseUrl, string bearerToken)
    {
        _httpClient = new HttpClient { BaseAddress = new Uri(baseUrl.TrimEnd('/')) };
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", bearerToken);
        _httpClient.DefaultRequestHeaders.Accept
            .Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    private async Task<bool> ToggleEntityAsync(string entityId)
    {
        var payload = new { entity_id = entityId };
        var content = new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json"
        );

        try
        {
            var res = await _httpClient.PostAsync("/api/services/homeassistant/toggle", content);
            if (!res.IsSuccessStatusCode)
            {
                Console.WriteLine($"[HA] Fejl toggling {entityId}: {(int)res.StatusCode} {res.ReasonPhrase}");
                return false;
            }
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[HA] Exception ved toggle af {entityId}: {ex.Message}");
            return false;
        }
    }

    public Task<bool> ToggleLamp() => ToggleEntityAsync(LampeId);
    public Task<bool> ToggleTV()   => ToggleEntityAsync(TvId);
}
