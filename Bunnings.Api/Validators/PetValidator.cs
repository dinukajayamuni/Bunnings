using Bunnings.Api.Models;
using FluentValidation;

namespace Bunnings.Api.Validators
{
    public class PetValidator : AbstractValidator<Pet>
    {
        public PetValidator()
        {
            RuleFor(pet => pet.Name).NotNull();
            RuleFor(person => person.Type).NotEmpty().IsInEnum();
        }
    }
}