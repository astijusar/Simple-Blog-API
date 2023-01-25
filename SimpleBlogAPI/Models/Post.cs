using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SimpleBlogAPI.Models
{
    public class Post
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "Post title is a required field.")]
        public string Title { get; set; } = null!;
        [Required(ErrorMessage = "Post slug is a required field.")]
        public string Slug { get; set; } = null!;
        public string? Summary { get; set; }
        [Required(ErrorMessage = "Post content is a required field.")]
        public string Content { get; set; } = null!;
        [Required(ErrorMessage = "IsPublished is a required field.")]
        public bool IsPublished { get; set; }
        [Required(ErrorMessage = "Post creation date required.")]
        public DateTime CreatedOn { get; set; }
        [Required(ErrorMessage = "Post modification date required.")]
        public DateTime LastModifiedOn { get; set; }

        [ForeignKey(nameof(Category))]
        public int CategoryId { get; set; }
        public virtual Category Category { get; set; } = null!;
        public virtual ICollection<Comment> Comments { get; set; } = null!;
    }
}
