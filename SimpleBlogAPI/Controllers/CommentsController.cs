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

        [HttpGet]
        [ServiceFilter(typeof(ValidatePostExistsAttribute))]
        public async Task<IActionResult> GetCommentsForPost(int postId)
        {
            var comments = await _blogContext.Comments.AsNoTracking()
                .Where(c => c.PostId == postId)
                .ToListAsync();

            var commentsDto = _mapper.Map<IEnumerable<CommentDto>>(comments);

            return Ok(commentsDto);
        }

        [HttpGet("{commentId}", Name = "GetCommentForPost")]
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

        [HttpGet("collection/({ids})", Name = "GetCommentCollection")]
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

        [HttpPost]
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

        [HttpDelete("{commentId}")]
        [ServiceFilter(typeof(ValidateCommentForPostExistsAttribute))]
        public async Task<IActionResult> DeleteCommentForPost(int postId, int commentId)
        {
            var comment = HttpContext.Items["comment"] as Comment;

            _blogContext.Comments.Remove(comment!);
            await _blogContext.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut("{commentId}")]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        [ServiceFilter(typeof(ValidateCommentForPostExistsAttribute))]
        public async Task<IActionResult> UpdateCommentForPost(int postId, int commentId, [FromBody] CommentUpdateDto input)
        {
            var comment = HttpContext.Items["comment"] as Comment;

            _mapper.Map(input, comment);
            await _blogContext.SaveChangesAsync();

            return NoContent();
        }

        [HttpPatch("{commentId}")]
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
