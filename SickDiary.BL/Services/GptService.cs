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
        var recordsForJson = recentRecords.Select(r => new
        {
            Date = r.Date.ToLocalTime(),
            r.BloodGlucoseLevel,
            r.InsulinDose,
            r.CarbohydrateIntake,
            r.WellBeingLevel,
            r.PhysicalActivityLevel,
            r.Dizziness,
            r.Sweating,
            r.VisionProblems,
            r.Weakness,
            r.MeasurementState,
            r.Result
        });
        string recordsJson = JsonSerializer.Serialize(recordsForJson, new JsonSerializerOptions { WriteIndented = true });

        string prompt = $@"
            I am monitoring a patient with Type 1 Diabetes. Below are recent diary entries.
            Each record includes date and time (local time), blood glucose level (mmol/L), insulin dose (units), carbohydrate intake (grams),
            well-being (VeryBad to VeryGood), physical activity (Low, Normal, High), measurement state (Fasting or Postprandial),
            and symptoms: dizziness, sweating, vision problems, weakness.

            Please:
            - Analyze trends in glucose levels, considering whether the measurements were taken fasting or postprandial (Fasting: normal 3.9–7.2 mmol/L, Postprandial: normal 5.0–10.0 mmol/L)
            - Evaluate insulin doses in relation to carbohydrate intake (e.g., insufficient insulin for high carb intake may cause hyperglycemia)
            - Consider physical activity: high activity may lower glucose levels, increasing the risk of hypoglycemia
            - Take into account well-being: poor well-being (Bad or VeryBad) may indicate underlying issues
            - Assess symptoms: multiple symptoms (dizziness, sweating, etc.) may indicate acute conditions like hypoglycemia or hyperglycemia
            - Provide suggestions for improvement or medical attention

            виводь результат українською


            Here are the records:
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
            max_tokens = 1500
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

                if (string.IsNullOrEmpty(reply))
                {
                    return "No analysis provided.";
                }

                // Форматуємо текст як HTML із секціями та списками
                var sections = reply.Split("---", StringSplitOptions.RemoveEmptyEntries);
                var formattedAnalysis = new StringBuilder();

                foreach (var section in sections)
                {
                    var lines = section.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                    if (lines.Length == 0) continue;

                    // Перший рядок секції — заголовок
                    var sectionTitle = lines[0].Trim();
                    formattedAnalysis.AppendLine($"<div class='analysis-section'><h5>{sectionTitle}</h5><ul>");
                    for (int i = 1; i < lines.Length; i++)
                    {
                        var line = lines[i].Trim();
                        if (line.StartsWith("-"))
                        {
                            var text = line.Substring(1).Trim();
                            formattedAnalysis.AppendLine($"<li>{text}</li>");
                        }
                        else
                        {
                            formattedAnalysis.AppendLine($"<li>{line}</li>");
                        }
                    }
                    formattedAnalysis.AppendLine("</ul></div>");
                }

                return formattedAnalysis.ToString();
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