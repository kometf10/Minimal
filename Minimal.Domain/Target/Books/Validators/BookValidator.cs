using FluentValidation;
using Minimal.Domain.Target.Books.Dtos;
using static Minimal.Domain.Core.Validators.Message;

namespace Minimal.Domain.Target.Books.Validators
{
    public class BookValidator : AbstractValidator<BookDto>
    {
        public BookValidator()
        {
            RuleFor(b => b.Title).NotEmpty().WithMessage(Messages.REQUIRED);
            RuleFor(b => b.Author).NotEmpty().WithMessage(Messages.REQUIRED);
        }
    }
}
