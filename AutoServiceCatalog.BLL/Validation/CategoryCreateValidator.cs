using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoServiceCatalog.BLL.DTO;
using FluentValidation;

namespace AutoServiceCatalog.BLL.Validation
{ 
    public class CategoryCreateDtoValidator : AbstractValidator<CategoryDto>
    {
        public CategoryCreateDtoValidator()
        {
            RuleFor(c => c.Name)
                .NotEmpty().WithMessage("Name is required")
                .MaximumLength(100).WithMessage("Name cannot exceed 100 characters");
        }
    }
}
