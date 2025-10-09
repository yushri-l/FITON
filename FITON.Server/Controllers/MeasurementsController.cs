using FITON.Server.DTOs;
using FITON.Server.Models;
using FITON.Server.Services;
using FITON.Server.Utils.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace FITON.Server.Controllers
{
    [Authorize]
    [Route("api/avatar/[controller]")]
    [ApiController]
    public class MeasurementsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IAvatarService _avatarService;
        private readonly ILogger<MeasurementsController> _logger;

        public MeasurementsController(
            AppDbContext context,
            IAvatarService avatarService,
            ILogger<MeasurementsController> logger)
        {
            _context = context;
            _avatarService = avatarService;
            _logger = logger;
        }

        private int GetUserIdFromToken()
        {
            // Try multiple possible claim locations
            var userIdStr = User.FindFirstValue(JwtRegisteredClaimNames.Sub) ??
                           User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                           User.FindFirstValue("userid") ??
                           User.FindFirstValue("id");

            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId))
            {
                // Log detailed information for debugging
                var claims = User.Claims.Select(c => $"{c.Type}: {c.Value}").ToList();
                Console.WriteLine($"Available claims: {string.Join(", ", claims)}");
                throw new UnauthorizedAccessException($"User ID not found in token. Available claims: {string.Join(", ", claims)}");
            }

            return userId;
        }

        [HttpGet("retrieve")]
        public async Task<IActionResult> GetMeasurements()
        {
            try
            {
                Console.WriteLine("=== FETCH MEASUREMENTS START ===");

                var userId = GetUserIdFromToken();
                Console.WriteLine($"User ID from token: {userId}");

                var measurement = await _context.Measurements
                    .AsNoTracking()
                    .FirstOrDefaultAsync(m => m.UserId == userId);

                Console.WriteLine($"Measurement found: {measurement != null}");

                if (measurement == null)
                {
                    Console.WriteLine("No measurements found for user");
                    return NotFound("No measurements found for this user.");
                }

                Console.WriteLine($"Returning measurement: {System.Text.Json.JsonSerializer.Serialize(measurement)}");
                Console.WriteLine("=== FETCH MEASUREMENTS END ===");

                return Ok(measurement);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR in GetMeasurements: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }


        [HttpPost("save")]
        public async Task<IActionResult> SaveMeasurements([FromBody] MeasurementDto dto)
        {
            Console.WriteLine("Received DTO: " + System.Text.Json.JsonSerializer.Serialize(dto));
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = GetUserIdFromToken();
            var existing = await _context.Measurements.FirstOrDefaultAsync(m => m.UserId == userId);

            if (existing != null)
            {
                // Update all optional fields dynamically
                existing.Height = dto.Height;
                existing.Weight = dto.Weight;
                existing.Chest = dto.Chest;
                existing.Waist = dto.Waist;
                existing.Hips = dto.Hips;
                existing.Shoulders = dto.Shoulders;
                existing.NeckCircumference = dto.NeckCircumference;
                existing.SleeveLength = dto.SleeveLength;
                existing.Inseam = dto.Inseam;
                existing.Thigh = dto.Thigh;
                existing.SkinColor = dto.SkinColor;
                existing.Description = dto.Description;

                _context.Measurements.Update(existing);
                await _context.SaveChangesAsync();
                if (ShouldRegenerateAvatar(oldHeight, oldWeight, oldSkinColor, dto.Height, dto.Weight, dto.SkinColor))
                {
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            var newAvatarUrl = await _avatarService.GenerateAvatarFromMeasurementsAsync(existing);
                            if (!string.IsNullOrEmpty(newAvatarUrl))
                            {
                                existing.AvatarUrl = newAvatarUrl;
                                _context.Measurements.Update(existing);
                                await _context.SaveChangesAsync();
                                _logger.LogInformation("Avatar regenerated for user {UserId}", userId);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to regenerate avatar for user {UserId}", userId);
                        }
                    });
                }
                return Ok(existing);
            }

            var newMeasurement = new Measurement
            {
                UserId = userId,
                Height = dto.Height,
                Weight = dto.Weight,
                Chest = dto.Chest,
                Waist = dto.Waist,
                Hips = dto.Hips,
                Shoulders = dto.Shoulders,
                NeckCircumference = dto.NeckCircumference,
                SleeveLength = dto.SleeveLength,
                Inseam = dto.Inseam,
                Thigh = dto.Thigh,
                SkinColor = dto.SkinColor,
                Description = dto.Description
            };

            await _context.Measurements.AddAsync(newMeasurement);
            await _context.SaveChangesAsync();
            // Generate avatar for new measurements (async - don't block response)
            _ = Task.Run(async () =>
            {
                try
                {
                    var avatarUrl = await _avatarService.GenerateAvatarFromMeasurementsAsync(newMeasurement);
                    if (!string.IsNullOrEmpty(avatarUrl))
                    {
                        newMeasurement.AvatarUrl = avatarUrl;
                        _context.Measurements.Update(newMeasurement);
                        await _context.SaveChangesAsync();
                        _logger.LogInformation("Avatar generated for new user {UserId}", userId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to generate avatar for new user {UserId}", userId);
                }
            });

            return Ok(newMeasurement);
        }

        [HttpDelete("remove")]
        public async Task<IActionResult> DeleteMeasurements()
        {
            var userId = GetUserIdFromToken();
            var measurement = await _context.Measurements.FirstOrDefaultAsync(m => m.UserId == userId);
            if (measurement == null) return NotFound("No measurements to delete.");

            _context.Measurements.Remove(measurement);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Measurements deleted successfully." });
        }
    }
}
