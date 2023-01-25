﻿namespace SimpleBlogAPI.Models.DTOs.Category
{
    public class CategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
