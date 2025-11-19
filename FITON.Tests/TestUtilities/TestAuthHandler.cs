using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace FITON.Tests.TestUtilities
{
 public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
 {
 public TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, Microsoft.AspNetCore.Authentication.ISystemClock clock)
 : base(options, logger, encoder, clock)
 {
 }

 protected override Task<AuthenticateResult> HandleAuthenticateAsync()
 {
 // Create a test identity with common claims
 var claims = new[]
 {
 new Claim(ClaimTypes.NameIdentifier, "1"),
 new Claim(ClaimTypes.Name, "testuser"),
 new Claim("sub", "1")
 };

 var identity = new ClaimsIdentity(claims, "Test");
 var principal = new ClaimsPrincipal(identity);
 var ticket = new AuthenticationTicket(principal, "Test");
 return Task.FromResult(AuthenticateResult.Success(ticket));
 }
 }
}