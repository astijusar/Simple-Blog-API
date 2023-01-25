using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using SimpleBlogAPI.Models;

namespace SimpleBlogAPI.Filters.ActionFilters
{
    public class ValidateCategoryExistsAttribute : IAsyncActionFilter
    {
        private readonly BlogContext _blogContext;
        private readonly ILogger<ValidateCategoryExistsAttribute> _logger;

        public ValidateCategoryExistsAttribute(BlogContext blogContext, ILogger<ValidateCategoryExistsAttribute> logger)
        {
            _blogContext = blogContext;
            _logger = logger;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var method = context.HttpContext.Request.Method;
            var trackChanges = (method.Equals("PUT") || method.Equals("PATCH")) ? true : false;
            var id = (int)context.ActionArguments["categoryId"]!;

            var category = trackChanges ?
                await _blogContext.Categories.FindAsync(id) :
                await _blogContext.Categories.AsNoTracking()
                    .SingleOrDefaultAsync(p => p.Id == id);

            if (category == null)
            {
                _logger.LogWarning($"Category with id: {id} doesn't exist in the database.");
                context.Result = new NotFoundResult();
            }
            else
            {
                context.HttpContext.Items.Add("category", category);
                await next();
            }
        }
    }
}
