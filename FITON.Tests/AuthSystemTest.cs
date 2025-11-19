using Xunit;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Threading;

namespace FITON.Tests
{
    public class AuthSystemTest : IDisposable
    {
        private IWebDriver driver;

        // Constructor = setup
        public AuthSystemTest()
        {
            var options = new ChromeOptions();
            options.AddArgument("--headless=new");
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");
            // Do not create driver here; create per test to allow skipping
        }

        [Fact]
        public void TestValidLogin()
        {
            var run = Environment.GetEnvironmentVariable("RUN_SYSTEM_TESTS");
            if (string.IsNullOrEmpty(run) || !run.Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                // Skip running system test in CI/local unless explicitly enabled
                return;
            }

            driver = new ChromeDriver(new ChromeOptions());
            try
            {
                var url = Environment.GetEnvironmentVariable("FRONTEND_URL") ?? "https://localhost:4403/login";
                driver.Navigate().GoToUrl(url);
                var email = Environment.GetEnvironmentVariable("TEST_USER_EMAIL") ?? "rapiram83@gmail.com";
                var password = Environment.GetEnvironmentVariable("TEST_USER_PASSWORD") ?? "12345678";
                driver.FindElement(By.Name("email")).SendKeys(email);
                driver.FindElement(By.Name("password")).SendKeys(password);
                driver.FindElement(By.CssSelector("button[type='submit']")).Click();

                Thread.Sleep(2000);
                Assert.Contains("dashboard", driver.Url);
            }
            finally
            {
                driver?.Quit();
            }
        }

        [Fact]
        public void TestInvalidLogin()
        {
            var run = Environment.GetEnvironmentVariable("RUN_SYSTEM_TESTS");
            if (string.IsNullOrEmpty(run) || !run.Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                // Skip running system test in CI/local unless explicitly enabled
                return;
            }

            driver = new ChromeDriver(new ChromeOptions());
            try
            {
                var url = Environment.GetEnvironmentVariable("FRONTEND_URL") ?? "https://localhost:4403/login";
                driver.Navigate().GoToUrl(url);
                driver.FindElement(By.Name("email")).SendKeys("wrong@example.com");
                driver.FindElement(By.Name("password")).SendKeys("wrongpass");
                driver.FindElement(By.CssSelector("button[type='submit']")).Click();

                Thread.Sleep(2000);
                var pageSource = driver.PageSource;
                Assert.True(pageSource.Contains("Invalid credentials") || pageSource.Contains("Login failed"));
            }
            finally
            {
                driver?.Quit();
            }
        }

        // Dispose = teardown
        public void Dispose()
        {
            try { driver?.Quit(); } catch { }
        }
    }
}
