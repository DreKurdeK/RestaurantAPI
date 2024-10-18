using System.ComponentModel.DataAnnotations;

namespace RestaurantAPI.Models
{
    public class UpdateRestaurantDto
    {
        [Required]
        [MaxLength(25)]
        public required string Name { get; set; }
        public string? Description { get; set; }
        public bool? HasDelivery { get; set; }
    }
}
