using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace FITON.Tests.TestUtilities
{
 public static class IntegrationAuthHelper
 {
 // WARNING: For CI, provide credentials via environment variables or secrets instead of hardcoding.
 public static async Task AuthenticateClientAsync(HttpClient client)
 {
 var email = System.Environment.GetEnvironmentVariable("TEST_USER_EMAIL") ?? "rapiram83@gmail.com";
 var password = System.Environment.GetEnvironmentVariable("TEST_USER_PASSWORD") ?? "12345678";

 var login = new { Email = email, Password = password };
 var res = await client.PostAsJsonAsync("/api/auth/login", login);
 res.EnsureSuccessStatusCode();
 var doc = await res.Content.ReadFromJsonAsync<JsonElement>();
 if (doc.TryGetProperty("token", out var tokenProp))
 {
 var token = tokenProp.GetString();
 client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
 }
 else
 {
 throw new System.InvalidOperationException("Login did not return token");
 }
 }
 }
}