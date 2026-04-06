using AutoServiceCatalog.BLL.DTO;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoServiceCatalog.BLL.Validation
{
    public class ServiceCreateDtoValidator : AbstractValidator<ServiceCreateDto>
    {
        public ServiceCreateDtoValidator()
        {
            RuleFor(s => s.Name)
                .NotEmpty().WithMessage("Service name is required")
                .MaximumLength(200).WithMessage("Name cannot exceed 200 characters");

            RuleFor(s => s.Price)
                .GreaterThanOrEqualTo(0).WithMessage("Price must be non-negative");

            RuleFor(s => s.CategoryId)
                .GreaterThan(0).WithMessage("CategoryId must be valid");
        }
    }
}
