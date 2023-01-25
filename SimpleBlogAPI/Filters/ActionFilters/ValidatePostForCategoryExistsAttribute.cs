using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using SimpleBlogAPI.Models;

namespace SimpleBlogAPI.Filters.ActionFilters
{
    public class ValidatePostForCategoryExistsAttribute : IAsyncActionFilter
    {
        private readonly BlogContext _blogContext;
        private readonly ILogger<ValidatePostForCategoryExistsAttribute> _logger;

        public ValidatePostForCategoryExistsAttribute(BlogContext blogContext, ILogger<ValidatePostForCategoryExistsAttribute> logger)
        {
            _blogContext = blogContext;
            _logger = logger;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var method = context.HttpContext.Request.Method;
            var trackChanges = (method.Equals("PUT") || method.Equals("PATCH")) ? true : false;

            var categoryId = (int)context.ActionArguments["categoryId"]!;
            var category = await _blogContext.Categories.AsNoTracking()
                .SingleOrDefaultAsync(c => c.Id == categoryId);

            if (category == null)
            {
                _logger.LogWarning($"Category with id: {categoryId} doesn't exist int the database.");
                context.Result = new NotFoundResult();
            }

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
