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

        /// <summary>
        /// Get a list of all categories
        /// </summary>
        /// <returns>The categories list</returns>
        /// <response code="200">Returns the categories list</response>
        [HttpGet]
        [ProducesResponseType(200)]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _blogContext.Categories.AsNoTracking()
                .ToListAsync();

            var categoriesDto = _mapper.Map<IEnumerable<CategoryDto>>(categories);

            return Ok(categoriesDto);
        }

        /// <summary>
        /// Get the category by id
        /// </summary>
        /// <param name="categoryId">Category id</param>
        /// <returns>Category found by id</returns>\
        /// <response code="200">Returns the category found by id</response>
        /// <response code="404">The category does not exist</response>
        [HttpGet("{categoryId}", Name = "GetCategoryById")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
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

        /// <summary>
        /// Get a list of categories by ids
        /// </summary>
        /// <param name="ids">Ids of categories</param>
        /// <returns>The categories list</returns>
        /// <response code="200">Returns the categories found by ids</response>
        /// <response code="400">Parameter ids is null</response>
        /// <response code="404">Some ids are not valid in a collection</response>
        [HttpGet("collection/({ids})", Name = "GetCategoryCollection")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
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

        /// <summary>
        /// Creates a new category
        /// </summary>
        /// <param name="input">Category object to create</param>
        /// <returns>A newly created category</returns>
        /// <response code="201">Returns a newly created category</response>
        /// <response code="400">Category input object sent from client is null</response>
        /// <response code="422">Invalid model state for the category input object</response>
        [HttpPost]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(422)]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        public async Task<IActionResult> CreateCategory([FromBody] CategoryCreationDto input)
        {
            var category = _mapper.Map<Category>(input);

            _blogContext.Categories.Add(category);
            await _blogContext.SaveChangesAsync();

            var categoryDto = _mapper.Map<CategoryDto>(category);

            return CreatedAtRoute("GetCategoryById", new { categoryId = categoryDto.Id }, categoryDto);
        }

        /// <summary>
        /// Creates multiple new categories
        /// </summary>
        /// <param name="input">Collection of category objects to create</param>
        /// <returns>Newly created categories</returns>
        /// <response code="201">Returns newly created categories</response>
        /// <response code="400">Categories collection input object sent from client is null</response>
        /// <response code="422">Invalid model state for the category input object</response>
        [HttpPost("collection")]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(422)]
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

        /// <summary>
        /// Deletes a category by id
        /// </summary>
        /// <param name="categoryId">Category id</param>
        /// <returns>204 no content response</returns>
        /// <response code="204">Returns 204 no content response</response>
        /// <response code="404">The category does not exist</response>
        [HttpDelete("{categoryId}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [ServiceFilter(typeof(ValidateCategoryExistsAttribute))]
        public async Task<IActionResult> DeleteCategory(int categoryId)
        {
            var category = HttpContext.Items["category"] as Category;

            _blogContext.Categories.Remove(category!);
            await _blogContext.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Update the whole category by id
        /// </summary>
        /// <param name="categoryId">Category id</param>
        /// <param name="input">Category object with updates</param>
        /// <returns>An updated category</returns>
        /// <response code="200">Returns 204 no content response</response>
        /// <response code="404">The category does not exist</response>
        /// <response code="400">Category input is null</response>
        /// <response code="422">Invalid model state for the category input</response>
        [HttpPut("{categoryId}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        [ProducesResponseType(422)]
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

        /// <summary>
        /// Partially update category by id
        /// </summary>
        /// <param name="categoryId">Category id</param>
        /// <param name="patchInput">Category object with partial updates</param>
        /// <returns>Partially updated category</returns>
        /// <response code="204">Returns 204 no content response</response>
        /// <response code="400">Category input object sent from client is null</response>
        /// <response code="422">Invalid model state for the category input object</response>
        /// <response code="404">Category does not exist</response>
        [HttpPatch("{categoryId}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(422)]
        [ProducesResponseType(404)]
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
