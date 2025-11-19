using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FITON.Server.Models;
using FITON.Server.Utils.Database;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FITON.Tests
{
 public class BulkDbTests
 {
 private AppDbContext CreateDb()
 {
 var options = new DbContextOptionsBuilder<AppDbContext>()
 .UseInMemoryDatabase(System.Guid.NewGuid().ToString())
 .Options;
 return new AppDbContext(options);
 }

 public static IEnumerable<object[]> UserData()
 {
 // produce60 distinct user test cases
 for (int i =1; i <=60; i++)
 {
 yield return new object[] { $"bulk_user_{i}", $"bulk_{i}@example.test" };
 }
 }

 [Theory]
 [MemberData(nameof(UserData))]
 public async Task AddUser_AndRetrieve_ByEmail(string username, string email)
 {
 using var db = CreateDb();
 var user = new User { Username = username, Email = email, PasswordHash = "x" };
 db.Users.Add(user);
 await db.SaveChangesAsync();

 var found = await db.Users.FirstOrDefaultAsync(u => u.Email == email);
 Assert.NotNull(found);
 Assert.Equal(username, found.Username);
 }

 public static IEnumerable<object[]> OutfitData()
 {
 // produce30 outfit cases combining indices
 for (int i =1; i <=30; i++)
 {
 yield return new object[] { i, $"Outfit {i}", $"Category{i %5}", $"Brand{i %3}", $"Size{i %4}" , $"Color{i %6}" };
 }
 }

 [Theory]
 [MemberData(nameof(OutfitData))]
 public async Task AddOutfit_AssignsUserAndRetrieves(int userId, string name, string category, string brand, string size, string color)
 {
 using var db = CreateDb();
 // ensure user exists
 db.Users.Add(new User { Id = userId, Username = $"u{userId}", Email = $"u{userId}@test.local", PasswordHash = "x" });
 await db.SaveChangesAsync();

 var outfit = new Outfit { Name = name, Category = category, Brand = brand, Size = size, Color = color, Type = "shirt", UserId = userId };
 db.Outfits.Add(outfit);
 await db.SaveChangesAsync();

 var found = await db.Outfits.FirstOrDefaultAsync(o => o.UserId == userId && o.Name == name);
 Assert.NotNull(found);
 Assert.Equal(color, found.Color);
 }

 public static IEnumerable<object[]> WardrobeData()
 {
 // produce25 wardrobe cases
 for (int i =1; i <=25; i++)
 {
 yield return new object[] { i +1000, $"Wardrobe {i}", i %2 ==0 ? (int?)null : i +2000 };
 }
 }

 [Theory]
 [MemberData(nameof(WardrobeData))]
 public async Task AddWardrobe_WithOptionalTop_ReturnsOk(int userId, string name, int? topOutfitId)
 {
 using var db = CreateDb();
 db.Users.Add(new User { Id = userId, Username = $"wu{userId}", Email = $"wu{userId}@test.local", PasswordHash = "x" });
 if (topOutfitId.HasValue)
 {
 db.Outfits.Add(new Outfit { Id = topOutfitId.Value, Name = "Top", UserId = userId, Type = "shirt" });
 }
 await db.SaveChangesAsync();

 var w = new Wardrobe { Name = name, UserId = userId, TopClothesId = topOutfitId };
 db.Wardrobes.Add(w);
 await db.SaveChangesAsync();

 var found = await db.Wardrobes.FirstOrDefaultAsync(x => x.UserId == userId && x.Name == name);
 Assert.NotNull(found);
 Assert.Equal(topOutfitId, found.TopClothesId);
 }
 }
}
