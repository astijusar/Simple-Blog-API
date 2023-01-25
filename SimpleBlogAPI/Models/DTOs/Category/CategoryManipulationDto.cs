using System.ComponentModel.DataAnnotations;

namespace SimpleBlogAPI.Models.DTOs.Category
{
    public abstract class CategoryManipulationDto
    {
        [Required(ErrorMessage = "Category name is a required field.")]
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
    }
}
