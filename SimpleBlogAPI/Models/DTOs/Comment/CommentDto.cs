namespace SimpleBlogAPI.Models.DTOs.Comment
{
    public class CommentDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Content { get; set; } = null!;
        public string PostedBy { get; set; } = null!;
        public DateTime CreatedOn { get; set; }
    }
}
