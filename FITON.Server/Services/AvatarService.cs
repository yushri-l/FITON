using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace FITON.Server.Services
{
    public class AvatarService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public AvatarService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public async Task<string?> GenerateAvatarAsync(string prompt)
        {
            // Check if we have real API keys configured
            var bananaApiKey = _configuration["Banana:ApiKey"];
            var bananaModelKey = _configuration["Banana:ModelKey"];
            
            // If no real API keys are configured, return a mock response for testing
            if (string.IsNullOrEmpty(bananaApiKey) || bananaApiKey == "YOUR_BANANA_API_KEY" ||
                string.IsNullOrEmpty(bananaModelKey) || bananaModelKey == "YOUR_BANANA_MODEL_KEY")
            {
                // Simulate processing time
                await Task.Delay(2000);
                
                // Return a mock response that matches the expected structure
                var mockResponse = new
                {
                    modelOutputs = new[]
                    {
                        new
                        {
                            image_base64 = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNkYPhfDwAChwGA60e6kgAAAABJRU5ErkJggg==" // 1x1 transparent PNG for testing
                        }
                    }
                };
                
                return JsonSerializer.Serialize(mockResponse);
            }

            // Real API call code (when you have actual keys)
            var client = _httpClientFactory.CreateClient();
            var bananaApiUrl = $"https://api.banana.dev/start/v1/";

            var payload = new
            {
                apiKey = bananaApiKey,
                modelKey = bananaModelKey,
                modelInputs = new { prompt = prompt }
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var response = await client.PostAsync(bananaApiUrl, content);

            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                return responseBody;
            }

            return null;
        }
    }
}
