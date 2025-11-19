using FITON.Server.Controllers;
using FITON.Server.Models;
using FITON.Server.Services;
using FITON.Server.Utils.Database;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using Xunit;
using System.Collections.Generic;

namespace FITON.Tests
{
 public class VirtualTryOnControllerTests_MockedImageGenerator
 {
 private AppDbContext GetDb()
 {
 var options = new DbContextOptionsBuilder<AppDbContext>()
 .UseInMemoryDatabase(System.Guid.NewGuid().ToString())
 .Options;
 return new AppDbContext(options);
 }

 private VirtualTryOnController GetController(AppDbContext db, IImageGenerator gen, int userId)
 {
 var controller = new VirtualTryOnController(db, gen);
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
 public async Task GenerateTryOn_Returns500_WhenImageGeneratorThrows()
 {
 var db = GetDb();
 var user = new User { Id =10, Username = "u1", Email = "u1@test", PasswordHash = "x" };
 db.Users.Add(user);
 db.Measurements.Add(new Measurement { UserId = user.Id, Height = "170", Weight = "65" });
 var outfit = new Outfit { Id =50, Name = "TopX", UserId = user.Id, Type = "shirt" };
 db.Outfits.Add(outfit);
 db.Wardrobes.Add(new Wardrobe { Id =60, UserId = user.Id, Name = "Set", TopClothesId = outfit.Id });
 await db.SaveChangesAsync();

 var mockGen = new Mock<IImageGenerator>();
 mockGen.Setup(g => g.GenerateImageAsync(It.IsAny<string>())).ThrowsAsync(new System.Exception("API down"));

 var controller = GetController(db, mockGen.Object, user.Id);
 var dto = new FITON.Server.Controllers.GenerateTryOnDto { WardrobeId =60 };
 var result = await controller.GenerateTryOn(dto);
 var obj = Assert.IsType<ObjectResult>(result);
 Assert.Equal(500, obj.StatusCode);
 }

 [Fact]
 public async Task GenerateTryOn_ReturnsOk_WithMockedImage()
 {
 var db = GetDb();
 var user = new User { Id =11, Username = "u2", Email = "u2@test", PasswordHash = "x" };
 db.Users.Add(user);
 db.Measurements.Add(new Measurement { UserId = user.Id, Height = "170", Weight = "65" });
 var outfit = new Outfit { Id =501, Name = "TopY", UserId = user.Id, Type = "shirt" };
 db.Outfits.Add(outfit);
 db.Wardrobes.Add(new Wardrobe { Id =602, UserId = user.Id, Name = "Set2", TopClothesId = outfit.Id });
 await db.SaveChangesAsync();

 var mockGen = new Mock<IImageGenerator>();
 mockGen.Setup(g => g.GenerateImageAsync(It.IsAny<string>())).ReturnsAsync("data:image/png;base64,AAA");

 var controller = GetController(db, mockGen.Object, user.Id);
 var dto = new FITON.Server.Controllers.GenerateTryOnDto { WardrobeId =602 };
 var result = await controller.GenerateTryOn(dto);
 var ok = Assert.IsType<OkObjectResult>(result);
 var val = ok.Value;
 if (val is IDictionary<string, object> dict)
 {
 Assert.True(dict.ContainsKey("imageUrl"));
 Assert.Contains("data:image/png;base64", dict["imageUrl"].ToString());
 }
 else
 {
 var prop = val?.GetType().GetProperty("imageUrl") ?? val?.GetType().GetProperty("ImageUrl");
 if (prop is null) throw new Xunit.Sdk.XunitException("Response does not contain imageUrl");
 var imageUrl = prop.GetValue(val)?.ToString();
 Assert.NotNull(imageUrl);
 Assert.Contains("data:image/png;base64", imageUrl);
 }
 }
 }
}
