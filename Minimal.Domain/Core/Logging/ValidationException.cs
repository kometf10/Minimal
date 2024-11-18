using Minimal.Domain.Core.Responces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minimal.Domain.Core.Logging
{
    public class ValidationException : Exception
    {
        public string? Field { get; set; }
        public List<ValidationResult> ValidationErrors { get; set; }

        public ValidationException(string message) : base(message)
        {
            var validation = new ValidationResult();
            validation.Field = "Validation";
            validation.Errors = new List<string> { message };

            ValidationErrors = new List<ValidationResult>() { validation };

        }

        public ValidationException(string field, string message) : base(message)
        {
            Field = field;

            var validation = new ValidationResult();
            validation.Field = field;
            validation.Errors = new List<string> { message };

            ValidationErrors = new List<ValidationResult>() { validation };
        }

        public ValidationException(List<ValidationResult> validationErrors) : base("Validation Exception")
        {
            ValidationErrors = validationErrors;
        }

        public ValidationException(List<string> errors)
        {
            var validation = new ValidationResult();
            validation.Field = "Validation";
            validation.Errors = errors;

            ValidationErrors = new List<ValidationResult>() { validation };
        }
    }
}
