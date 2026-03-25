using AutoMapper;
using CatalogApp.Infrastructure.Entities;
using CatalogApp.Shared.Dto;
using System;
using System.Collections.Generic;
using System.Text;

namespace CatalogApp.Infrastructure.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Entity -> DTO (для чтения)
            CreateMap<ProductEntity, ProductDto>()
                .ForMember(dest => dest.HasImage,
                           opt => opt.MapFrom(src => src.ImageData != null && src.ImageData.Length > 0));

            CreateMap<UserEntity, UserDto>();

            // DTO -> Entity (для записи обычных полей, не трогаем BLOB)
            CreateMap<ProductDto, ProductEntity>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ImageData, opt => opt.Ignore())
                .ForMember(dest => dest.ImageContentType, opt => opt.Ignore())
                .ForMember(dest => dest.ImageFileName, opt => opt.Ignore());

            CreateMap<UserDto, UserEntity>()
                .ForMember(dest => dest.Id, opt => opt.Ignore());
        }
    }
}
