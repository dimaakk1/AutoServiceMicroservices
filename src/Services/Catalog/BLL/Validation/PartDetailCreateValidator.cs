using AutoServiceCatalog.BLL.DTO;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoServiceCatalog.BLL.Validation
{
    public class ServiceDetailCreateDtoValidator : AbstractValidator<ServiceDetailCreateDto>
    {
        public ServiceDetailCreateDtoValidator()
        {
            RuleFor(sd => sd.ServiceId)
                .GreaterThan(0).WithMessage("ServiceId must be valid");
        }
    }
}
