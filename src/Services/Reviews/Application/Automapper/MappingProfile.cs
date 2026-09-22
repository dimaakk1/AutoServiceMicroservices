using Application.Commands;
using Application.DTO;
using AutoMapper;
using Domain.Entities;
using Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Automapper
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Review, ReviewDto>().ReverseMap();
            CreateMap<Review, ReviewDto>()
                .ForMember(dest => dest.Rating, opt => opt.MapFrom(src => src.Rating.Value));
        }
    }
}
