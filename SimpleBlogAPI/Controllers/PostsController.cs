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

        /// <summary>
        /// Get a list of posts for category
        /// </summary>
        /// <param name="categoryId">Category id</param>
        /// <returns>A list of posts</returns>
        /// <response code="200">Returns a list of posts</response>
        /// <response code="404">Category does not exist</response>
        [HttpGet]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
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

        /// <summary>
        /// Get a list of all posts regardless of category
        /// </summary>
        /// <returns>A list of all posts</returns>
        /// <response code="200">Returns a list of posts</response>
        [HttpGet]
        [ProducesResponseType(200)]
        public async Task<IActionResult> GetPosts()
        {
            var allPosts = await _blogContext.Posts.AsNoTracking()
                .Include(p => p.Category)
                .ToListAsync();

            var allPostsDto = _mapper.Map<IEnumerable<PostDto>>(allPosts);

            return Ok(allPostsDto);
        }

        /// <summary>
        /// Get a post by id
        /// </summary>
        /// <param name="postId">post id</param>
        /// <returns>A post</returns>
        /// <response code="200">Returns a posts</response>
        /// <response code="404">Post does not exist</response>
        [HttpGet("{postId}", Name = "GetPostById")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
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

        /// <summary>
        /// Get a list of posts by ids
        /// </summary>
        /// <param name="ids">Ids of posts</param>
        /// <returns>A list of posts</returns>
        /// <response code="200">Returns a list of posts</response>
        /// <response code="404">Some ids are not valid in a collection</response>
        /// <response code="400">Parameter ids is null</response>
        [HttpGet("collection/({ids})", Name = "GetPostCollection")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
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

        /// <summary>
        /// Create a post for category
        /// </summary>
        /// <param name="categoryId">Category id</param>
        /// <param name="input">Post input object</param>
        /// <returns>The newly created post</returns>
        /// <response code="201">Returns the newly created post</response>
        /// <response code="422">Invalid model state for post input object</response>
        /// <response code="400">Post input object is null</response>
        /// <response code="404">Category does not exist</response>
        [HttpPost]
        [ProducesResponseType(201)]
        [ProducesResponseType(422)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
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

        /// <summary>
        /// Create multiple posts for category
        /// </summary>
        /// <param name="categoryId">Category id</param>
        /// <param name="input">Post input object</param>
        /// <returns>The newly created posts</returns>
        /// <response code="201">Returns the newly created posts</response>
        /// <response code="422">Invalid model state for post input object(s)</response>
        /// <response code="400">Post input objects are null</response>
        /// <response code="404">Category does not exist</response>
        [HttpPost]
        [ProducesResponseType(201)]
        [ProducesResponseType(422)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
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

        /// <summary>
        /// Delete a post by id
        /// </summary>
        /// <param name="postId">Post id</param>
        /// <returns>204 no content response</returns>
        /// <response code="204">Returns no content response</response>
        /// <response code="404">Post does not exist</response>
        [HttpDelete("{postId}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [ServiceFilter(typeof(ValidatePostExistsAttribute))]
        public async Task<IActionResult> DeletePost(int postId)
        {
            var post = HttpContext.Items["post"] as Post;

            _blogContext.Posts.Remove(post!);
            await _blogContext.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Update a post by id
        /// </summary>
        /// <param name="postId">Post id</param>
        /// <param name="input">Post input object</param>
        /// <returns>204 no content response</returns>
        /// <response code="204">Returns no content response</response>
        /// <response code="404">Post does not exist</response>
        /// <response code="422">Invalid model state for post input object</response>
        /// <response code="400">Post input object is null</response>
        [HttpPut("{postId}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [ProducesResponseType(422)]
        [ProducesResponseType(400)]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        [ServiceFilter(typeof(ValidatePostExistsAttribute))]
        public async Task<IActionResult> UpdatePost(int postId, [FromBody] PostUpdateDto input)
        {
            var post = HttpContext.Items["post"] as Post;

            _mapper.Map(input, post);
            await _blogContext.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Partially update post by id
        /// </summary>
        /// <param name="postId">Post id</param>
        /// <param name="patchInput">Post input object</param>
        /// <returns>204 no content response</returns>
        /// <response code="204">Returns no content response</response>
        /// <response code="404">Post does not exist</response>
        /// <response code="400">Post input object sent from client is null</response>
        /// <response code="422">Invalid model state for the post input object</response>
        [HttpPatch("{postId}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        [ProducesResponseType(422)]
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
