using Newtonsoft.Json;

namespace SimpleBlogAPI.Models.DTOs.Post
{
    public class PostDto : PostDtoOutputBase
    {
        [JsonProperty(Order = 2)]
        public string Category { get; set; } = null!;
    }
}
