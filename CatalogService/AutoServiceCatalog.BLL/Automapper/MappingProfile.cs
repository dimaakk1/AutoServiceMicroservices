using AutoMapper;
using AutoServiceCatalog.BLL.DTO;
using AutoServiceCatalog.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoServiceCatalog.BLL.Automapper
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Category, CategoryDto>().ReverseMap();
            CreateMap<Service, ServiceDto>().ReverseMap();
            CreateMap<Service, ServiceCreateDto>().ReverseMap();
            CreateMap<ServiceCreateDto, Service>();
            CreateMap<Supplier, SupplierDto>().ReverseMap();
            CreateMap<Supplier, SupplierCreateDto>().ReverseMap();
            CreateMap<ServiceDetail, ServiceDetailDto>().ReverseMap();
            CreateMap<ServiceDetailCreateDto, ServiceDetail>().ReverseMap();
            CreateMap<ServiceSupplier, ServiceSupplierDto>().ReverseMap();
        }

    }
}
