using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;

namespace SalesVentana
{
    public class LoginViewModel : IValidatableObject
    {
        public string Username { get; set; }
        public string Password { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var validator = new LoginViewModelValidator();
            var result = validator.Validate(this);
            return result.Errors.Select(x=> new ValidationResult(x.ErrorMessage, new[] { x.PropertyName}));
        }
    }
}