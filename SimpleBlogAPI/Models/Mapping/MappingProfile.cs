using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SimpleBlogAPI.Models.DTOs.Category;
using SimpleBlogAPI.Models.DTOs.Comment;
using SimpleBlogAPI.Models.DTOs.Post;

namespace SimpleBlogAPI.Models.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Post, PostDto>()
                .ForMember(dest => dest.Category,
                    opt => opt.MapFrom(src => src.Category.Name));
            CreateMap<Post, CategoryPostDto>();
            CreateMap<PostCreationDto, Post>();
            CreateMap<PostUpdateDto, Post>()
                .ForMember(dest => dest.CategoryId,
                    opt => opt.MapFrom(src => src.CategoryId))
                .ReverseMap();

            CreateMap<Comment, CommentDto>();
            CreateMap<CommentCreationDto, Comment>();
            CreateMap<CommentUpdateDto, Comment>().ReverseMap();

            CreateMap<Category, CategoryDto>();
            CreateMap<CategoryCreationDto, Category>();
            CreateMap<CategoryUpdateDto, Category>().ReverseMap();
        }
    }
}
