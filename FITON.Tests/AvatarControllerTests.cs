using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

public class AvatarControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public AvatarControllerTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task UploadAvatar_ShouldReturnOk_WhenImageIsValid()
    {
        // Arrange: prepare a fake image file for upload
        var content = new MultipartFormDataContent();
        var imageContent = new ByteArrayContent(System.IO.File.ReadAllBytes("TestData/test-avatar.png"));
        imageContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/png");
        content.Add(imageContent, "file", "test-avatar.png");

        // Add authorization header if your API requires JWT
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", "fake-jwt-token");

        // Act: send POST request to the avatar upload endpoint
        var response = await _client.PostAsync("/api/Avatar/Upload", content);

        // Assert: expect 200 OK or 201 Created
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created);

        // Optionally verify returned JSON includes avatar URL/path
        var json = await response.Content.ReadFromJsonAsync<AvatarResponse>();
        json.Should().NotBeNull();
        json!.AvatarUrl.Should().Contain(".png");
    }

    [Fact]
    public async Task UploadAvatar_ShouldReturnBadRequest_WhenFileIsMissing()
    {
        // Arrange: send empty content
        var content = new MultipartFormDataContent();

        // Act
        var response = await _client.PostAsync("/api/Avatar/Upload", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private class AvatarResponse
    {
        public string? AvatarUrl { get; set; }
    }
}
