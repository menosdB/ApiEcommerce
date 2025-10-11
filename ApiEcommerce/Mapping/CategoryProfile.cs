using System;
using ApiEcommerce.Model.Dtos;
using AutoMapper;

namespace ApiEcommerce.Mapping;

public class CategoryProfile : Profile
{
    public CategoryProfile()
    {
        CreateMap<Category, CategoryDto>().ReverseMap();
        CreateMap<Category, CreateCategoryDto>().ReverseMap();
    }
}
