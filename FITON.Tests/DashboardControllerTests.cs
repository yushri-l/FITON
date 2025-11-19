using Xunit;
using System.Net;
using FluentAssertions;
using FITON.Server;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Threading.Tasks;

public class DashboardControllerTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    public DashboardControllerTests(TestWebApplicationFactory factory) => _client = factory.CreateClient();

    [Fact]
    public async Task Dashboard_ShouldReturnOk()
    {
        var res = await _client.GetAsync("/api/Dashboard/user-profile");
        res.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
