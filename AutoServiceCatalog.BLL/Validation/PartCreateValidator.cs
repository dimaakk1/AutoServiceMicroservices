using AutoServiceCatalog.BLL.DTO;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoServiceCatalog.BLL.Validation
{
    public class PartCreateDtoValidator : AbstractValidator<PartCreateDto>
    {
        public PartCreateDtoValidator()
        {
            RuleFor(p => p.Name)
                .NotEmpty().WithMessage("Part name is required")
                .MaximumLength(200).WithMessage("Name cannot exceed 200 characters");

            RuleFor(p => p.Price)
                .GreaterThanOrEqualTo(0).WithMessage("Price must be non-negative");

            RuleFor(p => p.CategoryId)
                .GreaterThan(0).WithMessage("CategoryId must be valid");
        }
    }
}
