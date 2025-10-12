using AutoServiceCatalog.BLL.DTO;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoServiceCatalog.BLL.Validation
{
    public class PartSupplierCreateDtoValidator : AbstractValidator<PartSupplierDto>
    {
        public PartSupplierCreateDtoValidator()
        {
            RuleFor(ps => ps.PartId)
                .GreaterThan(0).WithMessage("PartId must be valid");

            RuleFor(ps => ps.SupplierId)
                .GreaterThan(0).WithMessage("SupplierId must be valid");
        }
    }
}
