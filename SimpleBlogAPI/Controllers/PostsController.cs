using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using SimpleBlogAPI.Filters.ActionFilters;
using SimpleBlogAPI.Models;
using SimpleBlogAPI.Models.DTOs.Post;
using SimpleBlogAPI.Models.ModelBinders;

namespace SimpleBlogAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostsController : ControllerBase
    {
        private readonly BlogContext _blogContext;
        private readonly IMapper _mapper;
        private readonly ILogger<PostsController> _logger;

        public PostsController(BlogContext blogContext, IMapper mapper, ILogger<PostsController> logger)
        {
            _blogContext = blogContext;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpGet]
        [Route("~/api/categories/{categoryId}/[controller]")]
        public async Task<IActionResult> GetPostsForCategory(int categoryId) 
        {
            var category = await _blogContext.Categories.AsNoTracking()
                .SingleOrDefaultAsync(c => c.Id == categoryId);

            if (category == null)
            {
                _logger.LogWarning($"Category with id: {categoryId} doesn't exist int the database.");
                return NotFound();
            }

            var posts = await _blogContext.Posts.AsNoTracking()
                .Where(p => p.CategoryId == categoryId)
                .ToListAsync();

            var postsDto = _mapper.Map<IEnumerable<CategoryPostDto>>(posts);

            return Ok(postsDto);
        }

        [HttpGet]
        public async Task<IActionResult> GetPosts()
        {
            var allPosts = await _blogContext.Posts.AsNoTracking()
                .Include(p => p.Category)
                .ToListAsync();

            var allPostsDto = _mapper.Map<IEnumerable<PostDto>>(allPosts);

            return Ok(allPostsDto);
        }

        [HttpGet("{postId}", Name = "GetPostById")]
        public async Task<IActionResult> GetPost(int postId)
        {
            var post = await _blogContext.Posts.AsNoTracking()
                .Include(p => p.Category)
                .SingleOrDefaultAsync(p => p.Id == postId);

            if (post == null)
            {
                _logger.LogWarning($"Post with id: {postId} doesn't exist in the database.");
                return NotFound();
            }

            var postDto = _mapper.Map<PostDto>(post);
            return Ok(postDto);
        }

        [HttpGet("collection/({ids})", Name = "GetPostCollection")]
        public async Task<IActionResult> GetPostCollection([ModelBinder(BinderType = typeof(ArrayModelBinder))] IEnumerable<int> ids)
        {
            if (ids == null)
            {
                _logger.LogWarning("Parameter ids is null");
                return BadRequest("Parameter ids is null");
            }

            var posts = await _blogContext.Posts.AsNoTracking()
                .Where(p => ids.Contains(p.Id))
                .Include(p => p.Category)
                .ToListAsync();

            if (ids.Count() != posts.Count())
            {
                _logger.LogWarning("Some ids are not valid in a collection");
                return NotFound();
            }

            var postsDto = _mapper.Map<IEnumerable<PostDto>>(posts);

            return Ok(postsDto);
        }

        [HttpPost]
        [Route("~/api/categories/{categoryId}/[controller]")]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        public async Task<IActionResult> CreatePost(int categoryId, [FromBody] PostCreationDto input)
        { 
            var category = await _blogContext.Categories.AsNoTracking()
                .SingleOrDefaultAsync(c => c.Id == categoryId);

            if (category == null)
            {
                _logger.LogWarning($"Category with id: {categoryId} doesn't exist int the database.");
                return NotFound();
            }

            var post = _mapper.Map<Post>(input);
            post.CategoryId = categoryId;

            _blogContext.Posts.Add(post);
            await _blogContext.SaveChangesAsync();

            var createdPost = await _blogContext.Posts.AsNoTracking()
                .SingleOrDefaultAsync(p => p.Id == post.Id);

            var postToReturn = _mapper.Map<CategoryPostDto>(createdPost);

            return CreatedAtRoute("GetPostById", new { postId = postToReturn.Id }, postToReturn);
        }

        [HttpPost]
        [Route("~/api/categories/{categoryId}/[controller]/collection")]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        public async Task<IActionResult> CreatePostCollection(int categoryId, [FromBody] IEnumerable<PostCreationDto> input)
        {
            var category = await _blogContext.Categories.AsNoTracking()
                .SingleOrDefaultAsync(c => c.Id == categoryId);

            if (category == null)
            {
                _logger.LogWarning($"Category with id: {categoryId} doesn't exist int the database.");
                return NotFound();
            }

            var posts = _mapper.Map<IEnumerable<Post>>(input);

            foreach (var post in posts)
            {
                post.CategoryId = categoryId;

                _blogContext.Posts.Add(post);
            }
            await _blogContext.SaveChangesAsync();

            var createdPosts = await _blogContext.Posts.AsNoTracking()
                .Where(p => posts
                    .Select(p => p.Id)
                    .Contains(p.Id))
                .ToListAsync();

            var postsDto = _mapper.Map<IEnumerable<CategoryPostDto>>(createdPosts);
            var ids = string.Join(",", postsDto.Select(p => p.Id));

            return CreatedAtRoute("GetPostCollection", new { ids }, postsDto);
        }

        [HttpDelete("{postId}")]
        [ServiceFilter(typeof(ValidatePostExistsAttribute))]
        public async Task<IActionResult> DeletePost(int postId)
        {
            var post = HttpContext.Items["post"] as Post;

            _blogContext.Posts.Remove(post!);
            await _blogContext.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut("{postId}")]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        [ServiceFilter(typeof(ValidatePostExistsAttribute))]
        public async Task<IActionResult> UpdatePost(int postId, [FromBody] PostUpdateDto input)
        {
            var post = HttpContext.Items["post"] as Post;

            _mapper.Map(input, post);
            await _blogContext.SaveChangesAsync();

            return NoContent();
        }

        [HttpPatch("{postId}")]
        [ServiceFilter(typeof(ValidatePostExistsAttribute))]
        public async Task<IActionResult> PartiallyUpdatePost(int postId, [FromBody] JsonPatchDocument<PostUpdateDto> patchInput)
        {
            if (patchInput == null)
            {
                _logger.LogWarning("PostUpdateDto patch object sent from client is null.");
                return BadRequest("Post patch object is null");
            }

            var post = HttpContext.Items["post"] as Post;

            var postToPatch = _mapper.Map<PostUpdateDto>(post);

            patchInput.ApplyTo(postToPatch);

            TryValidateModel(postToPatch);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for the PostUpdateDto patch object");
                return UnprocessableEntity(ModelState);
            }

            _mapper.Map(postToPatch, post);

            await _blogContext.SaveChangesAsync();

            return NoContent();
        }
    }
}
