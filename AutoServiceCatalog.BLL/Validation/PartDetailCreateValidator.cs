using AutoServiceCatalog.BLL.DTO;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoServiceCatalog.BLL.Validation
{
    public class PartDetailCreateDtoValidator : AbstractValidator<PartDetailCreateDto>
    {
        public PartDetailCreateDtoValidator()
        {
            RuleFor(pd => pd.PartId)
                .GreaterThan(0).WithMessage("PartId must be valid");
        }
    }
}
