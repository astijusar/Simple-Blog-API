using System.ComponentModel.DataAnnotations;
using SimpleBlogAPI.Models.DTOs.Post;

namespace SimpleBlogAPI.Models.DTOs.Category
{
    public class CategoryCreationDto : CategoryManipulationDto
    {
        public IEnumerable<PostCreationDto> Posts { get; set; }
    }
}
