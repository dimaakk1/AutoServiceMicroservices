using AutoServiceCatalog.BLL.DTO;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoServiceCatalog.BLL.Validation
{
    public class SupplierCreateDtoValidator : AbstractValidator<SupplierCreateDto>
    {
        public SupplierCreateDtoValidator()
        {
            RuleFor(s => s.Name)
                .NotEmpty().WithMessage("Supplier name is required")
                .MaximumLength(150).WithMessage("Name cannot exceed 150 characters");

            RuleFor(s => s.Phone)
                .MaximumLength(300).WithMessage("Contact info cannot exceed 300 characters");
        }
    }
}
