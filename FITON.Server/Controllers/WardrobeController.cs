using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FITON.Server.Utils.Database;
using FITON.Server.Models;
using FITON.Server.DTOs;
using System.Security.Claims;

namespace FITON.Server.Controllers
{
    [ApiController]
    [Route("api/wardrobe")]
    [Authorize]
    public class WardrobeController : ControllerBase
    {
        private readonly AppDbContext _context;

        public WardrobeController(AppDbContext context)
        {
            _context = context;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out int userId))
            {
                return userId;
            }
            throw new UnauthorizedAccessException("Invalid user ID");
        }

        // GET: api/wardrobe
        [HttpGet]
        public async Task<ActionResult<WardrobeListResponseDto>> GetWardrobes()
        {
            try
            {
                var userId = GetCurrentUserId();
                var wardrobes = await _context.Wardrobes
                    .Where(w => w.UserId == userId)
                    .OrderByDescending(w => w.UpdatedAt)
                    .ToListAsync();

                var wardrobeDtos = new List<WardrobeDto>();

                foreach (var wardrobe in wardrobes)
                {
                    var wardrobeDto = new WardrobeDto
                    {
                        Id = wardrobe.Id,
                        Name = wardrobe.Name,
                        Description = wardrobe.Description,
                        UserId = wardrobe.UserId,
                        TopClothesId = wardrobe.TopClothesId,
                        BottomClothesId = wardrobe.BottomClothesId,
                        FullOutfitClothesId = wardrobe.FullOutfitClothesId,
                        Accessories = wardrobe.Accessories,
                        Occasion = wardrobe.Occasion,
                        Season = wardrobe.Season,
                        CreatedAt = wardrobe.CreatedAt,
                        UpdatedAt = wardrobe.UpdatedAt
                    };

                    // Populate clothing details
                    if (wardrobe.TopClothesId.HasValue)
                    {
                        var topClothes = await _context.Outfits.FindAsync(wardrobe.TopClothesId.Value);
                        if (topClothes != null)
                        {
                            wardrobeDto.TopClothes = new OutfitDto
                            {
                                Id = topClothes.Id,
                                Name = topClothes.Name,
                                Description = topClothes.Description,
                                Category = topClothes.Category,
                                Brand = topClothes.Brand,
                                Size = topClothes.Size,
                                Color = topClothes.Color,
                                Type = topClothes.Type,
                                Image = topClothes.Image,
                                CreatedAt = topClothes.CreatedAt,
                                UpdatedAt = topClothes.UpdatedAt,
                                UserId = topClothes.UserId
                            };
                        }
                    }

                    if (wardrobe.BottomClothesId.HasValue)
                    {
                        var bottomClothes = await _context.Outfits.FindAsync(wardrobe.BottomClothesId.Value);
                        if (bottomClothes != null)
                        {
                            wardrobeDto.BottomClothes = new OutfitDto
                            {
                                Id = bottomClothes.Id,
                                Name = bottomClothes.Name,
                                Description = bottomClothes.Description,
                                Category = bottomClothes.Category,
                                Brand = bottomClothes.Brand,
                                Size = bottomClothes.Size,
                                Color = bottomClothes.Color,
                                Type = bottomClothes.Type,
                                Image = bottomClothes.Image,
                                CreatedAt = bottomClothes.CreatedAt,
                                UpdatedAt = bottomClothes.UpdatedAt,
                                UserId = bottomClothes.UserId
                            };
                        }
                    }

                    if (wardrobe.FullOutfitClothesId.HasValue)
                    {
                        var fullOutfitClothes = await _context.Outfits.FindAsync(wardrobe.FullOutfitClothesId.Value);
                        if (fullOutfitClothes != null)
                        {
                            wardrobeDto.FullOutfitClothes = new OutfitDto
                            {
                                Id = fullOutfitClothes.Id,
                                Name = fullOutfitClothes.Name,
                                Description = fullOutfitClothes.Description,
                                Category = fullOutfitClothes.Category,
                                Brand = fullOutfitClothes.Brand,
                                Size = fullOutfitClothes.Size,
                                Color = fullOutfitClothes.Color,
                                Type = fullOutfitClothes.Type,
                                Image = fullOutfitClothes.Image,
                                CreatedAt = fullOutfitClothes.CreatedAt,
                                UpdatedAt = fullOutfitClothes.UpdatedAt,
                                UserId = fullOutfitClothes.UserId
                            };
                        }
                    }

                    wardrobeDtos.Add(wardrobeDto);
                }

                return Ok(new WardrobeListResponseDto
                {
                    Success = true,
                    Message = "Wardrobes retrieved successfully",
                    Data = wardrobeDtos
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new WardrobeListResponseDto
                {
                    Success = false,
                    Message = $"Error retrieving wardrobes: {ex.Message}"
                });
            }
        }

        // GET: api/wardrobe/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<WardrobeResponseDto>> GetWardrobe(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var wardrobe = await _context.Wardrobes
                    .FirstOrDefaultAsync(w => w.Id == id && w.UserId == userId);

                if (wardrobe == null)
                {
                    return NotFound(new WardrobeResponseDto
                    {
                        Success = false,
                        Message = "Wardrobe not found"
                    });
                }

                var wardrobeDto = new WardrobeDto
                {
                    Id = wardrobe.Id,
                    Name = wardrobe.Name,
                    Description = wardrobe.Description,
                    UserId = wardrobe.UserId,
                    TopClothesId = wardrobe.TopClothesId,
                    BottomClothesId = wardrobe.BottomClothesId,
                    FullOutfitClothesId = wardrobe.FullOutfitClothesId,
                    Accessories = wardrobe.Accessories,
                    Occasion = wardrobe.Occasion,
                    Season = wardrobe.Season,
                    CreatedAt = wardrobe.CreatedAt,
                    UpdatedAt = wardrobe.UpdatedAt
                };

                // Populate clothing details
                if (wardrobe.TopClothesId.HasValue)
                {
                    var topClothes = await _context.Outfits.FindAsync(wardrobe.TopClothesId.Value);
                    if (topClothes != null)
                    {
                        wardrobeDto.TopClothes = new OutfitDto
                        {
                            Id = topClothes.Id,
                            Name = topClothes.Name,
                            Description = topClothes.Description,
                            Category = topClothes.Category,
                            Brand = topClothes.Brand,
                            Size = topClothes.Size,
                            Color = topClothes.Color,
                            Type = topClothes.Type,
                            Image = topClothes.Image,
                            CreatedAt = topClothes.CreatedAt,
                            UpdatedAt = topClothes.UpdatedAt,
                            UserId = topClothes.UserId
                        };
                    }
                }

                if (wardrobe.BottomClothesId.HasValue)
                {
                    var bottomClothes = await _context.Outfits.FindAsync(wardrobe.BottomClothesId.Value);
                    if (bottomClothes != null)
                    {
                        wardrobeDto.BottomClothes = new OutfitDto
                        {
                            Id = bottomClothes.Id,
                            Name = bottomClothes.Name,
                            Description = bottomClothes.Description,
                            Category = bottomClothes.Category,
                            Brand = bottomClothes.Brand,
                            Size = bottomClothes.Size,
                            Color = bottomClothes.Color,
                            Type = bottomClothes.Type,
                            Image = bottomClothes.Image,
                            CreatedAt = bottomClothes.CreatedAt,
                            UpdatedAt = bottomClothes.UpdatedAt,
                            UserId = bottomClothes.UserId
                        };
                    }
                }

                if (wardrobe.FullOutfitClothesId.HasValue)
                {
                    var fullOutfitClothes = await _context.Outfits.FindAsync(wardrobe.FullOutfitClothesId.Value);
                    if (fullOutfitClothes != null)
                    {
                        wardrobeDto.FullOutfitClothes = new OutfitDto
                        {
                            Id = fullOutfitClothes.Id,
                            Name = fullOutfitClothes.Name,
                            Description = fullOutfitClothes.Description,
                            Category = fullOutfitClothes.Category,
                            Brand = fullOutfitClothes.Brand,
                            Size = fullOutfitClothes.Size,
                            Color = fullOutfitClothes.Color,
                            Type = fullOutfitClothes.Type,
                            Image = fullOutfitClothes.Image,
                            CreatedAt = fullOutfitClothes.CreatedAt,
                            UpdatedAt = fullOutfitClothes.UpdatedAt,
                            UserId = fullOutfitClothes.UserId
                        };
                    }
                }

                return Ok(new WardrobeResponseDto
                {
                    Success = true,
                    Message = "Wardrobe retrieved successfully",
                    Data = wardrobeDto
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new WardrobeResponseDto
                {
                    Success = false,
                    Message = $"Error retrieving wardrobe: {ex.Message}"
                });
            }
        }

        // POST: api/wardrobe
        [HttpPost]
        public async Task<ActionResult<WardrobeResponseDto>> CreateWardrobe([FromBody] SaveWardrobeDto saveWardrobeDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage));
                    return BadRequest(new WardrobeResponseDto
                    {
                        Success = false,
                        Message = $"Validation failed: {string.Join(", ", errors)}"
                    });
                }

                var userId = GetCurrentUserId();

                // Validate that at least one clothing item is selected
                if (!saveWardrobeDto.TopClothesId.HasValue && 
                    !saveWardrobeDto.BottomClothesId.HasValue && 
                    !saveWardrobeDto.FullOutfitClothesId.HasValue)
                {
                    return BadRequest(new WardrobeResponseDto
                    {
                        Success = false,
                        Message = "At least one clothing item must be selected"
                    });
                }

                // Validate that selected clothes exist and belong to the user
                if (saveWardrobeDto.TopClothesId.HasValue)
                {
                    var topClothes = await _context.Outfits.FindAsync(saveWardrobeDto.TopClothesId.Value);
                    if (topClothes == null || topClothes.UserId != userId)
                    {
                        return BadRequest(new WardrobeResponseDto
                        {
                            Success = false,
                            Message = "Selected top clothes not found or doesn't belong to user"
                        });
                    }
                }

                if (saveWardrobeDto.BottomClothesId.HasValue)
                {
                    var bottomClothes = await _context.Outfits.FindAsync(saveWardrobeDto.BottomClothesId.Value);
                    if (bottomClothes == null || bottomClothes.UserId != userId)
                    {
                        return BadRequest(new WardrobeResponseDto
                        {
                            Success = false,
                            Message = "Selected bottom clothes not found or doesn't belong to user"
                        });
                    }
                }

                if (saveWardrobeDto.FullOutfitClothesId.HasValue)
                {
                    var fullOutfitClothes = await _context.Outfits.FindAsync(saveWardrobeDto.FullOutfitClothesId.Value);
                    if (fullOutfitClothes == null || fullOutfitClothes.UserId != userId)
                    {
                        return BadRequest(new WardrobeResponseDto
                        {
                            Success = false,
                            Message = "Selected full outfit clothes not found or doesn't belong to user"
                        });
                    }
                }

                var wardrobe = new Wardrobe
                {
                    Name = saveWardrobeDto.Name,
                    Description = saveWardrobeDto.Description,
                    UserId = userId,
                    TopClothesId = saveWardrobeDto.TopClothesId,
                    BottomClothesId = saveWardrobeDto.BottomClothesId,
                    FullOutfitClothesId = saveWardrobeDto.FullOutfitClothesId,
                    Accessories = saveWardrobeDto.Accessories,
                    Occasion = saveWardrobeDto.Occasion,
                    Season = saveWardrobeDto.Season,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Wardrobes.Add(wardrobe);
                await _context.SaveChangesAsync();

                var wardrobeDto = new WardrobeDto
                {
                    Id = wardrobe.Id,
                    Name = wardrobe.Name,
                    Description = wardrobe.Description,
                    UserId = wardrobe.UserId,
                    TopClothesId = wardrobe.TopClothesId,
                    BottomClothesId = wardrobe.BottomClothesId,
                    FullOutfitClothesId = wardrobe.FullOutfitClothesId,
                    Accessories = wardrobe.Accessories,
                    Occasion = wardrobe.Occasion,
                    Season = wardrobe.Season,
                    CreatedAt = wardrobe.CreatedAt,
                    UpdatedAt = wardrobe.UpdatedAt
                };

                return Ok(new WardrobeResponseDto
                {
                    Success = true,
                    Message = "Wardrobe created successfully",
                    Data = wardrobeDto
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new WardrobeResponseDto
                {
                    Success = false,
                    Message = $"Error creating wardrobe: {ex.Message}"
                });
            }
        }

        // PUT: api/wardrobe/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<WardrobeResponseDto>> UpdateWardrobe(int id, [FromBody] UpdateWardrobeDto updateWardrobeDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage));
                    return BadRequest(new WardrobeResponseDto
                    {
                        Success = false,
                        Message = $"Validation failed: {string.Join(", ", errors)}"
                    });
                }

                var userId = GetCurrentUserId();
                var wardrobe = await _context.Wardrobes.FirstOrDefaultAsync(w => w.Id == id && w.UserId == userId);

                if (wardrobe == null)
                {
                    return NotFound(new WardrobeResponseDto
                    {
                        Success = false,
                        Message = "Wardrobe not found"
                    });
                }

                // Validate that at least one clothing item is selected
                if (!updateWardrobeDto.TopClothesId.HasValue && 
                    !updateWardrobeDto.BottomClothesId.HasValue && 
                    !updateWardrobeDto.FullOutfitClothesId.HasValue)
                {
                    return BadRequest(new WardrobeResponseDto
                    {
                        Success = false,
                        Message = "At least one clothing item must be selected"
                    });
                }

                // Validate that selected clothes exist and belong to the user
                if (updateWardrobeDto.TopClothesId.HasValue)
                {
                    var topClothes = await _context.Outfits.FindAsync(updateWardrobeDto.TopClothesId.Value);
                    if (topClothes == null || topClothes.UserId != userId)
                    {
                        return BadRequest(new WardrobeResponseDto
                        {
                            Success = false,
                            Message = "Selected top clothes not found or doesn't belong to user"
                        });
                    }
                }

                if (updateWardrobeDto.BottomClothesId.HasValue)
                {
                    var bottomClothes = await _context.Outfits.FindAsync(updateWardrobeDto.BottomClothesId.Value);
                    if (bottomClothes == null || bottomClothes.UserId != userId)
                    {
                        return BadRequest(new WardrobeResponseDto
                        {
                            Success = false,
                            Message = "Selected bottom clothes not found or doesn't belong to user"
                        });
                    }
                }

                if (updateWardrobeDto.FullOutfitClothesId.HasValue)
                {
                    var fullOutfitClothes = await _context.Outfits.FindAsync(updateWardrobeDto.FullOutfitClothesId.Value);
                    if (fullOutfitClothes == null || fullOutfitClothes.UserId != userId)
                    {
                        return BadRequest(new WardrobeResponseDto
                        {
                            Success = false,
                            Message = "Selected full outfit clothes not found or doesn't belong to user"
                        });
                    }
                }

                wardrobe.Name = updateWardrobeDto.Name;
                wardrobe.Description = updateWardrobeDto.Description;
                wardrobe.TopClothesId = updateWardrobeDto.TopClothesId;
                wardrobe.BottomClothesId = updateWardrobeDto.BottomClothesId;
                wardrobe.FullOutfitClothesId = updateWardrobeDto.FullOutfitClothesId;
                wardrobe.Accessories = updateWardrobeDto.Accessories;
                wardrobe.Occasion = updateWardrobeDto.Occasion;
                wardrobe.Season = updateWardrobeDto.Season;
                wardrobe.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var wardrobeDto = new WardrobeDto
                {
                    Id = wardrobe.Id,
                    Name = wardrobe.Name,
                    Description = wardrobe.Description,
                    UserId = wardrobe.UserId,
                    TopClothesId = wardrobe.TopClothesId,
                    BottomClothesId = wardrobe.BottomClothesId,
                    FullOutfitClothesId = wardrobe.FullOutfitClothesId,
                    Accessories = wardrobe.Accessories,
                    Occasion = wardrobe.Occasion,
                    Season = wardrobe.Season,
                    CreatedAt = wardrobe.CreatedAt,
                    UpdatedAt = wardrobe.UpdatedAt
                };

                return Ok(new WardrobeResponseDto
                {
                    Success = true,
                    Message = "Wardrobe updated successfully",
                    Data = wardrobeDto
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new WardrobeResponseDto
                {
                    Success = false,
                    Message = $"Error updating wardrobe: {ex.Message}"
                });
            }
        }

        // DELETE: api/wardrobe/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult<WardrobeResponseDto>> DeleteWardrobe(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var wardrobe = await _context.Wardrobes.FirstOrDefaultAsync(w => w.Id == id && w.UserId == userId);

                if (wardrobe == null)
                {
                    return NotFound(new WardrobeResponseDto
                    {
                        Success = false,
                        Message = "Wardrobe not found"
                    });
                }

                _context.Wardrobes.Remove(wardrobe);
                await _context.SaveChangesAsync();

                return Ok(new WardrobeResponseDto
                {
                    Success = true,
                    Message = "Wardrobe deleted successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new WardrobeResponseDto
                {
                    Success = false,
                    Message = $"Error deleting wardrobe: {ex.Message}"
                });
            }
        }

        // GET: api/wardrobe/clothes/filtered?type={type}
        [HttpGet("clothes/filtered")]
        public async Task<ActionResult<OutfitListResponseDto>> GetFilteredClothes([FromQuery] string type)
        {
            try
            {
                var userId = GetCurrentUserId();
                
                var topTypes = new[] { "shirt", "blouse", "t-shirt", "tank-top", "sweater", "cardigan", "jacket", "blazer" };
                var bottomTypes = new[] { "pants", "jeans", "skirt", "shorts", "leggings", "trousers" };
                var fullTypes = new[] { "dress", "frock", "gown", "jumpsuit", "romper", "overall" };

                IQueryable<Outfit> query = _context.Outfits.Where(o => o.UserId == userId);

                switch (type.ToLower())
                {
                    case "top":
                        query = query.Where(o => topTypes.Contains(o.Type.ToLower()));
                        break;
                    case "bottom":
                        query = query.Where(o => bottomTypes.Contains(o.Type.ToLower()));
                        break;
                    case "full":
                        query = query.Where(o => fullTypes.Contains(o.Type.ToLower()));
                        break;
                    default:
                        return BadRequest(new OutfitListResponseDto
                        {
                            Success = false,
                            Message = "Invalid type. Use 'top', 'bottom', or 'full'"
                        });
                }

                var clothes = await query.OrderByDescending(o => o.UpdatedAt).ToListAsync();

                var clothesDtos = clothes.Select(c => new OutfitDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    Category = c.Category,
                    Brand = c.Brand,
                    Size = c.Size,
                    Color = c.Color,
                    Type = c.Type,
                    Image = c.Image,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt,
                    UserId = c.UserId
                }).ToList();

                return Ok(new OutfitListResponseDto
                {
                    Success = true,
                    Message = $"Filtered clothes retrieved successfully",
                    Data = clothesDtos
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new OutfitListResponseDto
                {
                    Success = false,
                    Message = $"Error retrieving filtered clothes: {ex.Message}"
                });
            }
        }
    }
}