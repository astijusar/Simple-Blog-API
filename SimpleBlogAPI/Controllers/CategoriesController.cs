using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileSystemGlobbing.Internal;
using SimpleBlogAPI.Filters.ActionFilters;
using SimpleBlogAPI.Models;
using SimpleBlogAPI.Models.DTOs.Category;
using SimpleBlogAPI.Models.ModelBinders;
using Swashbuckle.AspNetCore.Annotations;

namespace SimpleBlogAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly BlogContext _blogContext;
        private readonly IMapper _mapper;
        private readonly ILogger<CategoriesController> _logger;

        public CategoriesController(BlogContext blogContext, IMapper mapper, ILogger<CategoriesController> logger)
        {
            _blogContext = blogContext;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _blogContext.Categories.AsNoTracking()
                .ToListAsync();

            var categoriesDto = _mapper.Map<IEnumerable<CategoryDto>>(categories);

            return Ok(categoriesDto);
        }

        [HttpGet("{categoryId}", Name = "GetCategoryById")]
        public async Task<IActionResult> GetCategory(int categoryId)
        {
            var category = await _blogContext.Categories.AsNoTracking()
                .SingleOrDefaultAsync(c => c.Id == categoryId);

            if (category == null)
            {
                _logger.LogWarning($"Category with id: {categoryId} doesn't exist in the database.");
                return NotFound();
            }

            var categoryDto = _mapper.Map<CategoryDto>(category);

            return Ok(categoryDto);
        }

        [HttpGet("collection/({ids})", Name = "GetCategoryCollection")]
        public async Task<IActionResult> GetCategoryCollection([ModelBinder(BinderType = typeof(ArrayModelBinder))] IEnumerable<int> ids)
        {
            if (ids == null)
            {
                _logger.LogWarning("Parameter ids is null");
                return BadRequest("Parameter ids is null");
            }

            var categories = await _blogContext.Categories.AsNoTracking()
                .Where(c => ids.Contains(c.Id))
                .ToListAsync();

            if (ids.Count() != categories.Count())
            {
                _logger.LogWarning("Some ids are not valid in a collection");
                return NotFound();
            }

            var categoriesDto = _mapper.Map<IEnumerable<CategoryDto>>(categories);

            return Ok(categoriesDto);
        }

        [HttpPost]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        public async Task<IActionResult> CreateCategory([FromBody] CategoryCreationDto input)
        {
            var category = _mapper.Map<Category>(input);

            _blogContext.Categories.Add(category);
            await _blogContext.SaveChangesAsync();

            var categoryDto = _mapper.Map<CategoryDto>(category);

            return CreatedAtRoute("GetCategoryById", new { categoryId = categoryDto.Id }, categoryDto);
        }

        [HttpPost("collection")]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        public async Task<IActionResult> CreateCategoryCollection([FromBody] IEnumerable<CategoryCreationDto> input)
        { 
            var categories = _mapper.Map<IEnumerable<Category>>(input);
            foreach (var category in categories)
            {
                _blogContext.Categories.Add(category);
            }

            await _blogContext.SaveChangesAsync();

            var categoriesDto = _mapper.Map<IEnumerable<CategoryDto>>(categories);
            var ids = string.Join(",", categoriesDto.Select(p => p.Id));

            return CreatedAtRoute("GetCategoryCollection", new { ids }, categoriesDto);
        }

        [HttpDelete("{categoryId}")]
        [ServiceFilter(typeof(ValidateCategoryExistsAttribute))]
        public async Task<IActionResult> DeleteCategory(int categoryId)
        {
            var category = HttpContext.Items["category"] as Category;

            _blogContext.Categories.Remove(category);
            await _blogContext.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut("{categoryId}")]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        [ServiceFilter(typeof(ValidateCategoryExistsAttribute))]
        public async Task<IActionResult> UpdateCategory(int categoryId, [FromBody] CategoryUpdateDto input)
        {
            var category = HttpContext.Items["category"] as Category;

            _mapper.Map(input, category);
            await _blogContext.SaveChangesAsync();

            var categoryDto = _mapper.Map<CategoryDto>(category);

            return Ok(categoryDto);
        }

        [HttpPatch("{categoryId}")]
        [ServiceFilter(typeof(ValidateCategoryExistsAttribute))]
        public async Task<IActionResult> PartiallyUpdateCategory(int categoryId, [FromBody] JsonPatchDocument<CategoryUpdateDto> patchInput)
        {
            if (patchInput == null)
            {
                _logger.LogError("CategoryUpdateDto patch object sent from client is null.");
                return BadRequest("Category patch object is null");
            }

            var category = HttpContext.Items["category"] as Category;

            var categoryToPatch = _mapper.Map<CategoryUpdateDto>(category);

            patchInput.ApplyTo(categoryToPatch);

            TryValidateModel(categoryToPatch);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for the CategoryUpdateDto patch object");
                return UnprocessableEntity(ModelState);
            }

            _mapper.Map(categoryToPatch, category);

            await _blogContext.SaveChangesAsync();

            return NoContent();
        }
    }
}
