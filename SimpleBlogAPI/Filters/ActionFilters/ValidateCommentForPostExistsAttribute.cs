using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using SimpleBlogAPI.Models;

namespace SimpleBlogAPI.Filters.ActionFilters
{
    public class ValidateCommentForPostExistsAttribute : IAsyncActionFilter
    {
        private readonly BlogContext _blogContext;
        private readonly ILogger<ValidateCommentForPostExistsAttribute> _logger;

        public ValidateCommentForPostExistsAttribute(BlogContext blogContext, ILogger<ValidateCommentForPostExistsAttribute> logger)
        {
            _blogContext = blogContext;
            _logger = logger;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var method = context.HttpContext.Request.Method;
            var trackChanges = (method.Equals("PUT") || method.Equals("PATCH")) ? true : false;

            var postId = (int)context.ActionArguments["postId"]!;
            var post = await _blogContext.Posts.AsNoTracking()
                .SingleOrDefaultAsync(p => p.Id == postId);

            if (post == null)
            {
                _logger.LogWarning($"Post with id: {postId} doesn't exist in the database.");
                context.Result = new NotFoundResult();
            }

            var id = (int)context.ActionArguments["commentId"]!;

            var comment = trackChanges ?
                await _blogContext.Comments.FindAsync(id) :
                await _blogContext.Comments.AsNoTracking()
                    .SingleOrDefaultAsync(c => c.Id == id && c.PostId == postId);

            if (comment == null)
            {
                _logger.LogWarning($"Comment with id: {id} doesn't exist in the database.");
                context.Result = new NotFoundResult();
            }
            else
            {
                context.HttpContext.Items.Add("comment", comment);
                await next();
            }
        }
    }
}
