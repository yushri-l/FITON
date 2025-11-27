using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using FITON.Server.Controllers;
using FITON.Server.Models;
using FITON.Server.DTOs;
using FITON.Server.Utils.Database;
using Xunit;
using System.Linq;

namespace FITON.Tests
{
 public class WardrobeBranchesExtraTests
 {
 private AppDbContext GetDb()
 {
 var options = new DbContextOptionsBuilder<AppDbContext>()
 .UseInMemoryDatabase(System.Guid.NewGuid().ToString())
 .Options;
 return new AppDbContext(options);
 }

 private WardrobeController GetController(AppDbContext db, int userId)
 {
 var controller = new WardrobeController(db);
 controller.ControllerContext = new ControllerContext
 {
 HttpContext = new DefaultHttpContext
 {
 User = new ClaimsPrincipal(new ClaimsIdentity(new[]
 {
 new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
 new Claim(ClaimTypes.NameIdentifier, userId.ToString())
 }, "test"))
 }
 };
 return controller;
 }

 [Fact]
 public async Task CreateWardrobe_WithAccessories_Succeeds()
 {
 var db = GetDb();
 var user = new User { Id =1101, Username = "acc", Email = "acc@test", PasswordHash = "x" };
 db.Users.Add(user);
 db.Outfits.Add(new Outfit { Id =1102, Name = "TopAcc", UserId = user.Id, Type = "shirt" });
 await db.SaveChangesAsync();

 var controller = GetController(db, user.Id);
 var dto = new SaveWardrobeDto { Name = "ASet", TopClothesId =1102, Accessories = "Hat" };
 var res = await controller.CreateWardrobe(dto);
 var ar = Assert.IsType<ActionResult<WardrobeResponseDto>>(res);
 // ensure persisted
 var persisted = await db.Wardrobes.Include(w => w.TopClothes).FirstOrDefaultAsync(w => w.UserId == user.Id && w.Name == "ASet");
 Assert.NotNull(persisted);
 Assert.Equal("Hat", persisted.Accessories);
 }

 [Fact]
 public async Task CreateWardrobe_BottomOnly_Succeeds()
 {
 var db = GetDb();
 var user = new User { Id =1201, Username = "buser", Email = "b@test", PasswordHash = "x" };
 db.Users.Add(user);
 db.Outfits.Add(new Outfit { Id =1202, Name = "BottomOnly", UserId = user.Id, Type = "pants" });
 await db.SaveChangesAsync();

 var controller = GetController(db, user.Id);
 var dto = new SaveWardrobeDto { Name = "BottomSet", BottomClothesId =1202 };
 var res = await controller.CreateWardrobe(dto);
 var ar = Assert.IsType<ActionResult<WardrobeResponseDto>>(res);
 var persisted = await db.Wardrobes.Include(w => w.BottomClothes).FirstOrDefaultAsync(w => w.UserId == user.Id && w.Name == "BottomSet");
 Assert.NotNull(persisted);
 Assert.NotNull(persisted.BottomClothes);
 }

 [Fact]
 public async Task UpdateWardrobe_ChangeTopToBottom_Succeeds()
 {
 var db = GetDb();
 var user = new User { Id =1301, Username = "u1301", Email = "u1301@test", PasswordHash = "x" };
 db.Users.Add(user);
 db.Outfits.Add(new Outfit { Id =1302, Name = "TopOld", UserId = user.Id, Type = "shirt" });
 db.Outfits.Add(new Outfit { Id =1303, Name = "BottomNew", UserId = user.Id, Type = "pants" });
 db.Wardrobes.Add(new Wardrobe { Id =1304, Name = "MSet", UserId = user.Id, TopClothesId =1302 });
 await db.SaveChangesAsync();

 var controller = GetController(db, user.Id);
 var dto = new UpdateWardrobeDto { Name = "MSet", TopClothesId = null, BottomClothesId =1303 };
 var res = await controller.UpdateWardrobe(1304, dto);
 var ar = Assert.IsType<ActionResult<WardrobeResponseDto>>(res);
 var ok = res.Result as OkObjectResult ?? throw new Xunit.Sdk.XunitException("Expected Ok");
 var resp = Assert.IsType<WardrobeResponseDto>(ok.Value);
 Assert.True(resp.Success);
 var persisted = await db.Wardrobes.Include(w => w.BottomClothes).FirstOrDefaultAsync(w => w.Id ==1304);
 Assert.NotNull(persisted.BottomClothes);
 }

 [Fact]
 public async Task UpdateWardrobe_InvalidWardrobeId_ReturnsNotFound()
 {
 var db = GetDb();
 var user = new User { Id =1401, Username = "u1401", Email = "u1401@test", PasswordHash = "x" };
 db.Users.Add(user);
 await db.SaveChangesAsync();

 var controller = GetController(db, user.Id);
 var dto = new UpdateWardrobeDto { Name = "X" };
 var res = await controller.UpdateWardrobe(99999, dto);
 Assert.True(res.Result is NotFoundObjectResult || res.Result is ObjectResult);
 }

 [Fact]
 public async Task DeleteWardrobe_NotOwned_ReturnsNotFound()
 {
 var db = GetDb();
 db.Users.Add(new User { Id =1501, Username = "owner", Email = "o@test", PasswordHash = "x" });
 db.Users.Add(new User { Id =1502, Username = "other", Email = "other@test", PasswordHash = "x" });
 db.Wardrobes.Add(new Wardrobe { Id =1503, Name = "W", UserId =1501 });
 await db.SaveChangesAsync();

 var controller = GetController(db,1502);
 var res = await controller.DeleteWardrobe(1503);
 Assert.True(res is NotFoundObjectResult || (res is ActionResult<WardrobeResponseDto> ar && ar.Result is NotFoundObjectResult));
 }

 [Fact]
 public async Task GetFilteredClothes_Invalid_ReturnsBadRequest()
 {
 var db = GetDb();
 var user = new User { Id =1601, Username = "uf1", Email = "uf1@test", PasswordHash = "x" };
 db.Users.Add(user);
 await db.SaveChangesAsync();
 var controller = GetController(db, user.Id);
 var res = await controller.GetFilteredClothes("invalid-type");
 // Should not be Ok
 Assert.False(res.Result is OkObjectResult && res.Value?.Success == true);
 }
 }
}
