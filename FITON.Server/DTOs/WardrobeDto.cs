using System.ComponentModel.DataAnnotations;

namespace FITON.Server.DTOs
{
    public class WardrobeDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = "";
        public int UserId { get; set; }
        
        // Clothing items
        public int? TopClothesId { get; set; }
        public int? BottomClothesId { get; set; }
        public int? FullOutfitClothesId { get; set; }
        
        // Clothing details (populated from Clothes)
        public OutfitDto? TopClothes { get; set; }
        public OutfitDto? BottomClothes { get; set; }
        public OutfitDto? FullOutfitClothes { get; set; }
        
        public string Accessories { get; set; } = "";
        public string Occasion { get; set; } = "Casual";
        public string Season { get; set; } = "All";
        
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class SaveWardrobeDto
    {
        [Required]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Name must be between 1 and 100 characters")]
        public string Name { get; set; } = null!;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string Description { get; set; } = "";

        // At least one clothing item must be selected
        public int? TopClothesId { get; set; }
        public int? BottomClothesId { get; set; }
        public int? FullOutfitClothesId { get; set; }

        [StringLength(200, ErrorMessage = "Accessories cannot exceed 200 characters")]
        public string Accessories { get; set; } = "";

        [StringLength(50, ErrorMessage = "Occasion cannot exceed 50 characters")]
        public string Occasion { get; set; } = "Casual";

        [StringLength(20, ErrorMessage = "Season cannot exceed 20 characters")]
        public string Season { get; set; } = "All";
    }

    public class UpdateWardrobeDto
    {
        [Required]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Name must be between 1 and 100 characters")]
        public string Name { get; set; } = null!;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string Description { get; set; } = "";

        public int? TopClothesId { get; set; }
        public int? BottomClothesId { get; set; }
        public int? FullOutfitClothesId { get; set; }

        [StringLength(200, ErrorMessage = "Accessories cannot exceed 200 characters")]
        public string Accessories { get; set; } = "";

        [StringLength(50, ErrorMessage = "Occasion cannot exceed 50 characters")]
        public string Occasion { get; set; } = "Casual";

        [StringLength(20, ErrorMessage = "Season cannot exceed 20 characters")]
        public string Season { get; set; } = "All";
    }

    public class WardrobeResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public WardrobeDto? Data { get; set; }
    }

    public class WardrobeListResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public List<WardrobeDto> Data { get; set; } = new();
    }

    // DTO for getting clothes filtered by type
    public class ClothesFilterDto
    {
        public string ClothingType { get; set; } = ""; // "top", "bottom", "full"
    }
}