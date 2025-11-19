using FITON.Server.Controllers;
using FITON.Server.DTOs;
using FITON.Server.Models;
using FITON.Server.Utils.Database;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using Xunit;

namespace FITON.Tests
{
    public class MeasurementsControllerTests
    {
        private AppDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(System.Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        private MeasurementsController GetController(AppDbContext db, int userId)
        {
            var controller = new MeasurementsController(db);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                    {
                        new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                        new Claim(ClaimTypes.NameIdentifier, userId.ToString())
                    }, "test"))
                }
            };
            return controller;
        }

        [Fact]
        public async Task SaveMeasurements_NewMeasurement_ReturnsOkAndPersists()
        {
            var db = GetDbContext();
            var controller = GetController(db, 1);

            var dto = new MeasurementDto
            {
                Height = "175",
                Weight = "70",
                Waist = "80",
                Hips = "95",
                SkinColor = "olive",
                Description = "Test description"
            };

            var result = await controller.SaveMeasurements(dto);
            var ok = Assert.IsType<OkObjectResult>(result);
            var saved = Assert.IsType<Measurement>(ok.Value);
            Assert.Equal("175", saved.Height);
            Assert.Equal(1, saved.UserId);

            // Confirm persisted
            var dbMeasurement = await db.Measurements.FirstOrDefaultAsync(m => m.UserId == 1);
            Assert.NotNull(dbMeasurement);
        }

        [Fact]
        public async Task GetMeasurements_NoMeasurement_ReturnsNotFound()
        {
            var db = GetDbContext();
            var controller = GetController(db, 2);

            var result = await controller.GetMeasurements();
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task DeleteMeasurements_NoMeasurement_ReturnsNotFound()
        {
            var db = GetDbContext();
            var controller = GetController(db, 3);

            var result = await controller.DeleteMeasurements();
            Assert.IsType<NotFoundObjectResult>(result);
        }
    }
}