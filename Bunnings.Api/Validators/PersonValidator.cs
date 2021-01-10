using System.Linq;
using Bunnings.Api.Models;
using FluentValidation;

namespace Bunnings.Api.Validators
{
    public class PersonValidator : AbstractValidator<Person>
    {
        public PersonValidator()
        {
            RuleFor(person => person.Email).NotEmpty().EmailAddress();
            RuleFor(person => person.FirstName).NotEmpty();
            RuleFor(person => person.LastName).NotEmpty();
            RuleFor(person => person.Gender).NotEmpty().IsInEnum();
            RuleFor(person => person.Pets).Must(x => x.Any()).WithMessage("There must be at least one pet")
                .When(person => person.Pets != null);
            RuleForEach(person => person.Pets).SetValidator(new PetValidator())
                .When(person => person.Pets != null);
        }
    }
}
