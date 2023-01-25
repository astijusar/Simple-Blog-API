using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SimpleBlogAPI.Filters.ActionFilters;
using SimpleBlogAPI.Models;
using SimpleBlogAPI.Models.DTOs.Comment;
using SimpleBlogAPI.Models.ModelBinders;

namespace SimpleBlogAPI.Controllers
{
    [Route("api/posts/{postId}/[controller]")]
    [ApiController]
    public class CommentsController : ControllerBase
    {
        private readonly BlogContext _blogContext;
        private readonly IMapper _mapper;
        private readonly ILogger<CommentsController> _logger;

        public CommentsController(BlogContext blogContext, IMapper mapper, ILogger<CommentsController> logger)
        {
            _blogContext = blogContext;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Get a list of comments from a specific post
        /// </summary>
        /// <param name="postId">Post id</param>
        /// <returns>A list of comments</returns>
        /// <response code="200">Returns the comments</response>
        /// <response code="404">The post does not exist</response>
        [HttpGet]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ServiceFilter(typeof(ValidatePostExistsAttribute))]
        public async Task<IActionResult> GetCommentsForPost(int postId)
        {
            var comments = await _blogContext.Comments.AsNoTracking()
                .Where(c => c.PostId == postId)
                .ToListAsync();

            var commentsDto = _mapper.Map<IEnumerable<CommentDto>>(comments);

            return Ok(commentsDto);
        }

        /// <summary>
        /// Get a specific comment from a specific post
        /// </summary>
        /// <param name="postId">Post id</param>
        /// <param name="commentId">Comment id</param>
        /// <returns>Comment with matching id</returns>
        /// <response code="200">Returns the comment</response>
        /// <response code="404">The post or comment does not exist</response>
        [HttpGet("{commentId}", Name = "GetCommentForPost")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ServiceFilter(typeof(ValidatePostExistsAttribute))]
        public async Task<IActionResult> GetCommentForPost(int postId, int commentId)
        {
            var comment = await _blogContext.Comments.AsNoTracking()
                .SingleOrDefaultAsync(c => c.PostId == postId && c.Id == commentId);

            if (comment == null)
            {
                _logger.LogWarning($"Comment with id: {commentId} doesn't exist int the database.");
                return NotFound();
            }

            var commentDto = _mapper.Map<CommentDto>(comment);

            return Ok(commentDto);
        }

        /// <summary>
        /// Get a list of comments by ids
        /// </summary>
        /// <param name="ids">Ids of comments</param>
        /// <returns>A list of comments by ids</returns>
        /// <response code="200">Returns a list of comments</response>
        /// <response code="400">Parameter ids is null</response>
        /// <response code="404">Some ids are not valid in a collection</response>
        [HttpGet("collection/({ids})", Name = "GetCommentCollection")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetCommentCollection([ModelBinder(BinderType = typeof(ArrayModelBinder))] IEnumerable<int> ids)
        {
            if (ids == null)
            {
                _logger.LogWarning("Parameter ids is null");
                return BadRequest("Parameter ids is null");
            }

            var comments = await _blogContext.Comments.AsNoTracking()
                .Where(c => ids.Contains(c.Id))
                .ToListAsync();

            if (ids.Count() != comments.Count())
            {
                _logger.LogWarning("Some ids are not valid in a collection");
                return NotFound();
            }

            var commentsDto = _mapper.Map<IEnumerable<CommentDto>>(comments);

            return Ok(commentsDto);
        }

        /// <summary>
        /// Create comment for post
        /// </summary>
        /// <param name="postId">Post id</param>
        /// <param name="input">Comment input object</param>
        /// <returns>Newly created comment</returns>
        /// <response code="201">Returns the newly created comment</response>
        /// <response code="422">Invalid model state for comment input object</response>
        /// <response code="400">Comment input object is null</response>
        /// <response code="404">Post does not exist</response>
        [HttpPost]
        [ProducesResponseType(201)]
        [ProducesResponseType(422)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        [ServiceFilter(typeof(ValidatePostExistsAttribute))]
        public async Task<IActionResult> CreateCommentForPost(int postId, [FromBody] CommentCreationDto input)
        {
            var comment = _mapper.Map<Comment>(input);
            comment.PostId = postId;

            _blogContext.Comments.Add(comment);
            await _blogContext.SaveChangesAsync();

            var commentDto = _mapper.Map<CommentDto>(comment);

            return CreatedAtRoute("GetCommentForPost", new { postId = postId, commentId = commentDto.Id }, commentDto);
        }

        /// <summary>
        /// Delete comment by id
        /// </summary>
        /// <param name="postId">post id</param>
        /// <param name="commentId">comment id</param>
        /// <returns>204 no content response</returns>
        /// <response code="204">Returns no content response</response>
        /// <response code="404">Post or comment does not exist</response>
        [HttpDelete("{commentId}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [ServiceFilter(typeof(ValidateCommentForPostExistsAttribute))]
        public async Task<IActionResult> DeleteCommentForPost(int postId, int commentId)
        {
            var comment = HttpContext.Items["comment"] as Comment;

            _blogContext.Comments.Remove(comment!);
            await _blogContext.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Update comment for post by id
        /// </summary>
        /// <param name="postId">Post id</param>
        /// <param name="commentId">Comment id</param>
        /// <param name="input">Comment input object</param>
        /// <returns>204 no content response</returns>
        /// <response code="204">Returns no content response</response>
        /// <response code="422">Invalid model state for comment input object</response>
        /// <response code="400">Comment input object is null</response>
        /// <response code="404">Post or comment does not exist</response>
        [HttpPut("{commentId}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(422)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        [ServiceFilter(typeof(ValidateCommentForPostExistsAttribute))]
        public async Task<IActionResult> UpdateCommentForPost(int postId, int commentId, [FromBody] CommentUpdateDto input)
        {
            var comment = HttpContext.Items["comment"] as Comment;

            _mapper.Map(input, comment);
            await _blogContext.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Partially update comment for post by id
        /// </summary>
        /// <param name="postId">Post id</param>
        /// <param name="commentId">Comment id</param>
        /// <param name="patchInput">Comment input object</param>
        /// <returns>204 no content response</returns>
        /// <response code="204">Returns no content response</response>
        /// <response code="404">Post or comment does not exist</response>
        /// <response code="400">Comment input object sent from client is null</response>
        /// <response code="422">Invalid model state for the comment input object</response>
        [HttpPatch("{commentId}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        [ProducesResponseType(422)]
        [ServiceFilter(typeof(ValidateCommentForPostExistsAttribute))]
        public async Task<IActionResult> PartiallyUpdateCommentForPost(int postId, int commentId, 
            [FromBody] JsonPatchDocument<CommentUpdateDto> patchInput)
        {
            if (patchInput == null)
            {
                _logger.LogWarning("CommentUpdateDto patch object sent from client is null.");
                return BadRequest("Comment patch object is null");
            }

            var comment = HttpContext.Items["comment"] as Comment;

            var commentToPatch = _mapper.Map<CommentUpdateDto>(comment);

            patchInput.ApplyTo(commentToPatch);

            TryValidateModel(commentToPatch);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for the CommentUpdateDto patch object");
                return UnprocessableEntity(ModelState);
            }

            _mapper.Map(commentToPatch, comment);

            await _blogContext.SaveChangesAsync();

            return NoContent();
        }
    }
}
