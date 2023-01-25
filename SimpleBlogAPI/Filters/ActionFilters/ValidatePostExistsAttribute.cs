using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using SimpleBlogAPI.Models;

namespace SimpleBlogAPI.Filters.ActionFilters
{
    public class ValidatePostExistsAttribute : IAsyncActionFilter
    {
        private readonly BlogContext _blogContext;
        private readonly ILogger<ValidatePostExistsAttribute> _logger;

        public ValidatePostExistsAttribute(BlogContext blogContext, ILogger<ValidatePostExistsAttribute> logger)
        {
            _blogContext = blogContext;
            _logger = logger;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var method = context.HttpContext.Request.Method;
            var trackChanges = (method.Equals("PUT") || method.Equals("PATCH")) ? true : false;
            var id = (int)context.ActionArguments["postId"]!;

            var post = trackChanges ? 
                await _blogContext.Posts.FindAsync(id) : 
                await _blogContext.Posts.AsNoTracking()
                    .SingleOrDefaultAsync(p => p.Id == id);

            if (post == null)
            {
                _logger.LogWarning($"Post with id: {id} doesn't exist in the database.");
                context.Result = new NotFoundResult();
            }
            else
            {
                context.HttpContext.Items.Add("post", post);
                await next();
            }
        }
    }
}
