using SimpleBlogAPI.Models.DTOs.Comment;

namespace SimpleBlogAPI.Models.DTOs.Post
{
    public class PostUpdateDto : PostManipulationDto
    {
        public int CategoryId { get; set; }
        public IEnumerable<CommentCreationDto> Comments { get; set; }
    }
}
