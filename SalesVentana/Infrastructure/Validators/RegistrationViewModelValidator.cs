using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SalesVentana
{
    public class RegistrationViewModelValidator : AbstractValidator<RegistrationViewModel>
    {
        public RegistrationViewModelValidator()
        {
            RuleFor(x => x.Username).NotEmpty().WithMessage("Invalid Username");
            RuleFor(x => x.Password).NotEmpty().WithMessage("Invalid Password");
            RuleFor(x => x.Email).NotEmpty().WithMessage("Invalid Email Address");  
        }
    }
}