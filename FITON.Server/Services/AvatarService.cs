using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using FITON.Server.Models;
using FITON.Server.Utils.Database;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;

namespace FITON.Server.Services
{
    public class AvatarService : IAvatarService
    {
        private readonly IConfiguration _configuration;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<AvatarService> _logger;
        private readonly AppDbContext _context;

        public AvatarService(
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory,
            ILogger<AvatarService> logger,
            AppDbContext context)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _context = context;

            var storageConnectionString = _configuration["AzureStorage:ConnectionString"];
            _blobServiceClient = new BlobServiceClient(storageConnectionString);
        }

        public async Task<Avatar?> GenerateAvatarFromMeasurementsAsync(int userId, Measurement measurements)
        {
            try
            {
                _logger.LogInformation("Generating avatar for user {UserId}", userId);

                // Delete old avatar if exists
                await DeleteAvatarAsync(userId);

                // Generate image using Gemini
                var imageUrl = await GenerateImageWithGeminiAsync(measurements);

                if (string.IsNullOrEmpty(imageUrl))
                {
                    _logger.LogWarning("Gemini API did not return an image URL for user {UserId}", userId);
                    return null;
                }

                // Download and store in Azure Storage
                var finalImageUrl = await DownloadAndStoreImageAsync(imageUrl);

                // Create avatar entity
                var avatar = new Avatar
                {
                    ImageUrl = finalImageUrl,
                    UserId = userId
                };

                _context.Avatars.Add(avatar);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Avatar generated successfully for user {UserId}: {ImageUrl}",
                    userId, finalImageUrl);

                return avatar;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating avatar for user {UserId}", userId);
                return null;
            }
        }

        public async Task<bool> DeleteAvatarAsync(int userId)
        {
            try
            {
                var avatar = await _context.Avatars
                    .FirstOrDefaultAsync(a => a.UserId == userId);

                if (avatar == null)
                    return true;

                // Delete from Azure Storage
                if (!string.IsNullOrEmpty(avatar.ImageUrl))
                {
                    try
                    {
                        var uri = new Uri(avatar.ImageUrl);
                        var segments = uri.Segments;
                        var containerName = segments[1].Trim('/');
                        var blobName = string.Join("", segments.Skip(2)).Trim('/');

                        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                        var blobClient = containerClient.GetBlobClient(blobName);
                        await blobClient.DeleteIfExistsAsync();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to delete blob from storage for user {UserId}", userId);
                    }
                }

                // Delete from database
                _context.Avatars.Remove(avatar);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting avatar for user {UserId}", userId);
                return false;
            }
        }

        public async Task<Avatar?> RegenerateAvatarAsync(int userId)
        {
            try
            {
                var measurements = await _context.Measurements
                    .FirstOrDefaultAsync(m => m.UserId == userId);

                if (measurements == null)
                {
                    _logger.LogWarning("No measurements found for user {UserId} when regenerating avatar", userId);
                    return null;
                }

                return await GenerateAvatarFromMeasurementsAsync(userId, measurements);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error regenerating avatar for user {UserId}", userId);
                return null;
            }
        }

        public async Task<Avatar?> GetUserAvatarAsync(int userId)
        {
            return await _context.Avatars
                .FirstOrDefaultAsync(a => a.UserId == userId);
        }

        private async Task<string> GenerateImageWithGeminiAsync(Measurement measurements)
        {
            var apiKey = _configuration["Gemini:ApiKey"];
            var prompt = CreatePrompt(measurements);

            var httpClient = _httpClientFactory.CreateClient();
            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new
                            {
                                text = prompt
                            }
                        }
                    }
                },
                generationConfig = new
                {
                    temperature = 0.7,
                    topK = 40,
                    topP = 0.95,
                    maxOutputTokens = 2048,
                }
            };

            var url = $"https://generativelanguage.googleapis.com/v1/models/gemini-pro:generateContent?key={apiKey}";

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync(url, content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Gemini API call failed: {response.StatusCode} - {errorContent}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var geminiResponse = JsonSerializer.Deserialize<GeminiResponse>(responseContent);

            var imageDescription = geminiResponse?.candidates?[0].content.parts[0].text;

            if (string.IsNullOrEmpty(imageDescription))
            {
                throw new Exception("No content generated by Gemini");
            }

            // Use free image generation service
            return await GenerateImageFromDescriptionAsync(imageDescription);
        }

        private string CreatePrompt(Measurement measurements)
        {
            var height = !string.IsNullOrEmpty(measurements.Height) ? measurements.Height : "average height";
            var weight = !string.IsNullOrEmpty(measurements.Weight) ? measurements.Weight : "average weight";
            var skinColor = !string.IsNullOrEmpty(measurements.SkinColor) ? measurements.SkinColor : "medium";
            var description = !string.IsNullOrEmpty(measurements.Description) ? measurements.Description : "professional appearance";

            // Calculate body type from measurements if possible
            var bodyType = CalculateBodyType(measurements);

            // Build additional measurements string
            var additionalMeasurements = new List<string>();

            if (!string.IsNullOrEmpty(measurements.Chest))
                additionalMeasurements.Add($"chest: {measurements.Chest}");
            if (!string.IsNullOrEmpty(measurements.Waist))
                additionalMeasurements.Add($"waist: {measurements.Waist}");
            if (!string.IsNullOrEmpty(measurements.Hips))
                additionalMeasurements.Add($"hips: {measurements.Hips}");
            if (!string.IsNullOrEmpty(measurements.Shoulders))
                additionalMeasurements.Add($"shoulders: {measurements.Shoulders}");

            var measurementsText = additionalMeasurements.Any()
                ? $"Additional measurements: {string.Join(", ", additionalMeasurements)}. "
                : "";

            return $@"
            Create a detailed description for generating a photorealistic avatar portrait:

            PHYSICAL CHARACTERISTICS:
            - Height: {height}
            - Weight: {weight}
            - Body type: {bodyType}
            - Skin tone: {skinColor}
            {measurementsText}
            - Additional details: {description}

            Provide a concise description suitable for AI image generation focusing on:
            - Realistic facial features based on the physical characteristics
            - Professional appearance
            - Natural lighting
            - Neutral background
            - Shoulders and above framing
            - Age-appropriate appearance (25-35 years)

            Keep it under 150 characters for image generation compatibility.
            ";
        }

        private string CalculateBodyType(Measurement measurements)
        {
            if (string.IsNullOrEmpty(measurements.Height) || string.IsNullOrEmpty(measurements.Weight))
                return "average";

            try
            {
                // Try to parse height and weight
                var heightValue = ExtractNumericValue(measurements.Height);
                var weightValue = ExtractNumericValue(measurements.Weight);

                if (heightValue <= 0 || weightValue <= 0)
                    return "average";

                // Calculate BMI (assuming height is in cm and weight is in kg)
                var heightInMeters = heightValue / 100;
                var bmi = weightValue / (heightInMeters * heightInMeters);

                return bmi switch
                {
                    < 18.5 => "slim",
                    >= 18.5 and < 25 => "average",
                    >= 25 and < 30 => "curvy",
                    >= 30 => "stocky",
                    _ => "average"
                };
            }
            catch
            {
                return "average";
            }
        }

        private double ExtractNumericValue(string measurement)
        {
            if (string.IsNullOrEmpty(measurement))
                return 0;

            // Extract numbers from string (e.g., "180 cm" -> 180, "75 kg" -> 75)
            var numericPart = new string(measurement
                .Where(c => char.IsDigit(c) || c == '.' || c == ',')
                .ToArray())
                .Replace(',', '.');

            if (double.TryParse(numericPart, out var result))
            {
                return result;
            }

            return 0;
        }

        private async Task<string> GenerateImageFromDescriptionAsync(string description)
        {
            // For demo purposes - replace with actual free image API
            var encodedDescription = Uri.EscapeDataString(
                description.Length > 50 ? description.Substring(0, 50) + "..." : description);

            // Using placeholder service - in production, use actual image generation API
            return $"https://placehold.co/600x600/667eea/ffffff?text={encodedDescription}";
        }

        private async Task<string> DownloadAndStoreImageAsync(string imageUrl)
        {
            var httpClient = _httpClientFactory.CreateClient();

            var imageResponse = await httpClient.GetAsync(imageUrl);
            if (!imageResponse.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to download image from {imageUrl}");
            }

            var imageBytes = await imageResponse.Content.ReadAsByteArrayAsync();

            return await UploadToAzureBlobStorageAsync(imageBytes, $"{Guid.NewGuid()}.png");
        }

        private async Task<string> UploadToAzureBlobStorageAsync(byte[] imageBytes, string fileName)
        {
            var containerName = _configuration["AzureStorage:ContainerName"] ?? "avatars";
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

            // FIXED: Use CreateIfNotExistsAsync instead of CreateIfExistsAsync
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

            var blobClient = containerClient.GetBlobClient(fileName);

            using var stream = new MemoryStream(imageBytes);
            await blobClient.UploadAsync(stream, new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders
                {
                    ContentType = "image/png"
                }
            });

            return blobClient.Uri.ToString();
        }
    }

    // Gemini API Response models
    public class GeminiResponse
    {
        public List<Candidate> candidates { get; set; } = new();
    }

    public class Candidate
    {
        public Content content { get; set; } = new();
    }

    public class Content
    {
        public List<Part> parts { get; set; } = new();
    }

    public class Part
    {
        public string text { get; set; } = string.Empty;
    }
}