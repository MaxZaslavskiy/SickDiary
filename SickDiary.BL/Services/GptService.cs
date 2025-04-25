using Microsoft.Extensions.Configuration;
using SickDiary.DL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SickDiary.BL.Services;

public class GptService
{
    private readonly string _apiKey;

    public GptService(IConfiguration configuration)
    {
        _apiKey = configuration["OpenAI:ApiKey"];
        if (string.IsNullOrEmpty(_apiKey))
        {
            throw new InvalidOperationException("OpenAI API key is not configured.");
        }
    }

    public async Task<string> Analyze(IEnumerable<Record> records)
    {
        using var client = new HttpClient();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

        var recentRecords = records.OrderByDescending(r => r.Date).Take(10).ToList();
        string recordsJson = JsonSerializer.Serialize(recentRecords, new JsonSerializerOptions { WriteIndented = true });

        string prompt = $@"
            
            Я спостерігаю за хворим на цукровий діабет 1 типу. Нижче наведені останні щоденникові записи.
            Кожен запис містить дату, рівень глюкози в крові (ммоль/л), дозу інсуліну (одиниці), споживання вуглеводів (грами),
            самопочуття (дуже погане до дуже добре), фізична активність (низька, нормальна, висока), а також симптоми: запаморочення, пітливість, проблеми із зором, слабкість.

            Будь ласка:
            - Аналізуйте тенденції рівня глюкози та інсуліну
            - Визначити ознаки гіпоглікемії або гіперглікемії
            - Дай відкриту та широку рекомендацію, що робити при моїх результах, чи потрібно вжити вуглеводів, чи потрібно вколоти інсулін чи ще щось. Дай відкриту відповідь.

            Ось записи:

            ```json
            {recordsJson}
            ```";

        var requestBody = new
        {
            model = "gpt-3.5-turbo",
            messages = new[]
            {
                new { role = "user", content = prompt }
            },
            max_tokens = 500
        };

        try
        {
            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            var response = await client.PostAsync("https://api.openai.com/v1/chat/completions", content);

            if (response.IsSuccessStatusCode)
            {
                string responseJson = await response.Content.ReadAsStringAsync();
                using JsonDocument doc = JsonDocument.Parse(responseJson);
                string reply = doc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();

                return reply ?? "No analysis provided.";
            }
            else
            {
                string errorMessage = await response.Content.ReadAsStringAsync();
                return $"Error during analysis: {errorMessage}";
            }
        }
        catch (Exception ex)
        {
            return $"Exception during analysis: {ex.Message}";
        }
    }
}