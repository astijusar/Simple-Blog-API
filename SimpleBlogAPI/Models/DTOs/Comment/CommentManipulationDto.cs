using System.ComponentModel.DataAnnotations;

namespace SimpleBlogAPI.Models.DTOs.Comment
{
    public class CommentManipulationDto
    {
        [Required(ErrorMessage = "Comment title is a required field.")]
        public string Title { get; set; } = null!;
        [Required(ErrorMessage = "Comment content is a required field.")]
        public string Content { get; set; } = null!;
        [Required(ErrorMessage = "Comment postedBy is a required field.")]
        public string PostedBy { get; set; } = null!;
    }
}
