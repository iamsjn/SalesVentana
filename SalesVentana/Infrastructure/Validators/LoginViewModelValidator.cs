using FluentValidation;

namespace SalesVentana
{
    public class LoginViewModelValidator : AbstractValidator<LoginViewModel>
    {
        public LoginViewModelValidator()
        {
            RuleFor(x => x.Username).NotEmpty().WithMessage("Invalid Username");
            RuleFor(x => x.Password).NotEmpty().WithMessage("Invalid Password");
        }
    }
}