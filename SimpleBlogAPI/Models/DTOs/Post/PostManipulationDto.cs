using System.ComponentModel.DataAnnotations;

namespace SimpleBlogAPI.Models.DTOs.Post
{
    public abstract class PostManipulationDto
    {
        [Required(ErrorMessage = "Post title is a required field.")]
        public string Title { get; set; } = null!;
        [Required(ErrorMessage = "Post slug is a required field.")]
        public string Slug { get; set; } = null!;
        public string? Summary { get; set; }
        [Required(ErrorMessage = "Post content is a required field.")]
        public string Content { get; set; } = null!;
        public bool IsPublished { get; set; }
    }
}
