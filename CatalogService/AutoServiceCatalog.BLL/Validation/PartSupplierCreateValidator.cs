using AutoServiceCatalog.BLL.DTO;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoServiceCatalog.BLL.Validation
{
    public class ServiceSupplierCreateDtoValidator : AbstractValidator<ServiceSupplierDto>
    {
        public ServiceSupplierCreateDtoValidator()
        {
            RuleFor(ss => ss.ServiceId)
                .GreaterThan(0).WithMessage("ServiceId must be valid");

            RuleFor(ss => ss.SupplierId)
                .GreaterThan(0).WithMessage("SupplierId must be valid");
        }
    }
}
