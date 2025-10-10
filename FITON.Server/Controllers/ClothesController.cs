using FITON.Server.DTOs;
using FITON.Server.Models;
using FITON.Server.Utils.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ClothesController : ControllerBase
{
    private readonly AppDbContext _db;

    public ClothesController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetOutfits()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized("Invalid user authentication");
            }

            // Get user's outfits from database
            var outfits = await _db.Outfits
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .Select(o => new OutfitResponseDto
                {
                    Id = o.Id,
                    Name = o.Name,
                    Description = o.Description,
                    Category = o.Category,
                    Brand = o.Brand,
                    Size = o.Size,
                    Color = o.Color,
                    Type = o.Type,
                    Image = o.Image,
                    CreatedAt = o.CreatedAt,
                    UpdatedAt = o.UpdatedAt
                })
                .ToListAsync();

            return Ok(outfits);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to retrieve outfits", details = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> SaveOutfit([FromBody] SaveOutfitDto dto)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized("Invalid user authentication");
            }

            // Create new outfit and save to database
            var outfit = new Outfit
            {
                Name = dto.Name,
                Description = dto.Description,
                Category = dto.Category,
                Brand = dto.Brand,
                Size = dto.Size,
                Color = dto.Color,
                Type = dto.Type,
                Image = dto.Image,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _db.Outfits.Add(outfit);
            await _db.SaveChangesAsync();

            var savedOutfit = new OutfitResponseDto
            {
                Id = outfit.Id,
                Name = outfit.Name,
                Description = outfit.Description,
                Category = outfit.Category,
                Brand = outfit.Brand,
                Size = outfit.Size,
                Color = outfit.Color,
                Type = outfit.Type,
                Image = outfit.Image,
                CreatedAt = outfit.CreatedAt,
                UpdatedAt = outfit.UpdatedAt
            };

            return Ok(savedOutfit);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to save outfit", details = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateOutfit(int id, [FromBody] SaveOutfitDto dto)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized("Invalid user authentication");
            }

            // Find the outfit and verify ownership
            var outfit = await _db.Outfits
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

            if (outfit == null)
            {
                return NotFound("Outfit not found or you don't have permission to update it");
            }

            // Update outfit properties
            outfit.Name = dto.Name;
            outfit.Description = dto.Description;
            outfit.Category = dto.Category;
            outfit.Brand = dto.Brand;
            outfit.Size = dto.Size;
            outfit.Color = dto.Color;
            outfit.Type = dto.Type;
            outfit.Image = dto.Image;
            outfit.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            var updatedOutfit = new OutfitResponseDto
            {
                Id = outfit.Id,
                Name = outfit.Name,
                Description = outfit.Description,
                Category = outfit.Category,
                Brand = outfit.Brand,
                Size = outfit.Size,
                Color = outfit.Color,
                Type = outfit.Type,
                Image = outfit.Image,
                CreatedAt = outfit.CreatedAt,
                UpdatedAt = outfit.UpdatedAt
            };

            return Ok(updatedOutfit);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to update outfit", details = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteOutfit(int id)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized("Invalid user authentication");
            }

            // Find the outfit and verify ownership
            var outfit = await _db.Outfits
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

            if (outfit == null)
            {
                return NotFound("Outfit not found or you don't have permission to delete it");
            }

            _db.Outfits.Remove(outfit);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Outfit deleted successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to delete outfit", details = ex.Message });
        }
    }

    // TEMPORARY: Add sample clothes data for testing
    [HttpPost("seed-sample-data")]
    public async Task<IActionResult> SeedSampleData()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized("Invalid user authentication");
            }

            // Check if user already has clothes to avoid duplicates
            var existingClothes = await _db.Outfits.Where(o => o.UserId == userId).CountAsync();
            if (existingClothes > 0)
            {
                return BadRequest("Sample data already exists. Please delete existing clothes first if you want to reseed.");
            }

            var sampleClothes = new List<Outfit>
            {
                // Tops
                new Outfit { Name = "Classic White Shirt", Description = "A timeless white button-down shirt perfect for any occasion", Category = "Business", Brand = "FITON", Size = "M", Color = "White", Type = "shirt", Image = "", UserId = userId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Outfit { Name = "Casual Blue T-Shirt", Description = "Comfortable cotton t-shirt for everyday wear", Category = "Casual", Brand = "FITON", Size = "M", Color = "Blue", Type = "t-shirt", Image = "", UserId = userId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Outfit { Name = "Elegant Black Blouse", Description = "Sophisticated blouse perfect for formal events", Category = "Formal", Brand = "FITON", Size = "M", Color = "Black", Type = "blouse", Image = "", UserId = userId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Outfit { Name = "Cozy Gray Sweater", Description = "Warm and comfortable sweater for cool weather", Category = "Casual", Brand = "FITON", Size = "M", Color = "Gray", Type = "sweater", Image = "", UserId = userId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Outfit { Name = "Navy Blue Blazer", Description = "Professional blazer for business meetings", Category = "Business", Brand = "FITON", Size = "M", Color = "Navy", Type = "blazer", Image = "", UserId = userId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },

                // Bottoms
                new Outfit { Name = "Dark Blue Jeans", Description = "Classic straight-leg jeans that go with everything", Category = "Casual", Brand = "FITON", Size = "M", Color = "Dark Blue", Type = "jeans", Image = "", UserId = userId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Outfit { Name = "Black Formal Pants", Description = "Tailored pants perfect for business attire", Category = "Business", Brand = "FITON", Size = "M", Color = "Black", Type = "pants", Image = "", UserId = userId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Outfit { Name = "Khaki Chinos", Description = "Versatile chino pants for smart casual looks", Category = "Casual", Brand = "FITON", Size = "M", Color = "Khaki", Type = "pants", Image = "", UserId = userId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Outfit { Name = "Pleated Mini Skirt", Description = "Trendy pleated skirt for a youthful look", Category = "Casual", Brand = "FITON", Size = "M", Color = "Navy", Type = "skirt", Image = "", UserId = userId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Outfit { Name = "Black Leggings", Description = "Comfortable stretch leggings for active wear", Category = "Sport", Brand = "FITON", Size = "M", Color = "Black", Type = "leggings", Image = "", UserId = userId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },

                // Full Outfits
                new Outfit { Name = "Little Black Dress", Description = "Classic black dress suitable for any formal occasion", Category = "Formal", Brand = "FITON", Size = "M", Color = "Black", Type = "dress", Image = "", UserId = userId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Outfit { Name = "Floral Summer Dress", Description = "Light and airy dress perfect for summer days", Category = "Casual", Brand = "FITON", Size = "M", Color = "Floral", Type = "dress", Image = "", UserId = userId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Outfit { Name = "Elegant Evening Gown", Description = "Stunning gown for special occasions and events", Category = "Formal", Brand = "FITON", Size = "M", Color = "Burgundy", Type = "gown", Image = "", UserId = userId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Outfit { Name = "Casual Denim Jumpsuit", Description = "Trendy jumpsuit for a modern casual look", Category = "Casual", Brand = "FITON", Size = "M", Color = "Blue", Type = "jumpsuit", Image = "", UserId = userId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Outfit { Name = "Bohemian Maxi Frock", Description = "Free-flowing frock with bohemian style", Category = "Casual", Brand = "FITON", Size = "M", Color = "Multicolor", Type = "frock", Image = "", UserId = userId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
            };

            await _db.Outfits.AddRangeAsync(sampleClothes);
            await _db.SaveChangesAsync();

            return Ok(new { 
                message = "Sample clothes data added successfully!", 
                count = sampleClothes.Count,
                details = "You now have sample tops, bottoms, and full outfits to test the wardrobe feature."
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to add sample data", details = ex.Message });
        }
    }
}