using Xunit;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace FITON.Tests
{
    public class AuthSystemTest : IDisposable
    {
        private IWebDriver driver;
        private WebDriverWait wait;
        private static readonly HttpClient httpClient = new HttpClient();

        // Constructor = setup
        public AuthSystemTest()
        {
            var options = new ChromeOptions();
            options.AddArgument("--start-maximized");
            options.AddArgument("--ignore-certificate-errors");
            options.AddArgument("--allow-insecure-localhost");
            options.AcceptInsecureCertificates = true;
            driver = new ChromeDriver(options);
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
        }

        [Fact]
        public void TestInvalidLogin()
        {
            driver.Navigate().GoToUrl("https://localhost:4403/login");

            // Wait for page to load
            wait.Until(d => d.FindElement(By.Name("email")));

            driver.FindElement(By.Name("email")).SendKeys("wrong@example.com");
            driver.FindElement(By.Name("password")).SendKeys("wrongpass");
            driver.FindElement(By.CssSelector("button[type='submit']")).Click();

            Thread.Sleep(2000);
            var pageSource = driver.PageSource;
            Assert.True(pageSource.Contains("Invalid credentials") || pageSource.Contains("Login failed") || pageSource.Contains("Invalid") || pageSource.Contains("error"));
        }

        // Dispose = teardown
        public void Dispose()
        {
            driver.Quit();
        }
    }
}
